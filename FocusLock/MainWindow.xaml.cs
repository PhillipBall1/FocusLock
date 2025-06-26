using System.Windows;
using System.Windows.Media;

namespace FocusLock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.Resources["TextIconBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#556070"));
            DataContext = new MainViewModel();
            ShowMainApp();
            /*
            if (UserManager.IsUserLoggedIn())
            {
                ShowMainApp();
            }
            else
            {
                ShowLogin();
            }
             */
        }

        public void ShowLogin()
        {
            AppLayout.Visibility = Visibility.Collapsed;
            LoginOverlay.Visibility = Visibility.Visible;
        }

        public void ShowRegistration()
        {
            AppLayout.Visibility = Visibility.Collapsed;
            LoginOverlay.Visibility = Visibility.Collapsed;
        }

        public void ShowMainApp()
        {
            LoginOverlay.Visibility = Visibility.Collapsed;
            AppLayout.Visibility = Visibility.Visible;
        }
    }
}
