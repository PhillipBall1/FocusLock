using System.ComponentModel;

namespace FocusLock.ViewModels
{
    /*
     * BaseViewModel implements INotifyPropertyChanged to provide
     * property change notification support for data binding in WPF.
     * ViewModels that inherit from this class can easily notify the UI
     * when a property value changes.
     */

    public class BaseViewModel : INotifyPropertyChanged
    {
        // Event raised when a property value changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise PropertyChanged event.
        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
