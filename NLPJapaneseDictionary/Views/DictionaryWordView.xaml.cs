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

using NLPJapaneseDictionary.ConvertClasses;
using NLPJapaneseDictionary.HelperClasses;
using NLPJapaneseDictionary.Models;
using NLPJapaneseDictionary.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NLPJapaneseDictionary.Views
{
    public sealed partial class DictionaryWordView : UserControl
    {
        private const int WIDE_STATE = 450;
        private ListBox currentListView;

        public delegate void DictWordRoutedEventHanlder(DictionaryWordModel model);

        public event OneWordView.WordClickHandler KanjiClickEvent;
        public event DictWordRoutedEventHanlder OnExampleCliked;
        public event DictWordRoutedEventHanlder OnWebSearchClicked;   

        public enum TextLanguage
        {
            English,
            Japanese,
            Unknown
        }
        public TextLanguage SelectedTextLanguage { get; private set; }
        public string SelectedText { get; private set; }

        private DictionaryWordViewModel viewModel;
        public DictionaryWordViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                if(currentListView == null)
                { 
                    var width = this.RenderSize.Width;
                    SetCurrentListView(width);
                }                

                currentListView.DataContext = viewModel.DictionaryWords;                
            }
        }

        public DictionaryWordView()
        {
            this.InitializeComponent();
        }

        public void ScrollToFirstItem()
        {
            if (!TryScrollWithScrollViewer())
            {
                if (currentListView.Items.Count > 1)
                    currentListView.ScrollIntoView(currentListView.Items[0]);
            }
        }

        private bool TryScrollWithScrollViewer()
        {
            var child = (VisualTreeHelper.GetChild(currentListView, 0));
            if (child == null)
                return false;

            var border = child as Border;
            if (border == null || border.Child == null)
                return false;

            var scrollViewer = border.Child as ScrollViewer;
            if (scrollViewer == null)
                return false;

            scrollViewer.ScrollToTop();
            return true;
        }

        private void OnKanjiWordClickEvent(NLPJapaneseDictionary.Models.OneWordModel word)
        {
            KanjiClickEvent?.Invoke(word);
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width > WIDE_STATE && e.NewSize.Width > WIDE_STATE)
                return;
            if (e.PreviousSize.Width < WIDE_STATE && e.NewSize.Width < WIDE_STATE)
                return;

            SetCurrentListView(e.NewSize.Width);
        }

        private void SetCurrentListView(double width)
        {
            if (width > WIDE_STATE)
            {
                if (oneColumnListView != null)
                {
                    oneColumnListView.DataContext = null;
                    oneColumnListView.Visibility = Visibility.Collapsed;                    
                }

                if (twoColumnListView == null)
                    this.FindName("twoColumnListView");

                if (viewModel != null)
                {
                    CreateNewSenseDocumentsObject();
                    twoColumnListView.DataContext = viewModel.DictionaryWords;
                }

                currentListView = twoColumnListView;
                currentListView.Visibility = Visibility.Visible;
            }
            else
            {
                if (twoColumnListView != null)
                {
                    twoColumnListView.DataContext = null;                    
                    twoColumnListView.Visibility = Visibility.Collapsed;                    
                }

                if (oneColumnListView == null)
                    this.FindName("oneColumnListView");

                if (viewModel != null)
                {
                    CreateNewSenseDocumentsObject();
                    oneColumnListView.DataContext = viewModel.DictionaryWords;
                }

                currentListView = oneColumnListView;
                currentListView.Visibility = Visibility.Visible;
            }
        }

        private void CreateNewSenseDocumentsObject()
        {
            foreach (var word in viewModel.DictionaryWords)
                word.Sense.CloneSenseDocument();
        }

        private void OnExampleButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var data = button.DataContext as DictionaryWordModel;
            if (data == null)
                return;

            OnExampleCliked?.Invoke(data);
        }

        private void OnWebSearchButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var data = button.DataContext as DictionaryWordModel;
            if (data == null)
                return;
            OnWebSearchClicked?.Invoke(data);
        }

        /// <summary>
        /// Avoid one FlowDocument to bet set to two different parents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBindableRichTextBoxUnloaded(object sender, RoutedEventArgs e)
        {
            var richTextBox = sender as RichTextBox;
            if (richTextBox != null)
                richTextBox.Document = new FlowDocument();
        }
    }
}
