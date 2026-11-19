using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace fastfoodshop.Models;

public class OrderItem : INotifyPropertyChanged
{
    private int _quantity;
    private decimal _priceAtOrder;

    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineSum));
        }
    }

    public decimal PriceAtOrder
    {
        get => _priceAtOrder;
        set
        {
            if (_priceAtOrder == value) return;
            _priceAtOrder = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineSum));
        }
    }

    public MenuItem? MenuItem { get; set; }
    public Order? Order { get; set; }

    // Стоимость позиции заказа.
    [NotMapped]
    public decimal LineSum => Quantity * PriceAtOrder;

    [NotMapped]
    public string ItemName => MenuItem?.ItemName ?? $"Позиция №{MenuItemId}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
