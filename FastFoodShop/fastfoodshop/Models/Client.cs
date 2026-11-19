using System.ComponentModel.DataAnnotations.Schema;

namespace fastfoodshop.Models;

public class Client
{
    public int ClientId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public int? DiscountId { get; set; }

    public Discount? Discount { get; set; }

    [NotMapped]
    public string FullName => string.IsNullOrWhiteSpace(FirstName)
        ? LastName
        : $"{LastName} {FirstName}".Trim();
}
