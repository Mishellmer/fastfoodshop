using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using fastfoodshop.Data;

namespace fastfoodshop.Services
{
    public class SalesReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageCheck { get; set; }
    }

    public class ReportService
    {
        // Формирование сводного отчёта о продажах за период.
        public async Task<SalesReport> BuildReportAsync(DateTime from, DateTime to)
        {
            using var db = new AppDbContext();

            var orders = await db.Orders
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .ToListAsync();

            var completed = orders.Where(o => o.Status == "выполнен").ToList();
            var totalRevenue = completed.Sum(o => o.TotalSum);

            return new SalesReport
            {
                From = from,
                To = to,
                OrderCount = completed.Count,
                TotalRevenue = totalRevenue,
                AverageCheck = completed.Count > 0
                    ? decimal.Round(totalRevenue / completed.Count, 2, MidpointRounding.AwayFromZero)
                    : 0
            };
        }
    }
}
