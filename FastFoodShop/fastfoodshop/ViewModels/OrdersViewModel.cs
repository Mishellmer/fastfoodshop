using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using fastfoodshop.Models;
using fastfoodshop.Services;

namespace fastfoodshop.ViewModels
{
    // История заказов с поиском по номеру и дате, закрытием и отменой заказов.
    public class OrdersViewModel : ViewModelBase
    {
        private readonly OrderService _orderService = new();

        private Order? _selectedOrder;
        private string _orderNumberText = string.Empty;
        private DateTime? _from = DateTime.Today.AddDays(-7);
        private DateTime? _to = DateTime.Today;
        private string _statusMessage = "Задайте фильтры и нажмите «Найти».";

        public ObservableCollection<Order> Orders { get; } = new();

        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        public string OrderNumberText
        {
            get => _orderNumberText;
            set => SetProperty(ref _orderNumberText, value);
        }

        public DateTime? From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime? To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand CloseOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }

        public OrdersViewModel()
        {
            SearchCommand = new RelayCommand(LoadOrders);
            CloseOrderCommand = new RelayCommand(() => ChangeStatus("выполнен"));
            CancelOrderCommand = new RelayCommand(() => ChangeStatus("отменён"));

            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                int? orderNumber = null;
                var numberText = (OrderNumberText ?? string.Empty).Trim();

                if (numberText.Length > 0)
                {
                    if (!int.TryParse(numberText, out var parsed) || parsed <= 0)
                        throw new InvalidOperationException("Номер заказа должен быть целым положительным числом.");

                    orderNumber = parsed;
                }

                if (From.HasValue && To.HasValue && From.Value.Date > To.Value.Date)
                    throw new InvalidOperationException("Дата начала периода больше даты окончания.");

                var orders = _orderService.Search(orderNumber, From, To);

                Orders.Clear();
                foreach (var order in orders)
                {
                    Orders.Add(order);
                }

                SelectedOrder = null;
                StatusMessage = orders.Count == 0
                    ? "Заказы за указанный период не найдены."
                    : $"Найдено заказов: {orders.Count}.";
            }
            catch (Exception ex)
            {
                Orders.Clear();
                SelectedOrder = null;
                StatusMessage = "Ошибка поиска: " + GetErrorMessage(ex);
            }
        }

        private void ChangeStatus(string status)
        {
            if (SelectedOrder == null)
            {
                StatusMessage = "Сначала выберите заказ в списке.";
                return;
            }

            var orderId = SelectedOrder.OrderId;

            try
            {
                _orderService.ChangeStatus(orderId, status);

                LoadOrders();
                SelectedOrder = Orders.FirstOrDefault(o => o.OrderId == orderId);

                StatusMessage = status == "выполнен"
                    ? $"Заказ №{orderId} закрыт (выполнен)."
                    : $"Заказ №{orderId} отменён.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка изменения статуса: " + GetErrorMessage(ex);
            }
        }

        private static string GetErrorMessage(Exception ex)
        {
            var current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }
    }
}
