using Avalonia.Controls;
using fastfoodshop.ViewModels;

namespace fastfoodshop.Views
{
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();
            DataContext = new MenuViewModel();
        }
    }
}
