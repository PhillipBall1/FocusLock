using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FocusLock.Helper
{
    public class ThemeHelper
    {
        public static SolidColorBrush lightDarkPastelBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B8EAD"));
        public static SolidColorBrush lightLightBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
        public static SolidColorBrush lightDarkRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
        public static SolidColorBrush lightLightRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCDD2"));
        public static SolidColorBrush lightTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#556070"));
        public static SolidColorBrush lightBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8F8"));
        public static SolidColorBrush lightSubBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        public static SolidColorBrush lightBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));

        public static SolidColorBrush darkDarkPastelBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
        public static SolidColorBrush darkLightBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B8EAD"));
        public static SolidColorBrush darkDarkRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCDD2"));
        public static SolidColorBrush darkLightRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
        public static SolidColorBrush darkTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F8F8"));
        public static SolidColorBrush darkBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B2B2B"));
        public static SolidColorBrush darkSubBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242424"));
        public static SolidColorBrush darkBorderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F2F36"));

        public static void ChangeTheme(bool dark)
        {
            Application.Current.Resources["TextIconBrush"] = dark ? darkTextColor : lightTextColor;
            Application.Current.Resources["BackgroundBrush"] = dark ? darkBackgroundColor : lightBackgroundColor;
            Application.Current.Resources["SubBackgroundBrush"] = dark ? darkSubBackgroundColor : lightSubBackgroundColor;
            Application.Current.Resources["PastelDarkBlueBrush"] = dark ? darkDarkPastelBlue : lightDarkPastelBlue;
            Application.Current.Resources["PastelBlueBrush"] = dark ? darkLightBlue : lightLightBlue;
            Application.Current.Resources["PastelDarkRedBrush"] = dark ? darkDarkRed : lightDarkRed;
            Application.Current.Resources["PastelRedBrush"] = dark ? darkLightRed : lightLightRed;
            Application.Current.Resources["BorderBrush"] = dark ? darkBorderColor : lightBorderColor;
        }
    }
}
