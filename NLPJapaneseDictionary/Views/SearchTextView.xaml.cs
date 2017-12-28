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
    /// <summary>
    /// Interaction logic for SearchTextView.xaml
    /// </summary>
    public partial class SearchTextView : UserControl
    {
        private SearchTextViewModel viewModel;
        public SearchTextViewModel ViewModel
        {
            get { return viewModel; }
            set
            {
                viewModel = value;
                listView.DataContext = viewModel.SearchedTexts;
            }
        }

        public delegate void SearchedTextClickedHandler(SearchTextModel model);
        public event SearchedTextClickedHandler SearchedTextClicked;

        public SearchTextView()
        {
            this.InitializeComponent();
        }

        private void OnSearchTextClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var textSearched = button.DataContext as SearchTextModel;
            if (textSearched == null)
                return;

            SearchedTextClicked?.Invoke(textSearched);
        }
    }
}
