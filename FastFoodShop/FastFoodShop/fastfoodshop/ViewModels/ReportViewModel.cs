using System;
using System.Threading.Tasks;
using System.Windows.Input;
using fastfoodshop.Services;

namespace fastfoodshop.ViewModels
{
    // Формирование отчёта о продажах за произвольный период (менеджер, администратор).
    public class ReportViewModel : ViewModelBase
    {
        private readonly ReportService _reportService = new();

        private DateTime? _from = DateTime.Today.AddDays(-7);
        private DateTime? _to = DateTime.Today;
        private int _orderCount;
        private decimal _totalRevenue;
        private decimal _averageCheck;
        private string _statusMessage = "Укажите период и нажмите «Сформировать».";

        public DateTime? From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime? To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public int OrderCount
        {
            get => _orderCount;
            set => SetProperty(ref _orderCount, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        public decimal AverageCheck
        {
            get => _averageCheck;
            set => SetProperty(ref _averageCheck, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand GenerateCommand { get; }

        public ReportViewModel()
        {
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
        }

        private async Task GenerateAsync()
        {
            try
            {
                var from = (From ?? DateTime.Today).Date;
                var to = (To ?? DateTime.Today).Date;

                if (from > to)
                    throw new InvalidOperationException("Дата начала периода больше даты окончания.");

                var report = await _reportService.BuildReportAsync(from, to.AddDays(1).AddSeconds(-1));

                OrderCount = report.OrderCount;
                TotalRevenue = report.TotalRevenue;
                AverageCheck = report.AverageCheck;

                StatusMessage = report.OrderCount == 0
                    ? $"За период {from:dd.MM.yyyy} — {to:dd.MM.yyyy} выполненных заказов нет."
                    : $"Отчёт сформирован за период {from:dd.MM.yyyy} — {to:dd.MM.yyyy}.";
            }
            catch (Exception ex)
            {
                OrderCount = 0;
                TotalRevenue = 0;
                AverageCheck = 0;

                var current = ex;
                while (current.InnerException != null)
                {
                    current = current.InnerException;
                }

                StatusMessage = "Ошибка формирования отчёта: " + current.Message;
            }
        }
    }
}
