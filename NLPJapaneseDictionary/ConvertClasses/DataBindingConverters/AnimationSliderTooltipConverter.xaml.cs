using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NLPJDict.ConvertClasses.DataBindingConverters
{
    class AnimationSliderTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rate = System.Convert.ToDouble(value);
            rate = Math.Round(rate, 1);
            return String.Format("Playback Rate: {0}x", rate);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
