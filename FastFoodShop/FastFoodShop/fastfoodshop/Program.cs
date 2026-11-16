using Avalonia;
using System;

namespace fastfoodshop
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Подключение к базе данных проверяется на форме авторизации,
            // чтобы окно входа открывалось без задержки при недоступном сервере.
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
