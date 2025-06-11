using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FocusLock.Converters
{
    public class FocusStateToBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is bool isFocusActive) || !(values[1] is bool isTaskRunning))
                return Brushes.LightBlue;

            if (isTaskRunning) return new SolidColorBrush(Color.FromRgb(255, 205, 210));
            return isFocusActive
                ? new SolidColorBrush(Color.FromRgb(255, 205, 210))
                : new SolidColorBrush(Color.FromRgb(187, 222, 251));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
