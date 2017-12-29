/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NLPJapaneseDictionary.ConvertClasses.DataBindingConverters
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
