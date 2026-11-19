using Avalonia.Controls;
using fastfoodshop.Models;
using fastfoodshop.ViewModels;

namespace fastfoodshop.Views
{
    public partial class UsersWindow : Window
    {
        public UsersWindow()
        {
            InitializeComponent();
        }

        public UsersWindow(Employee currentUser) : this()
        {
            DataContext = new UsersViewModel(currentUser);
        }
    }
}
