using System.Windows;
using System.Windows.Controls;

namespace FocusLock.Controls
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;
            ((MainWindow)Application.Current.MainWindow).ShowMainApp();
            /*
            if (UserManager.Authenticate(username, password))
            {
                ((MainWindow)Application.Current.MainWindow).ShowMainApp();
            }
            else
            {
                MessageBox.Show("Invalid credentials.");
            }
            */
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).ShowRegistration();
        }
    }
}
