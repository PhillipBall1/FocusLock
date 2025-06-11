using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FocusLock.Converters
{
    public class FocusActiveToBorderColorConverter : IValueConverter
    {
        private static readonly Brush RedBorder = new SolidColorBrush(Color.FromRgb(200, 70, 70));
        private static readonly Brush BlueBorder = new SolidColorBrush(Color.FromRgb(100, 160, 220));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isActive && isActive ? RedBorder : BlueBorder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
