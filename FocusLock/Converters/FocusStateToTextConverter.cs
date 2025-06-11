using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FocusLock.Converters
{
    public class FocusStateToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || !(values[0] is bool isFocusActive) || !(values[1] is bool isTaskRunning))
                return "Start Focus";

            if (isTaskRunning) return "Locked\nTask in Progress";
            return isFocusActive ? "Stop Focus" : "Start Focus";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
