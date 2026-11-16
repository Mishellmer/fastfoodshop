using Avalonia.Controls;
using fastfoodshop.ViewModels;

namespace fastfoodshop.Views
{
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
            DataContext = new ReportViewModel();
        }
    }
}
