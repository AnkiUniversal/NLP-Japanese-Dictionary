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

using NLPJapaneseDictionary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NLPJapaneseDictionary.KuromojiIpadic.Ipadic;
using NLPJapaneseDictionary.Core;
using NLPJapaneseDictionary.ConvertClasses;
using System.Diagnostics;
using NLPJapaneseDictionary.DatabaseTable.NLPJDictCore;
using NLPJapaneseDictionary.HelperClasses;
using System.Windows.Media;
using NLPJapaneseDictionary.Helpers;

namespace NLPJapaneseDictionary.ViewModels
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
