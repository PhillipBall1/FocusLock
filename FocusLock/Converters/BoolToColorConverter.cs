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
            bool flag = value is bool b && b;
            return flag
                ? new SolidColorBrush(Color.FromRgb(255, 200, 200)) // Red-ish when true
                : new SolidColorBrush(Color.FromRgb(240, 240, 240)); // Gray-ish when false
        }

        // ConvertBack not implemented since this converter is one-way
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
