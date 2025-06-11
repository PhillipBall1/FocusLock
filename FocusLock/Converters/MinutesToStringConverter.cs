using System;
using System.Globalization;
using System.Windows.Data;

namespace FocusLock.Converters
{
    public class MinutesToStringConverter : IValueConverter
    {
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

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
