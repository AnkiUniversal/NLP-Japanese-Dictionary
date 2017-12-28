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

        public Settings()
        {
            InitializeComponent();
            MainWindow.SetupWindowSizeAndPosition(this);
            InitViews();
        }

        public void InitViews()
        {
            katakanaReading.IsChecked = MainWindow.UserPrefs.IsShowReading;
            romajiReading.IsChecked = MainWindow.UserPrefs.IsShowPronun;
            omitCtrl.IsChecked = MainWindow.UserPrefs.IsOmitCtrl;
            ttsSpeed.Value = MainWindow.UserPrefs.TtsSpeed;            
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
    }
}
