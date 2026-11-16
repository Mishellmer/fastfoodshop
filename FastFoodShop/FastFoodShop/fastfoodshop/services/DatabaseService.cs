using System;
using System.IO;
using fastfoodshop.Data;
using Microsoft.EntityFrameworkCore;

namespace fastfoodshop.Services
{
    /// Проверка доступности базы данных для отображения статуса подключения.
    public static class DatabaseService
    {
        public static bool CanConnect(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                using var db = new AppDbContext();

                var connection = db.Database.GetDbConnection();
                connection.Open();
                connection.Close();

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.InnerException?.Message ?? ex.Message;
                LogError(ex, "CanConnect");
                return false;
            }
        }

        public static string GetStatusText()
            => CanConnect(out _) ? "БД: ПОДКЛЮЧЕНО" : "БД: НЕТ СВЯЗИ";

        //Запись полного текста ошибки в db_error.log рядом с приложением.
        public static void LogError(Exception ex, string context)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "db_error.log");
                File.AppendAllText(path,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // логирование не должно влиять на работу приложения
            }
        }
    }
}
