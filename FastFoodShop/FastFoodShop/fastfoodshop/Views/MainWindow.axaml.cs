using Avalonia.Controls;
using Avalonia.Interactivity;
using fastfoodshop.ViewModels;

namespace fastfoodshop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private async void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) return;

            var window = new MenuWindow();
            await window.ShowDialog(this);

            // После изменения меню обновляем список блюд и стоп-лист в главном окне.
            viewModel.ReloadMenu();
        }

        private async void OpenReport_Click(object sender, RoutedEventArgs e)
        {
            await new ReportWindow().ShowDialog(this);
        }

        private async void OpenOrders_Click(object sender, RoutedEventArgs e)
        {
            await new OrdersWindow().ShowDialog(this);
        }

        private async void OpenUsers_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) return;

            await new UsersWindow(viewModel.CurrentUser).ShowDialog(this);
        }
    }
}
