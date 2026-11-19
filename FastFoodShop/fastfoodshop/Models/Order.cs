using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace fastfoodshop.Models;

public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public int? ClientId { get; set; }
    public int EmployeeId { get; set; }
    public decimal TotalSum { get; set; }
    public string Status { get; set; } = "новый";

    public Client? Client { get; set; }
    public Employee? Employee { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    // Клиент заказа для отображения в истории заказов.
    [NotMapped]
    public string ClientName => Client?.FullName ?? "Без карты";

    // Дата заказа.
    [NotMapped]
    public string OrderDateText => OrderDate.ToString("dd.MM.yyyy HH:mm");
}
