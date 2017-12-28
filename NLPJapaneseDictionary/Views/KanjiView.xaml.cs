/**
 * Copyright © 2010-2017 Atilika Inc. and contributors (see CONTRIBUTORS.md)
 * 
 * Modifications copyright (C) 2017 - 2018 Anki Universal Team <ankiuniversal@gmail.com>
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

using NLPJapaneseDictionary.Helpers;
using NLPJDict.ConvertClasses.DataBindingConverters;
using NLPJDict.NLPJDictCore.DatabaseTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NLPJapaneseDictionary.Views
{
    public sealed partial class KanjiView : UserControl
    {
        private KanjiDict kanji;

        Regex pathRegex = new Regex(@"(<path .*?/>)", RegexOptions.Compiled | RegexOptions.Singleline);
        Regex dataRegex = new Regex("( d=\".*?\")", RegexOptions.Compiled | RegexOptions.Singleline);

        private bool isNightMode = false;

        private const double STROKE_THICKNESS = 2;
        private const double GRID_MAXHEIGHT = 110;
        private const double GRID_MAXWIDTH = 110;

        private static readonly Thickness borderThickNess = new Thickness(1);
        private static readonly Thickness gridMargin = new Thickness(3,0,0,3);        
        private static readonly SolidColorBrush strokeBrush = new SolidColorBrush(UIUtilities.OceanBlue);
        private static readonly SolidColorBrush lastStrokeBrush = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush borderBrush = new SolidColorBrush(Colors.Gray);

        public KanjiView()
        {
            this.InitializeComponent();
            ChangeReadMode();
        }

        public void ShowKanji(KanjiDict kanji, Window window)
        {
            ChangeReadMode();

            this.kanji = kanji;            
            SetKanjiElement();
            SetFrequency();
            SetOldLPT();
            SetStrokeCount();
            SetRadicalList();
            SetEngMean();
            SetRadicalName();
            SetKunyomi();
            SetOnyomi();
            SetNanori();
            SetVariants();
            SetVietnamese();
            SetPinyin();
            SetKorea();
            SetImage();
            ShowPopup(window);
        }

        public void Hide()
        {
            popup.IsOpen = false;
        }

        private void SetKanjiElement()
        {
            kanjiElement.Text = kanji.KanjiElement;
        }

        private void SetFrequency()
        {
            if (kanji.FrequencyRank != null)
                frequencyRank.Text = kanji.FrequencyRank.ToUpper();
            else
                frequencyRank.Text = "UNRANKED";
            frequencyRankBorder.Background = PriorityToColor.Convert(frequencyRank.Text);
        }

        private void SetOldLPT()
        {
            if (kanji.OldJlpt != null)
            {
                oldJlpt.Text = kanji.OldJlpt;
                jlptStack.Visibility = Visibility.Visible;
            }
            else
                jlptStack.Visibility = Visibility.Collapsed;
        }

        private void SetStrokeCount()
        {
            strokCount.Text = kanji.StrokeCount.ToString();
        }

        private void SetRadicalList()
        {
            radicalList.Text = kanji.RadicalList;
        }

        private void SetEngMean()
        {
            if (kanji.EngMean != null)
            {
                englishMean.Text = kanji.EngMean;
                engMeanStack.Visibility = Visibility.Visible;
            }
            else
                engMeanStack.Visibility = Visibility.Collapsed;
        }

        private void SetRadicalName()
        {
            if (kanji.RadicalName != null)
            {
                radicalName.Text = kanji.RadicalName;
                radicalNameStack.Visibility = Visibility.Visible;
            }
            else
                radicalNameStack.Visibility = Visibility.Collapsed;
        }

        private void SetKunyomi()
        {
            if (kanji.Kunyomi != null)
            {
                kunyomi.Text = kanji.Kunyomi;
                kunStack.Visibility = Visibility.Visible;
            }
            else
                kunStack.Visibility = Visibility.Collapsed;
        }

        private void SetOnyomi()
        {
            if (kanji.Onyomi != null)
            {
                onyomi.Text = kanji.Onyomi;
                onStack.Visibility = Visibility.Visible;
            }
            else
                onStack.Visibility = Visibility.Collapsed;
        }

        private void SetVietnamese()
        {
            if (kanji.Vietnam != null)
            {
                vietnam.Text = kanji.Vietnam;
                vietnamStack.Visibility = Visibility.Visible;
            }
            else
                vietnamStack.Visibility = Visibility.Collapsed;
        }

        private void SetPinyin()
        {
            if (kanji.Pinyin != null)
            {
                pinyin.Text = kanji.Pinyin;
                pinyinStack.Visibility = Visibility.Visible;
            }
            else
                pinyinStack.Visibility = Visibility.Collapsed;
        }

        private void SetKorea()
        {
            if (kanji.KoreanH != null)
            {
                korea.Text = kanji.KoreanH;
                koreaStack.Visibility = Visibility.Visible;
            }
            else
                koreaStack.Visibility = Visibility.Collapsed;
        }

        private void SetNanori()
        {
            if (kanji.Nanori != null)
            {
                nanori.Text = kanji.Nanori;
                nanoriStack.Visibility = Visibility.Visible;
            }
            else
                nanoriStack.Visibility = Visibility.Collapsed;
        }

        private void SetVariants()
        {
            if (kanji.Variants != null)
            {
                variants.Text = kanji.Variants;
                variantStack.Visibility = Visibility.Visible;
            }
            else
                variantStack.Visibility = Visibility.Collapsed;
        }

        private void SetImage()
        {
            try
            {
                if (kanji.SVGData == null)
                    return;

                kanjiImageView.Children.Clear();
                var paths = GetSvgPaths();
                for (int i = 1; i <= paths.Count; i++)
                    CreateGridPathXaml(paths, i);
            }
            catch
            {

            }
        }

        private List<Geometry> GetSvgPaths()
        {
            var matches = pathRegex.Matches(kanji.SVGData);
            List<Geometry> paths = new List<Geometry>(matches.Count);
            foreach (Match match in matches)
            {
                foreach (Match matchData in dataRegex.Matches(match.Value))
                {
                    string data = matchData.Value.Trim();
                    data = data.Substring(3, matchData.Value.Length - 5);
                    paths.Add(Geometry.Parse(data));
                }
            }
            return paths;
        }

        private void CreateGridPathXaml(List<Geometry> dataPaths, int length)
        {
            var grid = new Grid();
            grid.Width = GRID_MAXWIDTH;
            grid.Height = GRID_MAXHEIGHT;
            grid.Margin = gridMargin;

            var border = GetDefaultBorder();
            grid.Children.Add(border);
            Path path;
            for (int i = 0; i < length - 1; i++)
            {
                path = GetDefaultPath();                
                path.Data = dataPaths[i];                
                grid.Children.Add(path);
            }
            path = GetDefaultPath();
            path.Stroke = lastStrokeBrush;
            path.Data = dataPaths[length - 1];
            grid.Children.Add(path);

            kanjiImageView.Children.Add(grid);
        }

        private static Path GetDefaultPath()
        {
            Path path = new Path();
            path.VerticalAlignment = VerticalAlignment.Stretch;
            path.HorizontalAlignment = HorizontalAlignment.Stretch;
            path.Stroke = strokeBrush;

            path.Stretch = Stretch.None;
            path.StrokeThickness = STROKE_THICKNESS;
            return path;
        }

        private static Border GetDefaultBorder()
        {
            Border border = new Border();
            border.BorderBrush = borderBrush;
            border.BorderThickness = borderThickNess;
            return border;
        }

        private void ShowPopup(Window window)
        {
            ChangeReadMode();
            popup.MaxWidth = window.ActualWidth;
            popup.MaxHeight = window.ActualHeight;
            scrollViewer.ScrollToTop();
            popup.IsOpen = true;
        }

        private void ChangeReadMode()
        {
            if (isNightMode != MainWindow.UserPrefs.IsReadNightMode)
            {
                isNightMode = MainWindow.UserPrefs.IsReadNightMode;
                UIUtilities.ChangeReadMode(this, isNightMode);
            }
        }
    }
}
