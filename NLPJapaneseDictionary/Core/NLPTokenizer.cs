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

using NLPJDict.ConvertClasses;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Interfaces;
using NLPJDict.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Core
{
    public class NLPTokenizer<T> where T : class, IToken
    {
        public ITokenizer<T> tokenizer;
        private string currentTokenizedSentence;
        private Database japDict;

        private List<WordInformation> unTokenizedWords;
        private Queue<int> undoSkip;

        public List<WordInformation> Words { get; private set; }


        public NLPTokenizer(ITokenizer<T> tokenizer, Database japDict)
        {
            this.tokenizer = tokenizer;
            this.japDict = japDict;
            unTokenizedWords = new List<WordInformation>();
            undoSkip = new Queue<int>();
        }

        public List<WordInformation> Tokenize(string tokenizedSentence)
        {
            currentTokenizedSentence = tokenizedSentence;
            unTokenizedWords.Clear();
            undoSkip.Clear();
            return GetWords();            
        }

        public List<WordInformation> TokenizeReducedSentence()
        {
            if (currentTokenizedSentence.Length < 2)
                return Words;

            int skip = 0;
            while (true)
            {
                FindUntokenWordInDict();

                if (Words.Count < unTokenizedWords.Count)
                    break;

                var index = unTokenizedWords.Count - 1;
                if (!unTokenizedWords[index].Surface.Equals(Words[index].Surface, StringComparison.OrdinalIgnoreCase))
                    break;

                if (currentTokenizedSentence.Length < 2)
                {
                    undoSkip.Enqueue(skip);
                    return Words;
                }

                skip++;
            }
            undoSkip.Enqueue(skip);

            return GetWords();
        }

        private void FindUntokenWordInDict()
        {
            bool isInDict = false;
            string previousWord = null;
            WordInformation word = null;
            for (int index = 0; index < currentTokenizedSentence.Length; index++)
            {
                string unTokenized = currentTokenizedSentence.Substring(0, index + 1);
                if (JmdictEntity.HasJapWord(unTokenized + "*", japDict))
                {
                    previousWord = unTokenized;
                    isInDict = true;
                }
                else
                {
                    if (isInDict)
                    {
                        isInDict = false;
                        while (!String.IsNullOrWhiteSpace(previousWord))
                        {
                            if (JmdictEntity.HasJapWord(previousWord, japDict))
                            {
                                word = GetUntokenWord(previousWord);
                                word.IsInDictionary = true;
                                unTokenizedWords.Add(word);
                                currentTokenizedSentence = currentTokenizedSentence.Remove(0, previousWord.Length);
                                isInDict = true;
                                break;
                            }
                            previousWord = previousWord.Remove(previousWord.Length - 1);
                        }
                    }
                    break;
                }
            }

            if (!isInDict)
            {
                word = GetUntokenWord(currentTokenizedSentence[0].ToString());
                unTokenizedWords.Add(word);
                currentTokenizedSentence = currentTokenizedSentence.Remove(0, 1);
            }
            else if (word == null)
            {
                word = GetUntokenWord(previousWord);
                word.IsInDictionary = true;
                unTokenizedWords.Add(word);                
                currentTokenizedSentence = currentTokenizedSentence.Remove(0, previousWord.Length);
            }
        }

        public List<WordInformation> UndoTokenizeReducedSentence()
        {
            if(unTokenizedWords.Count == 0)
                return Words;

            StringBuilder builder = new StringBuilder(currentTokenizedSentence);

            int skip = 0;
            if(undoSkip.Count > 0)
                skip = undoSkip.Dequeue();

            while (skip >= 0)
            {
                builder.Insert(0, unTokenizedWords.Last().Surface);
                unTokenizedWords.RemoveAt(unTokenizedWords.Count - 1);
                skip--;

                if (unTokenizedWords.Count == 0)
                    break;
            }

            currentTokenizedSentence = builder.ToString();
            return GetWords();            
        }

        private WordInformation GetUntokenWord(string unTokenized)
        {
            WordInformation word = new WordInformation(null, null, unTokenized, null);
            string reading;
            string pronunciation;
            GetUntokenReadingAndPronunication(unTokenized, out reading, out pronunciation);

            word.AddWordPart(unTokenized, reading.ToString(), pronunciation.ToString());
            return word;
        }

        private void GetUntokenReadingAndPronunication(string unTokenized, out string reading, out string pronunication)
        {
            if (StringHelper.IsHaveKanji(unTokenized))
            {
                var tokens = tokenizer.Tokenize(unTokenized);
                StringBuilder readingBuilder = new StringBuilder();
                StringBuilder pronunciationBuilder = new StringBuilder();
                foreach (var token in tokens)
                {
                    if (token.Reading.Equals("*", StringComparison.OrdinalIgnoreCase)
                        || token.Pronunciation.Equals("*", StringComparison.OrdinalIgnoreCase))
                    {
                        readingBuilder.Clear();
                        readingBuilder.Append(" ");
                        pronunciationBuilder.Clear();
                        pronunciationBuilder.Append(" ");
                        break;
                    }
                    readingBuilder.Append(token.Reading);
                    pronunciationBuilder.Append(token.Pronunciation);
                }
                reading = readingBuilder.ToString();
                pronunication = pronunciationBuilder.ToString();
            }
            else if (StringHelper.IsHiraganaOnly(unTokenized))
            {
                reading = KataHiraConvert.ConvertHiraToKata(unTokenized);
                pronunication = reading;
            }
            else if (StringHelper.IsKatakanaOnly(unTokenized))
            {
                reading = unTokenized;
                pronunication = unTokenized;
            }
            else
            {
                reading = "";
                pronunication = "";
            }
        }

        private List<WordInformation> GetWords()
        {
            var tokens = tokenizer.Tokenize(currentTokenizedSentence);
            Words = TokensDictSearcher.ConvertTokensToWords(tokens, japDict);

            if (unTokenizedWords.Count > 0)
                Words.InsertRange(0, unTokenizedWords);
            return Words;
        }

    }
}
