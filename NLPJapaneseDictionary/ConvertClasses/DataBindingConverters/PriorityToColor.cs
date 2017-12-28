using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NLPJDict.ConvertClasses.DataBindingConverters
{
    public class PriorityToColor : IValueConverter
    {
        public static SolidColorBrush Convert(string priority)
        {
            switch (priority)
            {
                case "VERY HIGH":
                    return new SolidColorBrush(Colors.DodgerBlue);
                case "HIGH":
                    return new SolidColorBrush(Colors.RoyalBlue);
                case "COMMON":
                    return new SolidColorBrush(Colors.MediumBlue);
                case "QUITE LOW":
                    return new SolidColorBrush(Colors.Gray);
                case "LOW":
                    return new SolidColorBrush(Colors.Gray);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var priority = System.Convert.ToString(value);
            switch (priority)
            {
                case "VERY HIGH":
                    return new SolidColorBrush(Colors.DodgerBlue);
                case "HIGH":
                    return new SolidColorBrush(Colors.RoyalBlue);
                case "COMMON":
                    return new SolidColorBrush(Colors.MediumBlue);
                case "QUITE LOW":
                    return new SolidColorBrush(Colors.Gray);
                case "LOW":
                    return new SolidColorBrush(Colors.Gray);
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
