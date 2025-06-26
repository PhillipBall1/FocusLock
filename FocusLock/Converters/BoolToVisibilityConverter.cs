using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FocusLock.Converters
{
    /*
     * This class is used to convert a boolean value into a WPF Visibility value. 
     * It makes it easy to show or hide UI elements in XAML based on a condition 
     * (e.g., true = visible, false = collapsed).
     */

    // Converts a boolean to Visibility (true = Visible, false = Collapsed)
    public class BoolToVisibilityConverter : IValueConverter
    {
        // Converts a bool to Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;

            // If not a bool, default to Collapsed
            return Visibility.Collapsed;
        }

        // Converts back from Visibility to bool (Visible = true, others = false)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility visibility) && visibility == Visibility.Visible;
        }
    }
}
