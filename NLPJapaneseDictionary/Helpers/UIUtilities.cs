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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NLPJapaneseDictionary.Helpers
{
    public static class UIUtilities
    {
        public static SolidColorBrush DarkerGray { get; private set; }
        = Application.Current.Resources["DarkerGray"] as SolidColorBrush;
        public static SolidColorBrush MoreDarkerGray { get; private set; }
         = Application.Current.Resources["MoreDarkerGray"] as SolidColorBrush;
        public static SolidColorBrush LighterGray { get; private set; }
               = Application.Current.Resources["LighterGray"] as SolidColorBrush;
        public static SolidColorBrush MoreLighterGray { get; private set; }
               = Application.Current.Resources["MoreLighterGray"] as SolidColorBrush;
        public static SolidColorBrush WhiteForeGround { get; private set; }
               = Application.Current.Resources["WhiteForeGround"] as SolidColorBrush;
        public static SolidColorBrush WhiteBackground { get; private set; }
               = Application.Current.Resources["WhiteBackground"] as SolidColorBrush;
        public static SolidColorBrush BlackForeGround { get; private set; }
               = Application.Current.Resources["BlackForeGround"] as SolidColorBrush;
        public static SolidColorBrush BlackBackGround { get; private set; }
               = Application.Current.Resources["BlackBackGround"] as SolidColorBrush;
        public static SolidColorBrush Orange { get; private set; } = new SolidColorBrush(Colors.Orange);
        public static SolidColorBrush DodgerBlue { get; private set; } = new SolidColorBrush(Colors.DodgerBlue);
        public static SolidColorBrush Green { get; private set; } = new SolidColorBrush(Colors.Green);

        public static Color OceanBlue { get; private set; } = (Color)Application.Current.Resources["OceanBlueColor"];

        public static MessageBoxResult AskUserPermission(string message, string title = "")
        {
           return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public static void ShowMessageDialog(string message, string title = "")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ChangeReadMode(UserControl userControl, bool isNightMode)
        {
            if (isNightMode)
            {
                userControl.Background = BlackBackGround;
                userControl.Foreground = WhiteForeGround;
            }
            else
            {
                userControl.Background = WhiteBackground;
                userControl.Foreground = BlackForeGround;
            }
        }

        public static void ChangeReadMode(Page page, bool isNightMode)
        {
            if (isNightMode)
            {
                page.Background = BlackBackGround;
                page.Foreground = WhiteForeGround;
            }
            else
            {
                page.Background = WhiteBackground;
                page.Foreground = BlackForeGround;
            };
        }

        public static string GetTextFromClipboard(DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText)
                return null;
            return e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
        }

        public static ScrollViewer TryScrollWithScrollViewer(DependencyObject listView)
        {
            var child = (VisualTreeHelper.GetChild(listView, 0));
            if (child == null)
                return null;

            var border = child as Border;
            if (border == null || border.Child == null)
                return null;

            var scrollViewer = border.Child as ScrollViewer;
            if (scrollViewer == null)
                return null;

            return scrollViewer;
        }
    }
}
