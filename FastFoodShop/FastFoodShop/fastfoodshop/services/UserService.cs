using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using fastfoodshop.Data;
using fastfoodshop.Models;

namespace fastfoodshop.Services
{
    // Операции администратора по управлению учётными записями сотрудников и назначению ролей.
    public class UserService
    {
        public List<Employee> GetAll()
        {
            using var db = new AppDbContext();
            return db.Employees
                .Include(e => e.Role)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToList();
        }

        public List<Role> GetRoles()
        {
            using var db = new AppDbContext();
            return db.Roles.OrderBy(r => r.RoleId).ToList();
        }

        public Employee Create(string lastName, string firstName, string? middleName,
            string login, string password, int roleId)
        {
            Validate(lastName, firstName, login, roleId);

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Для новой учётной записи задайте пароль.");

            if (password.Length < 4)
                throw new InvalidOperationException("Пароль должен содержать не менее 4 символов.");

            using var db = new AppDbContext();
            EnsureRoleExists(db, roleId);

            var normalizedLogin = login.Trim();

            if (db.Employees.Any(e => e.Login == normalizedLogin))
                throw new InvalidOperationException("Сотрудник с таким логином уже существует.");

            var employee = new Employee
            {
                LastName = lastName.Trim(),
                FirstName = firstName.Trim(),
                MiddleName = NormalizeMiddleName(middleName),
                Login = normalizedLogin,
                PasswordHash = AuthService.HashPassword(password),
                RoleId = roleId
            };

            db.Employees.Add(employee);
            db.SaveChanges();
            return employee;
        }

        public void Update(int employeeId, string lastName, string firstName, string? middleName,
            string login, string? newPassword, int roleId)
        {
            Validate(lastName, firstName, login, roleId);

            using var db = new AppDbContext();
            EnsureRoleExists(db, roleId);

            var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId)
                           ?? throw new InvalidOperationException("Сотрудник не найден.");

            var normalizedLogin = login.Trim();

            if (db.Employees.Any(e => e.Login == normalizedLogin && e.EmployeeId != employeeId))
                throw new InvalidOperationException("Сотрудник с таким логином уже существует.");

            employee.LastName = lastName.Trim();
            employee.FirstName = firstName.Trim();
            employee.MiddleName = NormalizeMiddleName(middleName);
            employee.Login = normalizedLogin;
            employee.RoleId = roleId;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword.Length < 4)
                    throw new InvalidOperationException("Пароль должен содержать не менее 4 символов.");

                employee.PasswordHash = AuthService.HashPassword(newPassword);
            }

            db.SaveChanges();
        }

        public void Delete(int employeeId)
        {
            using var db = new AppDbContext();

            var employee = db.Employees.FirstOrDefault(e => e.EmployeeId == employeeId)
                           ?? throw new InvalidOperationException("Сотрудник не найден.");

            db.Employees.Remove(employee);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                throw new InvalidOperationException(
                    "Нельзя удалить сотрудника: за ним закреплены оформленные заказы. " +
                    "Сначала переоформите или удалите эти заказы.");
            }
        }

        private static void Validate(string lastName, string firstName, string login, int roleId)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidOperationException("Укажите фамилию сотрудника.");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new InvalidOperationException("Укажите имя сотрудника.");

            if (string.IsNullOrWhiteSpace(login))
                throw new InvalidOperationException("Укажите логин сотрудника.");

            if (login.Trim().Length < 3 || login.Trim().Length > 50)
                throw new InvalidOperationException("Логин должен содержать от 3 до 50 символов.");

            if (roleId <= 0)
                throw new InvalidOperationException("Выберите роль сотрудника.");
        }

        private static void EnsureRoleExists(AppDbContext db, int roleId)
        {
            if (!db.Roles.Any(r => r.RoleId == roleId))
                throw new InvalidOperationException("Выбранная роль не найдена.");
        }

        private static string? NormalizeMiddleName(string? middleName)
            => string.IsNullOrWhiteSpace(middleName) ? null : middleName.Trim();
    }
}
