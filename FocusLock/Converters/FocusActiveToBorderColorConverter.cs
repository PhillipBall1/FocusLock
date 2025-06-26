using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FocusLock.Converters
{
    // Converts a boolean (focus mode state) into a colored border brush
    public class FocusActiveToBorderColorConverter : IValueConverter
    {
        // Predefined brushes for performance and reuse
        private static readonly Brush RedBorder = new SolidColorBrush(Color.FromRgb(200, 70, 70));
        private static readonly Brush BlueBorder = new SolidColorBrush(Color.FromRgb(100, 160, 220));

        // Returns red if true (focus active), blue if false
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isActive && isActive ? RedBorder : BlueBorder;
        }

        // Not used — binding is one-way
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
