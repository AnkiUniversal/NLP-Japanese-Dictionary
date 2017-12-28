using Jocr;
using NLPJapaneseDictionary.Helpers;
using NLPJapaneseDictionary.OCR;
using NLPJapaneseDictionary.Views;
using NLPJDict.ConvertClasses;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.KuromojiIpadic.Ipadic;
using NLPJDict.NLPJDictCore;
using NLPJDict.NLPJDictCore.DatabaseTable;
using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static NLPJDict.NLPJDictCore.DatabaseTable.GeneralPreference;

namespace NLPJapaneseDictionary.Pages
{
    public partial class SearchPage : Page, IDisposable
    {
        private const int MAX_NO_EXAMPLES = 20;
        private const int MAX_RECENTS = 30;
        private const int MAX_RESULTS_PER_PAGE = 20;
        private string webSearchUri = @"https://dictionary.goo.ne.jp/srch/all/????/m0u/";

        private readonly Database japEngDictionary;
        private readonly Database kanjiDictionary;
        private readonly Database exampleDictionary;
        private readonly MainWindow mainPage;
        private readonly Dispatcher currentDispatcher;

        private WordInformationViewModel wordGridViewModel;
        private NLPTokenizer<Token> tokenizer;
        private DictionaryWordViewModel dictionaryWordViewModel = new DictionaryWordViewModel();
        private List<JmdictEntity> searchResults;

        private SearchTextViewModel searchTextViewModel;

        private delegate bool SearchInputHandler(string input);
        private SearchInputHandler searchMethod;

        private ExampleSentenceViewModel exampleSentenceViewModel;
        private ExampleSentenceView exampleView;
        private ExampleSentenceView ExampleView
        {
            get
            {
                if (exampleView == null)
                {
                    exampleView = new ExampleSentenceView();
                    exampleSentenceViewModel = new ExampleSentenceViewModel();
                    exampleView.ViewModel = exampleSentenceViewModel;
                }

                return exampleView;
            }
        }

        private bool isNightMode = false;

        public RoutedEventHandler OcrFinishedEvent;

        public SearchPage(MainWindow mainWindow)
        {
            this.InitializeComponent();
            InitViews();

            this.mainPage = mainWindow;
            currentDispatcher = Dispatcher.CurrentDispatcher;

            japEngDictionary = MainWindow.JapEngDictionary;
            tokenizer = MainWindow.NLPTokenizer;
            kanjiDictionary = MainWindow.KanjiDictionary;
            exampleDictionary = MainWindow.ExampleDictionary;

            ChangeReadMode();
            HookEvents();
        }

        /// <summary>
        /// Tokenize and search a word/sentence/paragraph. All new line chars are removed before searching
        /// </summary>
        /// <param name="text"></param>
        public void SearchText(string text)
        {
            text = StringHelper.NewLineRegex.Replace(text, " ");
            if (!String.IsNullOrWhiteSpace(text))                            
                UpdateTextboxThenSearch(text);            
        }

        /// <summary>
        /// Extract japanese sentences from a gray image.
        /// </summary>
        /// <param name="grayImage"></param>
        public void SearchOCRSentences(GrayImage grayImage)
        {
            if (MainWindow.UserPrefs.OcrEngine == OcrEngineType.NlpJdict)
                SearchImage(grayImage);
        }

        /// <summary>
        /// Extract one kanji letter from a gray image.
        /// </summary>
        /// <param name="grayImage"></param>
        public async void SearchOCROneLetter(GrayImage grayImage)
        {
            var oldText = searchTextBox.Text;            
            var caretIndex = searchTextBox.CaretIndex;
            if (MainWindow.UserPrefs.OcrEngine == OcrEngineType.NlpJdict)
                SearchImage(grayImage, Jocr.Ocr.RecognizeOneWord);
            await Task.Delay(10); //Wait for search textbox to be updated if needed
            searchTextBox.Text = oldText.Insert(caretIndex, searchTextBox.Text);            
        }

        public void HideJocrResults()
        {
            searchTextBox.Visibility = Visibility.Visible;
            ocrOneWordView.Visibility = Visibility.Collapsed;
        }

        private void InitViews()
        {
            wordGridViewModel = new WordInformationViewModel();
            
            wordsGridView.ViewModel = wordGridViewModel;
            wordsGridView.WordClicked += OnWordsGridViewWordClicked;
            wordsGridView.ReTokenizeClicked += OnReTokenizeClicked;
            wordsGridView.UndoReTokenizeClicked += OnUndoReTokenizeClicked;

            dictionaryWordView.ViewModel = dictionaryWordViewModel;
            dictionaryWordView.OnExampleCliked += OnExampleCliked;
            dictionaryWordView.KanjiClickEvent += OnDictionaryWordViewKanjiClickEvent;
            dictionaryWordView.OnWebSearchClicked += OnWebSearchClicked;

            pageControl.ItemsPerPage = MAX_RESULTS_PER_PAGE;
            pageControl.PageChanged += OnPageChanged;

            SetupSearchOptions();
            searchMethod = SearchAutoDectectInput;
            
            ocrOneWordView.ViewModel = new OcrOneWordViewModel();
            ocrOneWordView.WordClicked += OnOcrOneWordViewWordClicked;

            DataObject.AddPastingHandler(searchTextBox, OnSearchTextBoxPaste);
        }

        private void UpdateTextboxThenSearch(string text)
        {
            searchTextBox.Text = text;
            Search(text);
        }

        private void Search()
        {
            Search(searchTextBox.Text);
        }

        private void Search(string text)
        {
            try
            {
                string inputWord = text.Trim();

                noResultsMessage.Visibility = Visibility.Collapsed;
                searchTextViewModel.AddFirstNonDuplicate(new NLPJDict.Models.SearchTextModel(inputWord));

                wordsGridViewRoot.Visibility = Visibility.Collapsed;
                pageControl.Hide();

                if (!searchMethod(inputWord))
                {
                    dictionaryWordViewModel.DictionaryWords.Clear();
                    noResultsMessage.Text = "SORRY NO RESULTS FOUND.";
                    noResultsMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                UIUtilities.ShowErrorDialog("Search error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void SearchImage(GrayImage grayImage, Jocr.Ocr.RunOcrHandler runOcrHandler = null)
        {
            if (!JocrWrapper.IsOcrParametersInit)
                progressRing.StartAnimation();

            Task.Run(() =>
            {
                if (!JocrWrapper.IsOcrParametersInit)
                    JocrWrapper.InitOcrParameters();

                currentDispatcher.Invoke(() =>
                {
                    var textBlocks = JocrWrapper.RunOcr(grayImage, runOcrHandler);
                    SearchJOcrResults(textBlocks);
                    progressRing.StopAnimation();
                    OcrFinishedEvent?.Invoke(null, null);
                });
            });
        }

        private bool SearchAutoDectectInput(string inputWord)
        {
            if (StringHelper.IBasicLatinOnly(inputWord))
            {
                var hira = RomaConvert.ConvertRomaToHiraFullLoop(inputWord);
                if (!SearchRawJapInput(hira))
                {
                    if (!SearchEnglish(inputWord))
                        return SearchJapanese(hira);
                    else
                        return true;
                }
                else
                    return true;
            }
            else
            {
                return SearchJapanese(inputWord);
            }
        }

        private bool SearchEnglish(string inputWord)
        {
            List<JmdictEntity> rawSearchResults = TokensDictSearcher.FindByGloss(inputWord, japEngDictionary);
            if (rawSearchResults != null && rawSearchResults.Count > 0)
            {
                ShowRawResults(rawSearchResults);
                return true;
            }
            return false;
        }

        private bool SearchRomaji(string inputWord)
        {
            var hira = RomaConvert.ConvertRomaToHiraFullLoop(inputWord);
            return SearchJapanese(hira);
        }

        private bool SearchJapanese(string inputWord)
        {
            if (SearchRawJapInput(inputWord))
                return true;
            else
            {
                if (inputWord.Length == 1)
                {
                    return SearchKanji(inputWord);
                }
                else
                {
                    return Tokenize(inputWord);
                }
            }
        }

        private bool SearchRawJapInput(string inputWord)
        {
            List<JmdictEntity> rawSearchResults = TokensDictSearcher.FindJapNonToken(inputWord, japEngDictionary);
            if (rawSearchResults != null && rawSearchResults.Count > 0)
            {
                ShowRawResults(rawSearchResults);
                return true;
            }
            return false;
        }

        private bool SearchKanji(string inputWord)
        {
            var kanjiSearch = KanjiDict.GetKanji(inputWord, kanjiDictionary);
            if (kanjiSearch != null)
            {
                KanjiView kanjiView = new KanjiView();
                kanjiView.ShowKanji(kanjiSearch, mainPage);
                return true;
            }
            return false;
        }

        private void ShowRawResults(List<JmdictEntity> rawSearchResults)
        {
            wordsGridViewRoot.Visibility = Visibility.Collapsed;
            searchResults = rawSearchResults;
            pageControl.ChangeNumberOfItem(searchResults.Count);
        }

        private bool Tokenize(string inputWord)
        {
            wordsGridViewRoot.Visibility = Visibility.Collapsed;
            dictionaryWordView.ViewModel.DictionaryWords.Clear();

            tokenizer.Tokenize(inputWord);

            if (tokenizer.Words.Count > 0)
            {
                wordsGridViewRoot.Visibility = Visibility.Visible;
                AddNewTokenizedWordsToViewModel();

                if (tokenizer.Words.Count == 1)
                {
                    wordGridViewModel.CurrentSelectedIndex = 0;
                    ShowSearchResults(tokenizer.Words[0]);
                }
                return true;
            }
            else
                return false;
        }
        
        private void OnDictionaryWordViewKanjiClickEvent(NLPJDict.Models.OneWordModel word)
        {
            var kanji = KanjiDict.GetKanji(word.Word, MainWindow.KanjiDictionary);
            var kanjiView = new KanjiView();
            kanjiView.ShowKanji(kanji, mainPage);
        }

        private void OnWordsGridViewWordClicked(NLPJDict.Models.WordInformationModel word)
        {
            try
            {
                wordGridViewModel.CurrentSelectedIndex = word.Index;
                ShowSearchResults(tokenizer.Words[word.Index]);
                ReflectToOcrWordViewIfNeeded(word);
            }
            catch (Exception ex)
            {
                UIUtilities.ShowErrorDialog("Search token word error: " + ex.Message + "\n" + ex.StackTrace);                
            }
        }

        private void ReflectToOcrWordViewIfNeeded(NLPJDict.Models.WordInformationModel word)
        {            
            if (ocrOneWordView != null && ocrOneWordView.Visibility == Visibility.Visible)
            {
                if (ocrOneWordView.ViewModel.Sentence.Length == searchTextBox.Text.Length)
                {
                    int letterIndex = 0;
                    for (int i = 0; i < word.Index; i++)
                        letterIndex += wordGridViewModel.Words[i].Surface.Length;

                    ocrOneWordView.MarkWordIndex(letterIndex);
                }
            }
        }

        private void ShowSearchResults(WordInformation currentSelectedWord)
        {
            searchResults = TokensDictSearcher.SearchTokenWord(currentSelectedWord, wordGridViewModel.CurrentSelectedIndex,
                                                            tokenizer.Words, japEngDictionary);
            pageControl.ChangeNumberOfItem(searchResults.Count);
        }

        private void OnPageChanged(int newStartIndex, int length)
        {
            dictionaryWordViewModel.AddNewWordList(searchResults.GetRange(newStartIndex, length));
            dictionaryWordView.ScrollToFirstItem();
        }

        private void OnSearchOptionsButtonClick(object sender, RoutedEventArgs e)
        {            
            searchOptionsPopup.IsOpen = !searchOptionsPopup.IsOpen;
        }

        private void SetupSearchOptions()
        {
            searchTextViewModel = new SearchTextViewModel(MAX_RECENTS);
            searchedTextView.ViewModel = searchTextViewModel;
        }   

        private void OnReTokenizeClicked(object sender, RoutedEventArgs e)
        {
            dictionaryWordView.ViewModel.DictionaryWords.Clear();
            tokenizer.TokenizeReducedSentence();
            AddNewTokenizedWordsToViewModel();
        }

        private void OnUndoReTokenizeClicked(object sender, RoutedEventArgs e)
        {
            dictionaryWordView.ViewModel.DictionaryWords.Clear();
            tokenizer.UndoTokenizeReducedSentence();
            AddNewTokenizedWordsToViewModel();
        }

        private void OnReadModeChanged(object sender, RoutedEventArgs e)
        {
            ChangeReadMode();
        }

        private void ChangeReadMode()
        {
            if (isNightMode == MainWindow.UserPrefs.IsReadNightMode)
                return;

            isNightMode = MainWindow.UserPrefs.IsReadNightMode;
            
            UIUtilities.ChangeReadMode(userControl, MainWindow.UserPrefs.IsReadNightMode);
            if (isNightMode)
            {
                searchBoxBackground.Background = UIUtilities.DarkerGray;
                wordGridBackground.Background = UIUtilities.MoreDarkerGray;
            }
            else
            {
                searchBoxBackground.Background = UIUtilities.LighterGray;
                wordGridBackground.Background = UIUtilities.MoreLighterGray;
            }

            if (ocrOneWordView != null)
                ocrOneWordView.ChangeReadMode(MainWindow.UserPrefs.IsReadNightMode);
        }

        private void OnExampleCliked(NLPJDict.Models.DictionaryWordModel word)
        {
            ExampleView.ViewModel.GetExamples(word.FirstWord, exampleDictionary, MAX_NO_EXAMPLES);
            ExampleView.ShowExample(mainPage);
        }

        private void OnSearchedTextClicked(NLPJDict.Models.SearchTextModel model)
        {
            searchOptionsPopup.IsOpen = false;            
            searchTextViewModel.Remove(model);
            UpdateTextboxThenSearch(model.Text);
        }

        private void SearchJOcrResults(List<Jocr.TextBlock> textBlocks)
        {
            if (textBlocks == null)
                return;

            noResultsMessage.Visibility = Visibility.Collapsed;
            ocrOneWordView.ViewModel.AddNewList(textBlocks);
            var text = ocrOneWordView.ViewModel.Sentence.ToString();
            if (!String.IsNullOrWhiteSpace(text))
            {
                UpdateTextboxThenSearch(text);
                ShowOcrTexts(textBlocks);
            }
            else
            {
                searchTextBox.Text = "";
                wordsGridViewRoot.Visibility = Visibility.Collapsed;
                dictionaryWordViewModel.DictionaryWords.Clear();
                noResultsMessage.Text = "SORRY NO WORDS FOUND FROM OCR.";
                noResultsMessage.Visibility = Visibility.Visible;
                HideJocrResults();
            }
        }

        private void ShowOcrTexts(List<Jocr.TextBlock> textBlocks)
        {
            ocrOneWordView.RollToStart();
            if (ocrOneWordView.Visibility == Visibility.Collapsed)
            {                
                searchTextBox.Visibility = Visibility.Collapsed;
                ocrOneWordView.Visibility = Visibility.Visible;
            }
        }

        private void OnOcrOneWordViewWordClicked(object sender, RoutedEventArgs e)
        {
            var sentence = ocrOneWordView.ViewModel.Sentence.ToString();
            UpdateTextboxThenSearch(sentence);
        }

        private void OnWebSearchClicked(NLPJDict.Models.DictionaryWordModel model)
        {
            string search = webSearchUri.Replace("????", model.FirstWord);
            System.Diagnostics.Process.Start(search);            
        }

        private void OnSearchTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!String.IsNullOrWhiteSpace(searchTextBox.Text))
                    Search();
            }
        }

        private void OnSearchTextBoxPaste(object sender, DataObjectPastingEventArgs e)
        {
            try
            {                
                var text = UIUtilities.GetTextFromClipboard(e);
                SearchPastedText(text);
            }
            catch (Exception ex)
            {
                UIUtilities.ShowErrorDialog("Search paste error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        public async void SearchPastedText(string text)
        {
            text = StringHelper.NewLineRegex.Replace(text, " ");
            searchTextBox.SelectedText = text;
            await Task.Delay(50); //Wait for textbox content to update
            text = searchTextBox.Text;
            if (!String.IsNullOrWhiteSpace(text))
                Search(text);
        }

        private void AddNewTokenizedWordsToViewModel()
        {
            wordGridViewModel.AddNewWordsList(tokenizer.Words);
            wordsGridView.ScrollToFirst();
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var height = e.NewSize.Height;
            if (height < 500)
                wordsGridViewRoot.MaxHeight = 85;
            else if (height < 600)
                wordsGridViewRoot.MaxHeight = 100;
            else if (height < 700)
                wordsGridViewRoot.MaxHeight = 150;
            else if (height < 800)
                wordsGridViewRoot.MaxHeight = 200;
            else 
                wordsGridViewRoot.MaxHeight = 250;
        }

        private void HookEvents()
        {
            MainWindow.UserPrefs.DatabaseChangedEvent += OnUserPrefsDatabaseChangedEvent;
        }

        private void OnUserPrefsDatabaseChangedEvent(PreferenceChanged preferenceChanged)
        {
            switch(preferenceChanged)
            {
                case PreferenceChanged.ShowReading:
                    OnShowReadingToggle();
                    break;

                case PreferenceChanged.ShowPronunication:
                    OnShowPronunciationToggle();
                    break;

                case PreferenceChanged.ReadMode:
                    ChangeReadMode();
                    break;

                default:
                    break;
            }
        }

        private void OnShowReadingToggle()
        {
            wordsGridView.ChangeReadingVisibility(MainWindow.UserPrefs.IsShowReading);
        }

        private void OnShowPronunciationToggle()
        {
            wordsGridView.ChangePronunciationVisibility(MainWindow.UserPrefs.IsShowPronun);
        }

        public void Dispose()
        {
            UnHookEvents();
        }

        private void UnHookEvents()
        {
            MainWindow.UserPrefs.DatabaseChangedEvent -= OnUserPrefsDatabaseChangedEvent;
        }
    }
}
