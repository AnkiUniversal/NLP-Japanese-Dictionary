using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;

namespace NLPJapaneseDictionary.Views
{
    public sealed partial class WordInformationView : UserControl
    {
        private WordInformationViewModel viewModel;
        public WordInformationViewModel ViewModel
        {
            get
            {
                return viewModel;
            }
            set
            {
                viewModel = value;
                wordsListView.DataContext = viewModel.Words;
            }
        }


        public WordInformationView()
        {
            this.InitializeComponent();
        }
    }
}
