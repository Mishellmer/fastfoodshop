using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using fastfoodshop.Data;
using fastfoodshop.Models;

namespace fastfoodshop.Services
{
    // Оформление и обработка заказов: сохранение заказа, поиск в истории,
    // изменение статуса (закрытие и отмена).
    public class OrderService
    {
        public static readonly string[] Statuses = { "новый", "оплачен", "выполнен", "отменён" };

        public Order CreateOrder(int employeeId, int? clientId, IEnumerable<OrderItem> cartItems,
            decimal totalSum, string status = "оплачен")
        {
            if (employeeId <= 0)
                throw new InvalidOperationException("Не определён кассир, оформляющий заказ.");

            var items = cartItems?.ToList() ?? new List<OrderItem>();

            if (items.Count == 0)
                throw new InvalidOperationException("Нельзя сохранить пустой заказ.");

            if (items.Any(i => i.Quantity <= 0))
                throw new InvalidOperationException("Количество в позиции заказа должно быть больше нуля.");

            if (totalSum < 0)
                throw new InvalidOperationException("Сумма заказа не может быть отрицательной.");

            if (!Statuses.Contains(status))
                throw new InvalidOperationException("Недопустимый статус заказа.");

            using var db = new AppDbContext();

            var employeeExists = db.Employees.Any(e => e.EmployeeId == employeeId);
            if (!employeeExists)
                throw new InvalidOperationException("Кассир не найден в базе данных.");

            if (clientId.HasValue && clientId.Value > 0
                && !db.Clients.Any(c => c.ClientId == clientId.Value))
            {
                clientId = null;
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                ClientId = clientId,
                EmployeeId = employeeId,
                TotalSum = totalSum,
                Status = status,
                Items = items.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity,
                    PriceAtOrder = i.PriceAtOrder
                }).ToList()
            };

            db.Orders.Add(order);
            db.SaveChanges();
            return order;
        }

        // Поиск заказов в истории.

        public List<Order> Search(int? orderId, DateTime? fromDate, DateTime? toDate)
        {
            using var db = new AppDbContext();

            IQueryable<Order> query = db.Orders
                .Include(o => o.Client)
                .Include(o => o.Employee)
                .AsNoTracking();

            if (orderId.HasValue && orderId.Value > 0)
                query = query.Where(o => o.OrderId == orderId.Value);

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(o => o.OrderDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(o => o.OrderDate <= to);
            }

            return query
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderId)
                .ToList();
        }

        public void ChangeStatus(int orderId, string status)
        {
            if (!Statuses.Contains(status))
                throw new InvalidOperationException("Недопустимый статус заказа.");

            using var db = new AppDbContext();

            var order = db.Orders.FirstOrDefault(o => o.OrderId == orderId)
                        ?? throw new InvalidOperationException("Заказ не найден.");

            if (order.Status is "выполнен" or "отменён")
                throw new InvalidOperationException($"Заказ уже закрыт (статус «{order.Status}»).");

            order.Status = status;
            db.SaveChanges();
        }
    }
}
