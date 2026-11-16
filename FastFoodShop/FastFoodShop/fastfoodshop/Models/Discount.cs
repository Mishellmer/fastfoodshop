namespace fastfoodshop.Models;

public class Discount
{
    public int DiscountId { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal MinOrderSum { get; set; }
}