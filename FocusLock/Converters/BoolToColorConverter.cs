using FocusLock.Service;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FocusLock.Converters
{
    // Converts a boolean value to a SolidColorBrush (used in XAML for styling)
    public class BoolToColorConverter : IValueConverter
    {
        // Converts a bool to a color brush: light red if true, light gray if false
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush white = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            SolidColorBrush dark = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242424"));
            bool flag = value is bool b && b;
            return flag
                ? new SolidColorBrush(Color.FromRgb(255, 200, 200))
                : Properties.Settings.Default.IsDarkMode ? dark : white; 
        }

        // ConvertBack not implemented since this converter is one-way
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
