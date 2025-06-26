using System;
using System.Globalization;
using System.Windows.Data;

namespace FocusLock.Converters
{
    // Converts a TimeSpan to a 12-hour clock time string (e.g., "2:30 PM")
    public class TimeSpanToAmPmConverter : IValueConverter
    {
        // Convert TimeSpan to formatted string with AM/PM
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan time && time != TimeSpan.Zero)
            {
                // Add TimeSpan to today's date and format as h:mm tt (e.g., 2:30 PM)
                return DateTime.Today.Add(time).ToString("h:mm tt", CultureInfo.InvariantCulture);
            }

            // Return empty string if TimeSpan is zero or invalid
            return string.Empty;
        }

        // ConvertBack is not supported for this converter
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
