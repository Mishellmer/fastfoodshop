using Avalonia.Controls;
using fastfoodshop.Models;
using fastfoodshop.ViewModels;

namespace fastfoodshop.Views
{
    public partial class UsersWindow : Window
    {
        /// <summary>Конструктор без параметров нужен загрузчику XAML (превью в конструкторе).</summary>
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
