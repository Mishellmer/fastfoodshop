using System.ComponentModel.DataAnnotations.Schema;

namespace fastfoodshop.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }

    public Role? Role { get; set; }

    // ФИО сотрудника для верхней панели главного окна.
    [NotMapped]
    public string FullName => $"{LastName} {FirstName}".Trim();

    // ФИО с отчеством для формы управления пользователями.
    [NotMapped]
    public string FullNameWithMiddleName => string.IsNullOrWhiteSpace(MiddleName)
        ? FullName
        : $"{LastName} {FirstName} {MiddleName}".Trim();
}
