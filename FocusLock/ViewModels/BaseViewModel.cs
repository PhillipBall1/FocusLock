using System.ComponentModel;

namespace FocusLock.ViewModels
{
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
