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
