using System.ComponentModel.DataAnnotations.Schema;

namespace fastfoodshop.Models;

public class MenuItem
{
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }

    public Category? Category { get; set; }

    // Состояние позиции в стоп-листе для отображения в форме меню.
    [NotMapped]
    public string AvailabilityText => IsAvailable ? "В продаже" : "Стоп-лист";
}
