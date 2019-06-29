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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NLPJapaneseDictionary.Windows
{
    public partial class Settings : Window
    {   

        public Settings(Window mainWindow)
        {
            InitializeComponent();
            MainWindow.MoveSubWindowOnMainWindow(mainWindow, this);
            InitViews();
        }

        public void InitViews()
        {
            katakanaReading.IsChecked = MainWindow.UserPrefs.IsShowReading;
            romajiReading.IsChecked = MainWindow.UserPrefs.IsShowPronun;

            omitCtrl.IsChecked = MainWindow.UserPrefs.IsOmitCtrl;
            ttsSpeed.Value = MainWindow.UserPrefs.TtsSpeed;
            enableOcrDebug.IsChecked = MainWindow.UserPrefs.IsOcrDebugMode;
            vietnameseReading.IsChecked = MainWindow.UserPrefs.IsShowVietnamese;
            saveOcrCheckBox.IsChecked = MainWindow.UserPrefs.IsSaveOcrImage;
            saveOcrFolderTextBox.Text = MainWindow.UserPrefs.SaveOcrImageFolder;
            NumLetterVi.Text = MainWindow.UserPrefs.VietnameseLetterNumber.ToString();
            InitVoiceList();
        }

        private void OnShowKatakanaCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (katakanaReading.IsChecked == null || katakanaReading.IsChecked == false)
                MainWindow.UserPrefs.IsShowReading = false;
            else
                MainWindow.UserPrefs.IsShowReading = true;
        }

        private void OnRomajiReadingClick(object sender, RoutedEventArgs e)
        {
            if (romajiReading.IsChecked == null || romajiReading.IsChecked == false)
                MainWindow.UserPrefs.IsShowPronun = false;
            else
                MainWindow.UserPrefs.IsShowPronun = true;
        }

        private void InitVoiceList()
        {            
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                var installedVoices = synth.GetInstalledVoices();
                int i = 0;
                foreach (InstalledVoice voice in installedVoices)
                {
                    if (voice.VoiceInfo.Culture.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase))
                    {
                        var textBlock = new TextBlock();
                        textBlock.Text = voice.VoiceInfo.Name;
                        voiceList.Items.Add(textBlock);
                        if (textBlock.Text.Equals(MainWindow.UserPrefs.TtsVoice, StringComparison.OrdinalIgnoreCase))
                            voiceList.SelectedIndex = i;

                        i++;
                    }
                }             
            }
            if (voiceList.Items.Count == 0)
                voiceList.IsEnabled = false;
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var speed = (int)Math.Round(ttsSpeed.Value);
            if (speed < -10)
                speed = -10;
            else if (speed > 10)
                speed = 10;

            MainWindow.UserPrefs.TtsSpeed = speed;
        }

        private void OnVoiceListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textBlock = voiceList.SelectedItem as TextBlock;
            if (textBlock == null)
                return;

            MainWindow.UserPrefs.TtsVoice = textBlock.Text;
        }

        private void OnOmitCtrlClick(object sender, RoutedEventArgs e)
        {
            if (omitCtrl.IsChecked == null || omitCtrl.IsChecked == false)
                MainWindow.UserPrefs.IsOmitCtrl = false;
            else
                MainWindow.UserPrefs.IsOmitCtrl = true;
        }

        private void OnOcrDebugCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (enableOcrDebug.IsChecked == null || enableOcrDebug.IsChecked == false)
                MainWindow.UserPrefs.IsOcrDebugMode = false;
            else
                MainWindow.UserPrefs.IsOcrDebugMode = true;
        }

        private void SaveOcrCheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (saveOcrCheckBox.IsChecked == null || saveOcrCheckBox.IsChecked == false)
                MainWindow.UserPrefs.IsSaveOcrImage = false;
            else
                MainWindow.UserPrefs.IsSaveOcrImage = true;
        }

        private void OnSaveOcrFolderButtonClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    saveOcrFolderTextBox.Text = dialog.SelectedPath;            
                }
            }
        }

        private void OnSaveOcrFolderTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.UserPrefs.SaveOcrImageFolder = saveOcrFolderTextBox.Text.Trim(); 
        }

        private void OnVietnameseReadingClick(object sender, RoutedEventArgs e)
        {
            if (vietnameseReading.IsChecked == null || vietnameseReading.IsChecked == false)
                MainWindow.UserPrefs.IsShowVietnamese = false;
            else
                MainWindow.UserPrefs.IsShowVietnamese = true;
        }

        private void OnNumberLetterKeydownEvent(object sender, KeyEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(NumLetterVi.Text))
                MainWindow.UserPrefs.VietnameseLetterNumber =int.Parse(NumLetterVi.Text);
        }
    }
}
