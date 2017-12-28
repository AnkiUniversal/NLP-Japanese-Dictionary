using NLPJapaneseDictionary;
using NLPJDict.HelperClasses;
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
    public sealed partial class WordInformationGridView : UserControl
    {
        public delegate void WordClickHandler(WordInformationModel word);
        public event WordClickHandler WordClicked;
        public event RoutedEventHandler ReTokenizeClicked;
        public event RoutedEventHandler UndoReTokenizeClicked;

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
                wordsView.DataContext = viewModel.Words;
            }
        }

        public WordInformationGridView()
        {
            this.InitializeComponent();            
            ChangeReadingVisibility(MainWindow.UserPrefs.IsShowReading);
            ChangePronunciationVisibility(MainWindow.UserPrefs.IsShowPronun);
        }

        public void ScrollToFirst()
        {            
            scrollViewer.ScrollToTop();            
        }

        public void ChangeReadingVisibility(bool visibility)
        {
            readingBindingElement.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ChangePronunciationVisibility(bool visibility)
        {
            pronunBindingElement.Visibility = visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            var word = radioButton.DataContext as WordInformationModel;
            if (word == null)
                return;

            WordClicked?.Invoke(word);
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            UndoReTokenizeClicked?.Invoke(sender, e);
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            ReTokenizeClicked?.Invoke(sender, e);
        }

        private void OnWordsViewItemClick(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;

            var word = element.DataContext as WordInformationModel;
            if (word == null)
                return;

            //If word is selectable then it's handle by a different event
            if (word.IsSelectable)
                return;

            WordClicked?.Invoke(word);
        }

        private void OnWordsViewPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                scrollViewer.RaiseEvent(eventArg);
            }
        }

    }
}
