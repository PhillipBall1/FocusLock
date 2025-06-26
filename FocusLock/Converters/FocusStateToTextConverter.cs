using System;
using System.Globalization;
using System.Windows.Data;

namespace FocusLock.Converters
{
    // Converts focus/task state into a string label for the focus button
    public class FocusStateToTextConverter : IMultiValueConverter
    {
        // Converts [isFocusActive, isTaskRunning] into button text
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Fallback if values are missing or not valid booleans
            if (values.Length != 2 || !(values[0] is bool isFocusActive) || !(values[1] is bool isTaskRunning))
                return "Start Focus";

            // If a task is running, show a locked message
            if (isTaskRunning)
                return "Locked\nTask in Progress";

            // Show appropriate focus toggle text
            return isFocusActive ? "Stop Focus" : "Start Focus";
        }

        // Not used — one-way multi-binding only
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
