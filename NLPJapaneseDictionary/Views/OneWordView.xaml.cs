using NLPJDict.Models;
using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;

namespace NLPJapaneseDictionary.Views
{
    public sealed partial class OneWordView : UserControl
    {
        private OneWordViewModel viewModel;
        public OneWordViewModel ViewModel
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

        public delegate void WordClickHandler(OneWordModel word);
        public event WordClickHandler WordClickEvent;

        public OneWordView()
        {
            this.InitializeComponent();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var word = button.DataContext as OneWordModel;
            if(word != null)
            {
                WordClickEvent?.Invoke(word);
            }
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                if(parent != null)
                    parent.RaiseEvent(eventArg);                
            }
        }
    }
}
