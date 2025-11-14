using FocusLock.Helper;
using FocusLock.Service;
using System.ComponentModel;

namespace FocusLock.ViewModels
{
    public class SettingsViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public bool IsDarkMode
        {
            get => Properties.Settings.Default.IsDarkMode;
            set
            {
                if (Properties.Settings.Default.IsDarkMode != value)
                {
                    Properties.Settings.Default.IsDarkMode = value;
                    ThemeHelper.ChangeTheme(value);
                    OnPropertyChanged();
                    Properties.Settings.Default.Save();
                }
            }
        }

        //public bool LaunchOnStartup
        //{
        //    get => Properties.Settings.Default.LaunchOnStartup;
        //    set
        //    {
        //        if (Properties.Settings.Default.LaunchOnStartup != value)
        //        {
        //            Properties.Settings.Default.LaunchOnStartup = value;
        //            OnPropertyChanged();
        //            Properties.Settings.Default.Save();
        //        }
        //    }
        //}
        
        //public bool MinimizeToTray
        //{
        //    get => Properties.Settings.Default.MinimizeToTray;
        //    set
        //    {
        //        if (Properties.Settings.Default.MinimizeToTray != value)
        //        {
        //            Properties.Settings.Default.MinimizeToTray = value;
        //            OnPropertyChanged();
        //            Properties.Settings.Default.Save();
        //        }
        //    }
        //}

        public bool ShowNotifications
        {
            get => Properties.Settings.Default.ShowNotifications;
            set
            {
                if (Properties.Settings.Default.ShowNotifications != value)
                {
                    Properties.Settings.Default.ShowNotifications = value;
                    OnPropertyChanged();
                    Properties.Settings.Default.Save();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
