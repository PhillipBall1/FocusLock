using FocusLock.ViewModels;
using System.Windows.Controls;

namespace FocusLock.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
