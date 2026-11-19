using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using fastfoodshop.Services;
using fastfoodshop.ViewModels;
using System;
using System.Threading.Tasks;

namespace fastfoodshop.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Opened += OnWindowOpened;
        }

        private async void OnWindowOpened(object? sender, EventArgs e)
        {
            await CheckDatabaseAsync();
        }

        // Проверка подключения к БД при старте (статус «ПОДКЛЮЧЕНО»).
        private async Task CheckDatabaseAsync()
        {
            var statusText = this.FindControl<TextBlock>("DbStatusText");
            if (statusText == null) return;

            statusText.Text = "БД: проверка подключения...";
            statusText.Foreground = Brushes.Gray;

            try
            {
                string connectionError = string.Empty;
                var isConnected = await Task.Run(() => DatabaseService.CanConnect(out connectionError));

                if (isConnected)
                {
                    statusText.Text = "БД: ПОДКЛЮЧЕНО";
                    statusText.Foreground = Brushes.Green;
                }
                else
                {
                    statusText.Text = "БД: НЕТ СВЯЗИ — " + connectionError;
                    statusText.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                statusText.Text = "БД: ОШИБКА ПОДКЛЮЧЕНИЯ — " + ex.Message;
                statusText.Foreground = Brushes.Red;
                DatabaseService.LogError(ex, "CheckDatabaseAsync");
            }
        }

        // Обработчик нажатия на кнопку
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginBox = this.FindControl<TextBox>("LoginTextBox");
            var passwordBox = this.FindControl<TextBox>("PasswordTextBox");
            var errorText = this.FindControl<TextBlock>("ErrorTextBlock");
            var loginButton = this.FindControl<Button>("LoginButton");

            var login = loginBox?.Text;
            var password = passwordBox?.Text;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                if (errorText != null) errorText.Text = "Заполните все поля!";
                return;
            }

            if (errorText != null) errorText.Text = string.Empty;
            if (loginButton != null) loginButton.IsEnabled = false;

            try
            {
                var authService = new AuthService();
                var user = await authService.AuthenticateAsync(login, password);

                if (user == null)
                {
                    if (errorText != null) errorText.Text = "Неверный логин или пароль!";
                    return;
                }

                var mainWindow = new MainWindow();

                mainWindow.DataContext = new MainViewModel(user);

                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                DatabaseService.LogError(ex, "LoginButton_Click");
                if (errorText != null)
                {
                    errorText.Text = "Ошибка подключения к БД: " +
                                     (ex.InnerException?.Message ?? ex.Message);
                }
            }
            finally
            {
                if (loginButton != null) loginButton.IsEnabled = true;
            }
        }

        // Вход по клавише Enter из полей логина и пароля.
        private void Credential_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender ?? this, e);
            }
        }
    }
}
