using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FocusLock.Converters
{
    // Converts focus state and task state into a background brush
    public class FocusStateToBackgroundConverter : IMultiValueConverter
    {
        // Converts [isFocusActive, isTaskRunning] into a color
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Default fallback color if values are missing or invalid
            if (values.Length != 2 || !(values[0] is bool isFocusActive) || !(values[1] is bool isTaskRunning))
                return Brushes.LightBlue;

            // If a task is running, use red-tinted background
            if (isTaskRunning)
                return new SolidColorBrush(Color.FromRgb(255, 205, 210)); // light red

            // If focus is active (but task isn't), also use red-tinted
            return isFocusActive
                ? new SolidColorBrush(Color.FromRgb(255, 205, 210)) // light red
                : new SolidColorBrush(Color.FromRgb(187, 222, 251)); // light blue
        }

        // Not used — one-way multi-binding
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
