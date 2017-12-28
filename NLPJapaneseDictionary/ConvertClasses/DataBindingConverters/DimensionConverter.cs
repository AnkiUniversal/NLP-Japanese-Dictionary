using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NLPJDict.ConvertClasses.DataBindingConverters
{
    public class DimensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = System.Convert.ToDouble(value);
            var divide = System.Convert.ToDouble(parameter);
            return size / divide;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = System.Convert.ToDouble(value);
            var divide = System.Convert.ToDouble(parameter);
            return size * divide;
        }
    }
}
