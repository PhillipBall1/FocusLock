using System;
using System.Globalization;
using System.Windows.Data;

namespace FocusLock.Converters
{
    /*
     * This class inverts a boolean value. 
     * Useful in XAML when you need the opposite of a bool property, such as disabling a button when a value is true.
     */

    // Converts a boolean to its inverse (true → false, false → true)
    public class InverseBoolConverter : IValueConverter
    {
        // Returns the opposite of the input boolean
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : value;

        // Not used — one-way binding only
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
