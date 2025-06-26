using System;
using System.Globalization;
using System.Windows.Data;

namespace FocusLock.Converters
{
    /*
     * This class converts a TimeSpan into a human-readable string showing hours and minutes.
     * For example, a TimeSpan of 90 minutes becomes "1 Hour 30 Minutes".
     * Useful for displaying durations in the UI in a friendly format.
     */

    // Converts a TimeSpan into a formatted string with hours and minutes
    public class MinutesToStringConverter : IValueConverter
    {
        // Converts TimeSpan to string like "1 Hour 30 Minutes"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                int totalMinutes = (int)timeSpan.TotalMinutes;
                int hours = totalMinutes / 60;
                int minutes = totalMinutes % 60;

                if (hours > 0 && minutes > 0)
                    return $"{hours} Hour{(hours > 1 ? "s" : "")} {minutes} Minute{(minutes > 1 ? "s" : "")}";
                else if (hours > 0)
                    return $"{hours} Hour{(hours > 1 ? "s" : "")}";
                else
                    return $"{minutes} Minute{(minutes > 1 ? "s" : "")}";
            }

            // Return empty string if input is not a TimeSpan
            return string.Empty;
        }

        // ConvertBack is not implemented because one-way binding is sufficient
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
