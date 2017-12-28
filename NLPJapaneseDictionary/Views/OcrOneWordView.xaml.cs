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
using NLPJDict.Models;
using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public sealed partial class OcrOneWordView : UserControl
    {
        private const int ONE_LETTER_WIDTH = 30;

        private OcrWordsViewModel wordsViewModel;
        private OcrOneWordModel selectedWord;

        public event RoutedEventHandler WordClicked;

        private OcrOneWordViewModel viewModel;
        public OcrOneWordViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                wordsView.DataContext = viewModel.Words;
            }
        }

        private bool isNightMode = false;

        public OcrOneWordView()
        {
            this.InitializeComponent();
            wordsViewModel = new OcrWordsViewModel();
            popupWordList.DataContext = wordsViewModel.Words;
        }

        public void ChangeReadMode(bool isNightMode)
        {
            if (this.isNightMode == isNightMode)
                return;

            this.isNightMode = isNightMode;
            if (isNightMode)
                ChangePopupToDark();
            else
                ChangePopupToLight();
        }

        private void ChangePopupToDark()
        {
            popupRoot.Background = new SolidColorBrush(Colors.Black);
        }

        private void ChangePopupToLight()
        {
            popupRoot.Background = new SolidColorBrush(Colors.White);
        }

        private void OnWordClicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            OcrOneWordModel word = button.DataContext as OcrOneWordModel;
            if (word == null)
                return;

            ShowWordsPopup(viewModel.TextBlocks[word.Index], word.Index, button);
        }

        public void ShowWordsPopup(Jocr.TextBlock block, int index, Button button)
        {
            popupScrollViewer.ScrollToTop();
            wordsViewModel.AddNewList(block, index);
            wordPopup.IsOpen = true;
        }

        public void RollToStart()
        {
            if(wordsView.Items.Count > 1)
                wordsView.ScrollIntoView(wordsView.Items[0]);            
        }

        public void MarkWordIndex(int index)
        {
            ScrollToWordIndex(index);
            ChangeOcrDisplayWordColor(index);
        }

        private void ChangeOcrDisplayWordColor(int index)
        {
            if (index < wordsView.Items.Count)
            {
                var item = wordsView.Items[index] as OcrOneWordModel;
                if (item != null)
                {
                    if (selectedWord != null)
                        selectedWord.Brush = UIUtilities.Green;

                    selectedWord = item;
                    selectedWord.Brush = UIUtilities.Orange;
                }
            }
        }

        private void ScrollToWordIndex(int index)
        {  
            if(index < wordsView.Items.Count)
                wordsView.ScrollIntoView(wordsView.Items[index]);
        }

        private void OnWordPopupButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            OcrWordsModel word = button.DataContext as OcrWordsModel;
            if (word == null)
                return;

            wordPopup.IsOpen = false;
            viewModel.ChangeWord(word.Word, wordsViewModel.Index);
            WordClicked?.Invoke(null, null);
        }

        private void OnOrcWordsViewPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                var scrollViewer = UIUtilities.TryScrollWithScrollViewer(wordsView);
                if (scrollViewer == null)
                    return;

                e.Handled = true;
                if (e.Delta < 0)
                    scrollViewer.LineRight();
                else
                    scrollViewer.LineLeft();
            }
        }

        private void OnPopupPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                popupScrollViewer.RaiseEvent(eventArg);
            }
        }
    }
}
