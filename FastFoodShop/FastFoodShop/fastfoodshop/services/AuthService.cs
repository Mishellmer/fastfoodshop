using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using fastfoodshop.Data;
using fastfoodshop.Models;

namespace fastfoodshop.Services
{
    public class AuthService
    {
        // Проверка учётных данных сотрудника. Пароль хранится в БД в виде хеша BCrypt,
        // поэтому введённый пароль проверяется методом BCrypt.Verify.
        public async Task<Employee?> AuthenticateAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return null;

            using var db = new AppDbContext();

            var employee = await db.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Login == login.Trim());

            if (employee == null) return null;

            if (!VerifyPassword(password, employee.PasswordHash))
                return null;

            // Демонстрационные учётные записи из скрипта наполнения содержат не хеш,
            // а условное значение (hash_admin и т. п.). После первого успешного входа
            // пароль автоматически переводится в хеш BCrypt.
            await UpgradeLegacyHashAsync(db, employee, password);

            return employee;
        }

        // Хеширование пароля алгоритмом BCrypt для новых и изменяемых учётных записей.
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// Проверка пароля по значению из базы. Поддерживает и хеши BCrypt,
        /// и прежние демонстрационные значения, чтобы существующая база оставалась работоспособной.
        public static bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(passwordHash))
                return false;

            if (IsBCryptHash(passwordHash))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, passwordHash);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    return false;
                }
            }

            return string.Equals(password, passwordHash, StringComparison.Ordinal);
        }

        private static bool IsBCryptHash(string value)
            => value.StartsWith("$2a$", StringComparison.Ordinal)
               || value.StartsWith("$2b$", StringComparison.Ordinal)
               || value.StartsWith("$2y$", StringComparison.Ordinal);

        private static async Task UpgradeLegacyHashAsync(AppDbContext db, Employee employee, string password)
        {
            if (IsBCryptHash(employee.PasswordHash))
                return;

            try
            {
                employee.PasswordHash = HashPassword(password);
                await db.SaveChangesAsync();
            }
            catch
            {
                // Если обновление хеша не удалось (например, нет прав на запись),
                // вход всё равно должен состояться.
            }
        }
    }
}
