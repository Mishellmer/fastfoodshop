using Avalonia.Controls;
using fastfoodshop.ViewModels;

namespace fastfoodshop.Views
{
    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            DataContext = new OrdersViewModel();
        }
    }
}
