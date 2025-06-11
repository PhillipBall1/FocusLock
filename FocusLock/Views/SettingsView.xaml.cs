using FocusLock.ViewModels;
using System.Windows.Controls;

namespace FocusLock.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
