using FocusLock.Service;
using FocusLock.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace FocusLock.Views
{
    public partial class FocusView : UserControl
    {
        // ViewModel instance for data binding
        private FocusViewModel viewModel;

        public FocusView()
        {
            InitializeComponent();
            viewModel = new FocusViewModel();
            DataContext = viewModel; // Bind ViewModel to View
        }

        // Called when user clicks the focus mode toggle button
        private async void FocusButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't start/stop focus mode if a task is currently running
            if (viewModel.IsTaskRunning) return;

            // Start focus mode if not active, else stop it
            if (!viewModel.IsFocusActive)
                await FocusService.StartAsync();
            else
                FocusService.Stop();

            // Notify UI that IsFocusActive property changed to update button state
            viewModel.OnPropertyChanged(nameof(viewModel.IsFocusActive));
        }
    }
}
