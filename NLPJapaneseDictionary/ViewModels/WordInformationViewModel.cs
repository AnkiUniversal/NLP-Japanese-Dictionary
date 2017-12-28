using NLPJDict.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NLPJDict.KuromojiIpadic.Ipadic;
using NLPJDict.NLPJDictCore;
using NLPJDict.ConvertClasses;
using System.Diagnostics;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.HelperClasses;
using System.Windows.Media;
using NLPJapaneseDictionary.Helpers;

namespace NLPJDict.ViewModels
{
    public class WordInformationViewModel
    {
        private const int NO_INDEX = -1;
        public int CurrentSelectedIndex { get; set; } = NO_INDEX;

        public ObservableCollection<WordInformationModel> Words { get; set; }

        public WordInformationViewModel()
        {
            Words = new ObservableCollection<WordInformationModel>();
        }

        public void AddNewWordsList(List<WordInformation> words)
        {
            Words.Clear();
            CurrentSelectedIndex = NO_INDEX;
            var romajiWords = ConvertPronunicationToRomaji(words);
            for (int i = 0; i < words.Count; i++)
            {
                WordInformationModel word = GetWordInformationModel(words[i], romajiWords[i], i);
                Words.Add(word);
            }
        }

        private WordInformationModel GetWordInformationModel(WordInformation wordInfor, string romajiWords, int index)
        {
            string baseForm;
            string reading;
            string pronunciation;
            string conjugation;
            bool isChecked = false;
            bool isInDicionary = false;

            if (wordInfor.IsSymbol() || String.IsNullOrWhiteSpace(wordInfor.Surface))
            {
                reading = null;
                pronunciation = null;
                baseForm = null;
                conjugation = null;
                isChecked = false;
                isInDicionary = false;
            }
            else
            {
                reading = wordInfor.Reading;
                pronunciation = romajiWords;
                isInDicionary = wordInfor.IsInDictionary;
                baseForm = wordInfor.BaseForm;

                var wrodConjugation = wordInfor.Conjugation;
                if (WordInformation.IsHave(wrodConjugation))
                    conjugation = wrodConjugation;
                else
                    conjugation = null;

                if (CurrentSelectedIndex == NO_INDEX && isInDicionary)
                {
                    CurrentSelectedIndex = index;
                    isChecked = true;
                }
            }

            SolidColorBrush borderColor = GetBorderColor(wordInfor);
            var word = new WordInformationModel(wordInfor.Surface, conjugation, baseForm, reading, pronunciation, isInDicionary, isChecked, borderColor);
            word.Index = index;
            return word;
        }

        private static string[] ConvertPronunicationToRomaji(List<WordInformation> words)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < words.Count - 1; i++)
            {
                builder.Append(words[i].Pronunciation);
                builder.Append("\nRomaji\n");
            }
            builder.Append(words[words.Count - 1].Pronunciation);

            var results = RomaConvert.ConvertKataToRomaFullLoop(builder)
                          .Split(new string[] { "\nRomaji\n" }, StringSplitOptions.None);
            results = MakeSureCorrectWords(words, results);
            return results;
        }

        private static string[] MakeSureCorrectWords(List<WordInformation> words, string[] results)
        {
            if (results.Length != words.Count)
            {
                Debug.WriteLine("WordInformationViewModel.ConvertPronunicationToRomaji: String contains token word -> use extreme slow method");
                results = new string[words.Count];
                //WARNING: This is very slow
                for (int i = 0; i < words.Count; i++)
                {
                    results[i] = RomaConvert.ConvertKataToRomaFullLoop(words[i].Pronunciation);
                }
            }

            return results;
        }

        private static SolidColorBrush GetBorderColor(WordInformation wordInfor)
        {
            if (wordInfor.LinkWordGroup != 0)
            {
                if (wordInfor.LinkWordGroup % 2 == 0)
                    return UIUtilities.Orange;
                else
                    return UIUtilities.DodgerBlue;
            }

            return UIUtilities.Green;
        }
    }
}
