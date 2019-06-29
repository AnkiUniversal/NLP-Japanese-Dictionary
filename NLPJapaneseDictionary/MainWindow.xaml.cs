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

using NLPJapaneseDictionary.Core.Hooks;
using NLPJapaneseDictionary.Helpers;
using NLPJapaneseDictionary.OCR;
using NLPJapaneseDictionary.Pages;
using NLPJapaneseDictionary.Windows;
using NLPJapaneseDictionary.DatabaseTable.NLPJDictCore;
using NLPJapaneseDictionary.HelperClasses;
using NLPJapaneseDictionary.KuromojiIpadic.Ipadic;
using NLPJapaneseDictionary.Core;
using NLPJapaneseDictionary.Core.DatabaseTable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NLPJapaneseDictionary
{

    public partial class MainWindow : Window, IDisposable
    {
        private const string OCR_IMAGE_SAVE_ENTRY_LIST = "EntryList.txt";
        private const string OCR_ENTRY_SEPERATOR = "|*|";

        private bool isSplitPlaneOpen = false;
        private Storyboard openMenu;
        private Storyboard closeMenu;
        private Storyboard noticeMe;

        private Dispatcher currentDispatcher;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenu notifyIconContextMenu;

        public static GeneralPreference UserPrefs { get; private set; }

        public static Tokenizer KuromojiTokenizer { get; private set; }
        public static NLPTokenizer<Token> NLPTokenizer { get; private set; }

        public static Database JapEngDictionary { get; private set; }

        private static Database kanjiDictionary;
        public static Database KanjiDictionary
        {
            get
            {
                if(kanjiDictionary == null)
                    kanjiDictionary = new Database(Locations.ABS_DICT_CONVERT_PATH + "KanjiDict.db");
                return kanjiDictionary;
            }
        }

        private static Database exampleDictionary;
        public static Database ExampleDictionary
        {
            get
            {
                if (exampleDictionary == null)
                   exampleDictionary = new Database(Locations.ABS_DICT_CONVERT_PATH + "Example.db");
          
                return exampleDictionary;
            }
        }

        private SearchPage searchPage;
        private SpeechSynthesizer synth;

        private bool isHotKeysDisable;
        private enum KeyboardCommand
        {
            Search,
            TTS,
            OCRSentences,
            OCROneLetter
        }
        private KeyboardCommand currentCommand;

        public MainWindow()
        {
            InitializeComponent();
            RestoreUserPrefs();
            SetupWindowSizeAndPosition(this);
            SetupDictionaryDatabases();
            SetupNavigation();
            SetupNotifyIconIcon();
        }

        public static void SetupWindowSizeAndPosition(Window window)
        {
            SizeToScreen();
            MoveIntoView();
            LoadWindowSize(window);
        }

        public static void MoveSubWindowOnMainWindow(Window mainWindow, Window subWindow)
        {
            subWindow.Top = mainWindow.Top;
            subWindow.Left = mainWindow.Left;
            subWindow.Width = mainWindow.Width;            
            subWindow.Height = mainWindow.Height;
        }

        public static void MoveSubWindowVerticalCenter(Window mainWindow, Window subWindow)
        {
            subWindow.Top = mainWindow.Top + mainWindow.Height / 2 - subWindow.Height / 2;
            subWindow.Width = mainWindow.Width - 10;
            subWindow.Left = mainWindow.Left + 5;
        }

        private void RestoreUserPrefs()
        {
            UserPrefs = GeneralPreference.RetrieveUserPreference();
        }

        private void SetupNavigation()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            openMenu = (Storyboard)TryFindResource("MenuOpen");
            closeMenu = (Storyboard)TryFindResource("MenuClose");
            noticeMe = (Storyboard)TryFindResource("NoticeMe");

            searchPage = new SearchPage(this);
            searchPage.OcrFinishedEvent += OnSearchPageOcrFinishedEvent;
            contentFrame.Navigate(searchPage);

            ChangeReadMode();
            
            App.KeyboadHook = KeyboardHook.GetInstance();
            App.KeyboadHook.KeyPressed += KeyPressed;
            SnippingTool.AreaSelected += SnippingToolOnAreaSelected;

            App.NlpJdictService = NetTcp.NetTcpHelper.CreateNetNamedPipeServer(SearchTextFromAnotherProcess);
        }

        private void SetupNotifyIconIcon()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon(@"./AppIcon.ico");
            notifyIcon.MouseDoubleClick += OnNotifyIconDoubleClick;

            notifyIconContextMenu = new System.Windows.Forms.ContextMenu();
            notifyIconContextMenu.MenuItems.Add("E&xit");
            notifyIconContextMenu.MenuItems[0].Click += OnExitNotifyIconClick;
            notifyIcon.ContextMenu = notifyIconContextMenu;
        }

        private bool KeyPressed(Key e)
        {            
            if (e == Key.Escape)
            {
                if (SnippingTool.IsCapturing)
                    SnippingTool.CancelSnipping();
            }
            else if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                || (Keyboard.Modifiers == ModifierKeys.Shift && UserPrefs.IsOmitCtrl))
            {
                return HandleCtrlShiftPress(e);
            }
            else if (UserPrefs.IsOmitCtrl || Keyboard.Modifiers == ModifierKeys.Control)
            {
                return HandleCtrlPress(e);
            }
            return false;
        }

        private bool HandleCtrlPress(Key e)
        {
            if (isHotKeysDisable)
                return false;

            switch (e)
            {
                case Key.Space:
                    currentCommand = KeyboardCommand.Search;
                    App.ClipBoardHook.CopyFromActiveProgram();
                    return true;

                case Key.R:
                    currentCommand = KeyboardCommand.OCRSentences;
                    SnippingTool.Snip();
                    return true;

                case Key.D1:
                    currentCommand = KeyboardCommand.OCROneLetter;
                    SnippingTool.Snip();
                    return true;

                case Key.T:
                    currentCommand = KeyboardCommand.TTS;
                    App.ClipBoardHook.CopyFromActiveProgram();
                    return true;

                default:
                    return false;
            }
        }

        private bool HandleCtrlShiftPress(Key e)
        {
            switch (e)
            {
                case Key.E:
                    isHotKeysDisable = false;
                    return true;

                case Key.D:
                    isHotKeysDisable = true;
                    return true;

                default:
                    break;
            }
            return false;
        }

        private void SnippingToolOnAreaSelected(object sender, EventArgs e)
        {
            try
            {
                ActivateWindow();
                var grayImg = JorcImageConvert.BitmapToGrayImageJocr(SnippingTool.Bitmap);
                if (searchPage == null)
                    return;                

                switch (currentCommand)
                {                    
                    case KeyboardCommand.OCROneLetter:
                        searchPage.SearchOCROneLetter(grayImg);
                        break;

                    default:
                        searchPage.SearchOCRSentences(grayImg);
                        break;
                }
            }
            catch (Exception ex)
            {
                UIUtilities.ShowErrorDialog("Search using OCR Snipping error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnExitNotifyIconClick(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            this.Close();
        }

        private void OnNotifyIconDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon.Visible = true;

                if (!UserPrefs.IsShownFirtNoitfy)
                {
                    notifyIcon.BalloonTipTitle = "The app is minimized.";
                    notifyIcon.BalloonTipText = "You can still use the app through shortcuts.";
                    notifyIcon.ShowBalloonTip(1000);
                    UserPrefs.IsShownFirtNoitfy = true;
                }
            }
            else if (this.WindowState == WindowState.Normal)
            {
                notifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }

            base.OnStateChanged(e);
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            App.ClipBoardHook = new ClipBoardHook();
            App.ClipBoardHook.ClipboardCopyFinished += OnClipboardCopyFinished;
        }

        private void OnClipboardCopyFinished(object sender, RoutedEventArgs e)
        {
            try
            {
                var text = sender as string;
                if (String.IsNullOrWhiteSpace(text))
                {
                    UIUtilities.ShowMessageDialog("Please select some text first.", "No Text");
                    return;
                }

                switch (currentCommand)
                {
                    case KeyboardCommand.Search:
                        SearchTextFromClipboard(text);
                        break;

                    case KeyboardCommand.TTS:
                        StartTTS(text);
                        break;

                }
            }
            catch(Exception ex)
            {
                UIUtilities.ShowErrorDialog("OnClipboardCopyFinished: " + ex.Message + "\n" + ex.Message);
            }
        }

        private void SearchTextFromClipboard(string text)
        {
            ActivateWindow();
            if (searchPage != null)
                searchPage.SearchText(text);
        }

        private void SearchTextFromAnotherProcess(string text)
        {
            ForceWindowToFront();
            if (searchPage != null)
                searchPage.SearchText(text);
        }

        private void StartTTS(string Text)
        {
            try
            {
                if (synth == null)
                {
                    synth = new SpeechSynthesizer();
                    if (UserPrefs.TtsVoice != null)                    
                        synth.SelectVoice(UserPrefs.TtsVoice);                                            
                    else
                    {
                        bool isFoundVoices = TryToFindJapaneseVoice();
                        if (!isFoundVoices)
                        {
                            UIUtilities.ShowMessageDialog("Please install a Japanese voice in the \"Region & language\" setting of your Windows.",
                                                        "No Japanese Voice");
                            return;
                        }
                    }
                }
                synth.Rate = UserPrefs.TtsSpeed;
                synth.SpeakAsync(Text);
            }
            catch(Exception ex)
            {
                UIUtilities.ShowErrorDialog("StartTTS: " + ex.Message + "\n" + ex.Message);
            }
        }

        private bool TryToFindJapaneseVoice()
        {
            bool isFoundVoices = false;           
            var installedVoices = synth.GetInstalledVoices();
            foreach (InstalledVoice voice in installedVoices)
            {
                if (voice.VoiceInfo.Culture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase))
                {
                    synth.SelectVoice(voice.VoiceInfo.Name);
                    synth.Rate = UserPrefs.TtsSpeed;
                    isFoundVoices = true;
                    break;
                }
            }

            return isFoundVoices;
        }

        public void ActivateWindow()
        {
            if(this.WindowState == WindowState.Minimized)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }
            this.Activate();
        }

        public void ForceWindowToFront()
        {
            ActivateWindow();
            this.Topmost = true;  
            this.Topmost = false;             
        }

        private void OnSplitPlaneToggleClick(object sender, RoutedEventArgs e)
        {
            isSplitPlaneOpen = !isSplitPlaneOpen;
            if (isSplitPlaneOpen)
            {
                openMenu.Begin();
                outOfSplitPlaneDetect.Visibility = Visibility.Visible;
            }
            else
                closeMenu.Begin();
        }

        private void SetupDictionaryDatabases()
        {
            JapEngDictionary = new Database(Locations.ABS_DICT_CONVERT_PATH + "JapEngDict.db");

            KuromojiTokenizer = MakeTokenizer();
            NLPTokenizer = new NLPTokenizer<Token>(KuromojiTokenizer, JapEngDictionary);            
        }

        private Tokenizer MakeTokenizer()
        {
            using (var file = File.OpenRead(Locations.ABS_DICT_COMPILED_PATH + System.IO.Path.DirectorySeparatorChar + "userDict.txt"))
            {
                var builder = new Tokenizer.Builder(Locations.ABS_DICT_COMPILED_PATH);
                builder.LoadUserDictionary(file);
                builder.IsSplitOnNakaguro = true;
                return new Tokenizer(builder);
            }
        }

        private void OnOutOfSplitPlaneDetectClick(object sender, RoutedEventArgs e)
        {
            closeMenu.Begin();
            isSplitPlaneOpen = false;
            outOfSplitPlaneDetect.Visibility = Visibility.Collapsed;
        }

        private void OnReadModeClick(object sender, RoutedEventArgs e)
        {
            UserPrefs.IsReadNightMode = !UserPrefs.IsReadNightMode;
            ChangeReadMode();
        }

        private void ChangeReadMode()
        {            
            ChangeMainWindowReadMode();            
            ChangeResourceDefinition();
        }

        private void ChangeMainWindowReadMode()
        {
            UIUtilities.ChangeReadMode(hotkeyPopup, UserPrefs.IsReadNightMode);
            if (UserPrefs.IsReadNightMode)
            {
                readModeButtonSymbol.Style = Application.Current.Resources["SunPathIcon"] as Style;                
            }
            else
            {
                readModeButtonSymbol.Style = Application.Current.Resources["MoonPathIcon"] as Style;
            }
        }

        private void ChangeResourceDefinition()
        {
            if(UserPrefs.IsReadNightMode)
                Application.Current.Resources["TrackBackground"] = Application.Current.Resources["TrackBackgroundDark"] as SolidColorBrush;
            else
                Application.Current.Resources["TrackBackground"] = Application.Current.Resources["TrackBackgroundLight"] as SolidColorBrush;
        }

        private void OnOCRButtonClick(object sender, RoutedEventArgs e)
        {
            SnippingTool.Snip();
        }

        private void OnSearchPageOcrFinishedEvent(object sender, RoutedEventArgs e)
        {
            if (showTextBoxButton.Visibility == Visibility.Collapsed)
            {
                showTextBoxButton.Visibility = Visibility.Visible;
                noticeMe.Begin();                
            }
            SaveImageIfNeeded(SnippingTool.Bitmap);
        }

        private void SaveImageIfNeeded(System.Drawing.Bitmap bitmap)
        {
            try
            {
                if (MainWindow.UserPrefs.IsSaveOcrImage)
                {
                    var saveFolder = MainWindow.UserPrefs.SaveOcrImageFolder;
                    if (!String.IsNullOrWhiteSpace(saveFolder) && Directory.Exists(saveFolder))
                    {                        
                        StringInputDialog dialog = new StringInputDialog(this, "Entry Name", searchPage.searchTextBox.Text);
                        if (dialog.ShowDialog() == true)
                        {
                            string fileName = DateTime.UtcNow.Ticks.ToString() + ".bmp";
                            char seperator = System.IO.Path.DirectorySeparatorChar;
                            bitmap.Save(saveFolder + seperator + fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                            using (var writer = new StreamWriter(saveFolder + seperator + OCR_IMAGE_SAVE_ENTRY_LIST, true))
                            {
                                writer.WriteLine(OCR_ENTRY_SEPERATOR + fileName + OCR_ENTRY_SEPERATOR + dialog.InputText);
                            }
                        }
                    }
                    else
                        UIUtilities.ShowErrorDialog("Please choose a valid folder path first!");
                }
            }
            catch (Exception e)
            {
                UIUtilities.ShowErrorDialog(e.Message + "\n" + e.StackTrace);
            }
        }

        private void OnShowTextBoxButtonClick(object sender, RoutedEventArgs e)
        {
            showTextBoxButton.Visibility = Visibility.Collapsed;
            searchPage.HideJocrResults();
        }    

        private static void SizeToScreen()
        {
            if (UserPrefs.WindowHeight > SystemParameters.VirtualScreenHeight)            
                UserPrefs.WindowHeight = SystemParameters.VirtualScreenHeight;            

            if (UserPrefs.WindowWidth > SystemParameters.VirtualScreenWidth)            
                UserPrefs.WindowWidth = SystemParameters.VirtualScreenWidth;            
        }

        private static void MoveIntoView()
        {
            var trueScreenHeight = SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop;
            if (UserPrefs.WindowTop + UserPrefs.WindowHeight / 2 > trueScreenHeight)            
                UserPrefs.WindowTop = trueScreenHeight - UserPrefs.WindowHeight;

            var trueScreenWidth = SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft;
            if (UserPrefs.WindowLeft + UserPrefs.WindowWidth / 2 > trueScreenWidth)
                UserPrefs.WindowLeft = trueScreenWidth - UserPrefs.WindowWidth;

            if (UserPrefs.WindowTop < SystemParameters.VirtualScreenTop)
                UserPrefs.WindowTop = SystemParameters.VirtualScreenTop;

            if (UserPrefs.WindowLeft < SystemParameters.VirtualScreenLeft)
                UserPrefs.WindowLeft = SystemParameters.VirtualScreenLeft;
        }

        private static void LoadWindowSize(Window window)
        {
            window.Height = UserPrefs.WindowHeight;
            window.Width = UserPrefs.WindowWidth;
            window.Top = UserPrefs.WindowTop;
            window.Left = UserPrefs.WindowLeft;
            window.WindowState = UserPrefs.WindowState;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool isTrue)
        {
            KuromojiTokenizer.Dispose();
            JapEngDictionary.Dispose();

            if (KanjiDictionary != null)
                KanjiDictionary.Dispose();

            if (ExampleDictionary != null)
                ExampleDictionary.Dispose();

            if (synth != null)
                synth.Dispose();

            if (UserPrefs != null)
                GeneralPreference.Close();
            
            if (isTrue)
            {
                if (notifyIcon != null)
                    notifyIcon.Dispose();
                if (notifyIconContextMenu != null)
                    notifyIconContextMenu.Dispose();
            }
        }     

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowSize();
            UserPrefs.UpdateUserPreference();
        }

        private void SaveWindowSize()
        {
            if (this.WindowState == WindowState.Normal)
            {
                UserPrefs.WindowHeight = this.Height;
                UserPrefs.WindowWidth = this.Width;
                UserPrefs.WindowTop = this.Top;
                UserPrefs.WindowLeft = this.Left;
            }
        }

        private void OnHotkeysButtonClick(object sender, RoutedEventArgs e)
        {
            hotkeyPopup.Show();
        }

        private void OnFAQButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"https://nlpjapanesedictionary.wordpress.com/faqs/");
            }
            catch(Exception ex)
            {
                UIUtilities.ShowErrorDialog("Open FAQ error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnContactButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string Subject = "NLP Japanese Dictionary (" + Assembly.GetExecutingAssembly().GetName().Version + ")";
                string message = "For bugs: Please describe the steps needed to reproduce them.%0A%0D"
                                + "For feature requests: Please explain briefly why you need them.";
                System.Diagnostics.Process.Start($"mailto:ankiuniversal@gmail.com?subject={Subject}&body={message}");
            }
            catch(Exception ex)
            {
                UIUtilities.ShowErrorDialog("Open mail error: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void OnAboutButtonClick(object sender, RoutedEventArgs e)
        {
            About about = new About(this);
            about.Show();
        }

        private void OnSettingButtonClick(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings(this);
            settings.Show();
        }

        private async void OnCheckForUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            if (await IsNewestVersion())
                UIUtilities.ShowMessageDialog("You're using the latest version.");
            else
            {
                AskUserForAppUpdate();
            }
        }

        private static void AskUserForAppUpdate()
        {
            var result = UIUtilities.AskUserPermission("A new update is available. Do you want to visit the download page?");
            if (result == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start("http://nlpjapanesedictionary.wordpress.com");
        }

        public static async Task<bool> IsNewestVersion()
        {
            //All github releases have "v" prefix
            var currentVersion = "v" + About.AppVersion.ToString();

            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("NLP-Japanese-Dictionary"));
            var releases = await client.Repository.Release.GetAll("AnkiUniversal", "NLP-Japanese-Dictionary");
            var latest = releases[0];
            return currentVersion.Equals(latest.TagName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
