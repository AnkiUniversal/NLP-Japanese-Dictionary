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

using NLPJapaneseDictionary.ConvertClasses;
using NLPJapaneseDictionary.DatabaseTable.NLPJDictCore;
using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Core
{
    public class WordListReducer
    {
        private static char[] specialChars = new char[] { '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', '=', '|', '\\' };

        private List<WordInformation> words;
        private Database dictionary;
        private List<WordInformation> results;

        private int startIndex;
        private int endIndex;
        private StringBuilder surface;
        private StringBuilder baseform;
        private StringBuilder pronunciation;
        private StringBuilder reading;

        public bool IsCombineAll { get; set; } = false;
        private int linkWordGroup = 0;        

        public WordInformation ReduceOnce(int startIndex, List<WordInformation> words, Database dictionary, bool isCombineAll = false)
        {
            this.startIndex = startIndex;
            this.words = words;
            this.dictionary = dictionary;
            this.IsCombineAll = isCombineAll;            
            results = new List<WordInformation>();
            surface = new StringBuilder(words[startIndex].Surface);            
            baseform = new StringBuilder(words[startIndex].Surface);
            pronunciation = new StringBuilder(words[startIndex].Pronunciation);
            reading = new StringBuilder(words[startIndex].Reading);            
            AddWord();
            return results[0];
        }

        public List<WordInformation> ReduceAll(List<WordInformation> words, Database dictionary, bool isCombineAll = false)
        {
            this.words = words;
            this.dictionary = dictionary;
            this.IsCombineAll = isCombineAll;
            startIndex = 0;
            results = new List<WordInformation>();
            surface = new StringBuilder();
            baseform = new StringBuilder();
            pronunciation = new StringBuilder();
            reading = new StringBuilder();

            for (; startIndex < words.Count;)
            {
                if (String.IsNullOrWhiteSpace(words[startIndex].Surface))
                {
                    AddCurrentStartIndex(words);
                    continue;
                }

                if (words[startIndex].IsUnknownWord || IsSpecialChar(words[startIndex]))
                {
                    HandleSpecialChars(words);
                    continue;
                }

                ClearPreviousStrings();
                InitNewStartWord(words);

                AddWord();
                startIndex = endIndex + 1;
            }
            return results;
        }

        private void AddCurrentStartIndex(List<WordInformation> words)
        {
            results.Add(words[startIndex]);
            startIndex++;
        }

        private void HandleSpecialChars(List<WordInformation> words)
        {
            var word = new WordInformation(null, null, words[startIndex].Surface, null, words[startIndex].IsUnknownWord);
            word.AddWordPart(words[startIndex].Surface, null, null);            
            results.Add(word);
            startIndex++;
        }

        private void AddWord()
        {
            endIndex = startIndex + 1;
            SearchLongestPartialWordMatch();
            AddLongestFullWordMatch();
        }

        private void SearchLongestPartialWordMatch()
        {            
            for (; endIndex < words.Count; endIndex++)
            {
                if (String.IsNullOrWhiteSpace(words[endIndex].Surface))
                    break;

                if (words[endIndex].IsUnknownWord || IsSpecialChar(words[endIndex]))
                    break;

                string currentBaseform = GetBaseForm(words[endIndex]);                
                bool isIn = CheckInDictionary(words[endIndex].Surface, currentBaseform, 
                                              words[endIndex].Reading, words[endIndex].Pronunciation);
                if (!isIn)
                    break;
            }
            endIndex--;
        }

        private bool CheckInDictionary(string currentSurface, string currentBaseform, string reading, string pronun)
        {
            AppendWordPart(currentSurface, reading, pronun, currentBaseform);
            surface.Append("*");

            string searchWord = String.Concat(surface.ToString(), " OR ", baseform.ToString());
            bool isHave = JmdictEntity.HasJapWord(searchWord, dictionary);            

            if (!isHave)
            {
                surface.Remove(surface.Length - 1, 1);
                RemoveLastWordPath();
                return false;
            }
            else
            {
                surface.Remove(surface.Length - 1, 1);
                return true;
            }
        }

        private void AddLongestFullWordMatch()
        {            
            while (endIndex >= startIndex)
            {
                if (endIndex > startIndex)
                {                                            
                    string wordSurface = surface.ToString();
                    string wordBaseform = baseform.ToString();                                        

                    var matchWords = JmdictEntity.GetJapMatchAll(wordBaseform, dictionary);                    
                    if(matchWords.Count == 0 && !wordSurface.EqualsOrdinalIgnore(wordBaseform))                    
                        matchWords = JmdictEntity.GetJapMatchAll(wordSurface, dictionary);                    

                    if (matchWords.Count > 0)
                    {
                        CreateOrMarkReduceWord(wordSurface, wordBaseform, matchWords);
                        break;
                    }
                    RemoveLastWordPath();
                    endIndex--;
                }
                else
                {
                    results.Add(words[startIndex]);
                    break;
                }
            }
        }

        private void CreateOrMarkReduceWord(string wordSurface, string wordBaseform, List<JmdictEntity> matchWords)
        {
            if (IsCombineToNewWord())
            {
                CreateAndAddNewWord(wordSurface, wordBaseform, matchWords);
                return;
            }

            //If not reduce and is still a valid longest word match then specified linkWordGroup
            //to mark it up 
            if (TokensDictSearcher.IsValidLongestWord(wordSurface, matchWords.Count))
            {
                linkWordGroup++;
                for (int i = startIndex; i <= endIndex; i++)
                    words[i].LinkWordGroup = linkWordGroup;
            }
            results.Add(words[startIndex]);
            endIndex = startIndex;
        }

        private bool IsCombineToNewWord()
        {            
            if (!IsCombineAll)
            {
                if (words[startIndex].IsParticle()
                    || IsValidVerb(words[startIndex])
                    || words[startIndex].IsAdjective()
                    || (words[startIndex].Surface.Equals("な") && words[startIndex].IsAuxiliaryVerb())
                    || words[startIndex].Surface.EqualsOrdinalIgnore("の")
                   )
                {
                    return false;
                }

                for (int i = startIndex + 1; i <= endIndex; i++)
                {
                    if (words[i].IsParticle()
                        || IsValidVerb(words[i])
                        || words[i].IsAdjective()
                        || words[i].LinkWordGroup != words[startIndex].LinkWordGroup)
                        return false;
                }
            }
            return true;
        }

        private void CreateAndAddNewWord(string wordSurface, string wordBaseform, List<JmdictEntity> matchWords)
        {
            WordInformation newWord = new WordInformation(null,
                                                         null,
                                                          wordBaseform,
                                                          null);
            string readingStr = reading.ToString();
            string pronunStr = pronunciation.ToString();
            MakeSureReadingAndPronunIsCorrect(wordSurface, matchWords, ref readingStr, ref pronunStr);
            newWord.AddWordPart(wordSurface, readingStr, pronunStr);
            newWord.IsInDictionary = true;
            newWord.LinkWordGroup = words[startIndex].LinkWordGroup;
            results.Add(newWord);
        }

        private void MakeSureReadingAndPronunIsCorrect(string wordSurface, List<JmdictEntity> matchWords, ref string readingStr, ref string pronunStr)
        {
            if (!IsCombineAll && matchWords.Count == 1 && StringHelper.IsHaveKanji(wordSurface))
            {
                if (matchWords[0].RepresentWord.ContainsExtend(wordSurface, StringComparison.OrdinalIgnoreCase)
                  || matchWords[0].KanjiElement.ContainsExtend(wordSurface, StringComparison.OrdinalIgnoreCase))
                {
                    var readingElements = JmdictWord.ParseReadElement(matchWords[0]);
                    var firstCorrectReading = KataHiraConvert.ConvertHiraToKata(readingElements[0].Word);
                    if (!readingStr.EqualsOrdinalIgnore(firstCorrectReading))
                    {
                        readingStr = firstCorrectReading;
                        pronunStr = readingStr;
                    }
                }
            }
        }

        private void AppendWordPart(string surface, string reading, string pronunciation, string baseform)
        {
            this.surface.Append(surface);
            this.reading.Append(reading);
            this.pronunciation.Append(pronunciation);
            this.baseform.Append(baseform);
        }

        private void RemoveLastWordPath()
        {
            var surfaceLength = words[endIndex].Surface.Length;
            surface.Remove(surface.Length - surfaceLength, surfaceLength);

            var readingLength = words[endIndex].Reading.Length;
            reading.Remove(reading.Length - readingLength, readingLength);

            var pronunLength = words[endIndex].Pronunciation.Length;
            pronunciation.Remove(pronunciation.Length - pronunLength, pronunLength);

            var baseformLength = GetBaseForm(words[endIndex]).Length;
            baseform.Remove(baseform.Length - baseformLength, baseformLength);
        }

        public static bool IsSpecialChar(WordInformation word)
        {
            return word.Surface.Length == 1 && IsSpecialChar(word.Surface[0]);
        }

        public static bool IsSpecialChar(char c)
        {
            return specialChars.Contains(c);
        }

        private string GetBaseForm(WordInformation word)
        {
            if (WordInformation.IsHave(word.BaseForm))
                return word.BaseForm;
            return word.Surface;
        }

        private void ClearPreviousStrings()
        {
            surface.Clear();
            baseform.Clear();
            pronunciation.Clear();
            reading.Clear();
        }

        private void InitNewStartWord(List<WordInformation> words)
        {
            surface.Append(words[startIndex].Surface);
            baseform.Append(words[startIndex].Surface);
            pronunciation.Append(words[startIndex].Pronunciation);
            reading.Append(words[startIndex].Reading);
        }

        private static bool IsValidVerb(WordInformation word)
        {
            if (!word.IsVerb())
                return false;

            if (word.IsBaseForm())
                return true;

            if (!String.IsNullOrWhiteSpace(word.Conjugation))
                return true;

            return false;
        }
    }
}
