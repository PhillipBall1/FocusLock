using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace FocusLock.Controls
{
    // Partial class for the custom Header user control
    public partial class Header : UserControl
    {
        // Constructor initializes the component
        public Header()
        {
            InitializeComponent();
        }

        // Event handler for minimizing the window when the minimize button is clicked
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        // Event handler for toggling the window between maximized and normal state
        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
        }

        // Event handler for closing the window when the close button is clicked
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }

        // Enables dragging of the window when the header area is clicked and dragged
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var window = Window.GetWindow(this);
                window?.DragMove(); // Initiates window drag operation
            }
        }
    }
}
