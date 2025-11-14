using FocusLock.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusLock.Service
{
    public static class SettingsService
    {
        // General Settings
        public static bool IsDarkMode;
        public static bool LaunchOnStartup;
        public static bool MinimizeToTray;
        public static bool ShowNotifications;

        public static void LoadSettings()
        {
            IsDarkMode = Properties.Settings.Default.IsDarkMode;
            LaunchOnStartup = Properties.Settings.Default.LaunchOnStartup;
            MinimizeToTray = Properties.Settings.Default.MinimizeToTray;
            ShowNotifications = Properties.Settings.Default.ShowNotifications;

            ThemeHelper.ChangeTheme(IsDarkMode);
        }
    }
}
