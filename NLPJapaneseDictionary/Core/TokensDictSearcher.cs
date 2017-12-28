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
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Interfaces;
using NLPJDict.KuromojiIpadic.Ipadic;
using NLPJDict.NLPJDictCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore
{
    public static class TokensDictSearcher
    {
        private const int VALID_LONGEST_MATCH_COUNT = 1;
        private const int MAX_VALID_LONGEST_MATCH_COUNT = 3;

        public delegate List<JmdictEntity> GetFromDictMethod(string word, Database dict);
        public static GetFromDictMethod DefaultGetJapMethod { get; set; } = JmdictEntity.GetJapMatchAll;
        public static GetFromDictMethod DefaultGetGlossMethod { get; set; } = JmdictEntity.GetByGlossAll;

        public static bool IsValidLongestWord(string surface, int entriesCount)
        {
            if(StringHelper.IsHaveKanji(surface))
            {
                if (entriesCount <= MAX_VALID_LONGEST_MATCH_COUNT)
                    return true;
            }
            else
            {
                if (entriesCount <= VALID_LONGEST_MATCH_COUNT)
                    return true;
            }
            return false;
        }

        public static List<WordInformation> ConvertTokensToWords<T>(List<T> tokens, Database dictionary) where T : class, IToken
        {
            List<WordInformation> words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            return words;
        }

        public static List<JmdictEntity> SearchTokenWord(WordInformation selectedWord, int selectedIndex, List<WordInformation> words, Database japEngDictionary)
        {
            List<JmdictEntity> results = TrySearchCompoundVerbs(selectedWord, selectedIndex, words, japEngDictionary);

            if (results == null || results.Count == 0)
                results = TrylongestWordSearch(selectedWord, selectedIndex, words, japEngDictionary);

            if (results == null || results.Count == 0)
                results = FindTokenPerfectMatchInDictionary(selectedWord, japEngDictionary);
            else
                results = results.Union(FindTokenPerfectMatchInDictionary(selectedWord, japEngDictionary), JmdictEntity.EqualComparer).ToList();

            var possibleWord = WordInformation.TryRemoveGodanPotential(selectedWord, japEngDictionary);
            if (possibleWord != null)
            {
                var newResults = FindTokenPerfectMatchInDictionary(possibleWord, japEngDictionary);
                results = results.Union(newResults, JmdictEntity.EqualComparer).ToList();
            }

            if (StringHelper.IsKatakanaOnly(selectedWord.Surface))
                results = TrySearchByConvertToHira(selectedWord, japEngDictionary, results);            

            if (results.Count == 0)
            {
                return FindTokenPartialMatchInDictionary(selectedWord, japEngDictionary);
            }
            else
                return results;
        }

        private static List<JmdictEntity> TrySearchCompoundVerbs(WordInformation currentSelectedWord, int selectedIndex, List<WordInformation> words, Database japEngDictionary)
        {
            if (currentSelectedWord.IsVerb() && currentSelectedWord.IsMasuConjugation()
                            && (selectedIndex < (words.Count - 1))
                            && words[selectedIndex + 1].IsVerb())
            {                
                var nextWord = words[selectedIndex + 1];
                
                //Remove potential conjugation if has to make sure word is in its most baseform
                var nextWordBase = WordInformation.TryRemoveGodanPotential(nextWord, japEngDictionary);
                if (nextWordBase != null)
                    nextWord = nextWordBase;

                string baseForm = currentSelectedWord.Surface + nextWord.BaseForm;
                var compoundWord = new WordInformation(nextWord.FirstConjugationType, nextWord.FirstConjugationForm, baseForm, nextWord.PartOfSpeech, false, nextWord.Conjugation);
                compoundWord.AddWordPart(currentSelectedWord.Surface + nextWord.Surface,
                                        currentSelectedWord.Reading + nextWord.Reading,
                                        currentSelectedWord.Pronunciation + nextWord.Pronunciation);
                return FindTokenPerfectMatchInDictionary(compoundWord, japEngDictionary);
            }
            return null;
        }

        private static List<JmdictEntity> TrylongestWordSearch(WordInformation currentSelectedWord, int selectedIndex, List<WordInformation> words, Database japEngDictionary)
        {
            WordListReducer reducer = new WordListReducer();
            var longestWord = reducer.ReduceOnce(selectedIndex, words, japEngDictionary, true);

            if (!longestWord.Surface.Equals(currentSelectedWord.Surface, StringComparison.OrdinalIgnoreCase))
            {
                var perfectMatches = FindTokenPerfectMatchInDictionary(longestWord, japEngDictionary, true);
                if (perfectMatches.Count <= VALID_LONGEST_MATCH_COUNT)
                    return perfectMatches;

                if (StringHelper.IsHaveKanji(longestWord.Surface) && perfectMatches.Count <= MAX_VALID_LONGEST_MATCH_COUNT)
                    return perfectMatches;

                return null;
            }

            return null;
        }

        private static List<JmdictEntity> TrySearchByConvertToHira(WordInformation selectedWord, Database japEngDictionary, List<JmdictEntity> perfectMatches)
        {
            var hira = KataHiraConvert.ConvertKataToHira(selectedWord.Surface);
            var word = new WordInformation(selectedWord.FirstConjugationType, selectedWord.FirstConjugationForm,
                                                        selectedWord.BaseForm, selectedWord.PartOfSpeech);
            word.AddWordPart(hira, selectedWord.Reading, selectedWord.Pronunciation);
            var entries = FindTokenPerfectMatchInDictionary(word, japEngDictionary);
            if (entries.Count > 0)
                return perfectMatches.Union(entries, JmdictEntity.EqualComparer).ToList();
            else                            
                return perfectMatches;            
        }

        public static List<JmdictEntity> FindByGloss(string inputWord, Database dictionary)
        {
            return DefaultGetGlossMethod(inputWord, dictionary);
        }

        public static List<JmdictEntity> FindJapNonToken(string inputWord, Database dictionary)
        {
            var perfectMatches = DefaultGetJapMethod(inputWord, dictionary);
            var partialMatches = DefaultGetJapMethod(inputWord + "*", dictionary);
            return perfectMatches.Union(partialMatches, JmdictEntity.EqualComparer).ToList();
        }

        public static List<JmdictEntity> FindTokenPerfectMatchInDictionary(WordInformation word, Database dictionary, bool isSkipSurfaceIfHasBase = false)
        {
            var results = GetMatchedWords(word, dictionary, "", isSkipSurfaceIfHasBase, word.Conjugation);
            ReoderFirstMatchedReadingIfNeeded(results, word);
            ReorderByPartOfSpeech(results, word);
            return results;
        }

        private static void ReoderFirstMatchedReadingIfNeeded(List<JmdictEntity> results, WordInformation word)
        {
            if (results.Count < 2)
                return;
            if (!StringHelper.IsHaveKanji(results[0].RepresentWord)
                || !StringHelper.IsHaveKanji(word.Surface))
                //If no kanji then already match with reading
                return;

            string hiragana = GetBaseFormReadingOfWordWithKanji(word);
            hiragana = "\"" + hiragana;
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].ReadElement.ContainsExtend(hiragana, StringComparison.OrdinalIgnoreCase))
                {
                    JmdictEntity removed = results[i];
                    results.RemoveAt(i);
                    results.Insert(0, removed);
                    break;
                }
            }
        }

        private static void ReorderByPartOfSpeech(List<JmdictEntity> results, WordInformation word)
        {
            if (results.Count < 2)
                return;

            string pos = word.GetPartOfSpeechInEnglish();
            if (String.IsNullOrEmpty(pos))
                return;
            
            List<JmdictEntity> removed = new List<JmdictEntity>();
            for(int i = 0; i < results.Count;)
            {
                if(word.IsVerb() && 
                    (!results[i].PartOfSpeech.Contains("Godan") || !results[i].PartOfSpeech.Contains("Ichidan"))
                  )
                {
                    removed.Add(results[i]);
                    results.RemoveAt(i);
                }
                else if (!results[i].PartOfSpeech.Contains(pos))
                {
                    removed.Add(results[i]);
                    results.RemoveAt(i);
                }
                else
                    i++;
            }
            results.AddRange(removed);
        }

        private static string GetBaseFormReadingOfWordWithKanji(WordInformation word)
        {
            string hiragana = KataHiraConvert.ConvertKataToHira(word.Reading);            
            string surface = word.Surface;
            if (surface.Length == 1)
                // if only one kanji exists then reading is already base form
                return hiragana;
            if (word.BaseForm == null)            
                return hiragana;            

            string baseForm = word.BaseForm;
            var surfaceIndex = surface.Length - 1;
            int readingIndex = hiragana.Length - 1;
            for (; surfaceIndex > 0 && readingIndex > 0; surfaceIndex--, readingIndex--)
            {
                if(surface[surfaceIndex] != hiragana[readingIndex])
                {
                    break;
                }
            }
            var startBaseReading = hiragana.Substring(0, readingIndex + 1);
            surfaceIndex++;
            if (surfaceIndex > baseForm.Length - 1)
            {                
                return hiragana.Substring(0, readingIndex);
            }
            else
            {
                return startBaseReading + baseForm.Substring(surfaceIndex);
            }            
        }

        public static List<JmdictEntity> FindTokenPartialMatchInDictionary(WordInformation word, Database dictionary, bool isSkipSurfaceIfHasBaseForm = false)
        {            
            return GetMatchedWords(word, dictionary, "*", isSkipSurfaceIfHasBaseForm);
        }

        private static List<JmdictEntity> GetMatchedWords(WordInformation word, Database dictionary, string queryCommmand, bool isSkipSurfaceIfHasBaseForm, string conjungation = null)
        {
            //Use Dictionary to ensure unique entries
            Dictionary<int, JmdictEntity> entries = new Dictionary<int, JmdictEntity>();
            if (WordInformation.IsHave(word.BaseForm))
            {
                if (word.IsMaybeAmbiguousGodan())
                {
                    var allVariants = GetPossibleGodanVerb(word);              
                    AddPossibleGodanVerbsDictionaryEntry(allVariants, dictionary, entries, conjungation);
                    if(allVariants.Count == 1)
                        AddPossibleSpecialSuruVerb(word, dictionary, conjungation, entries);
                }
                else
                {                 
                    if (word.IsIAdjectiveConjugation())
                        HandleIAdjective(word, dictionary, conjungation, entries);
                    else if (word.IsVerb())
                    {
                        HandleVerb(word, dictionary, conjungation, entries);
                    }
                    else if(word.IsAuxiliaryVerb())
                    {
                        AddVerbDictionaryEntry(word.BaseForm, dictionary, entries, conjungation);
                    }
                    else
                        AddAllDictionaryEntry(word.BaseForm + queryCommmand, dictionary, entries);
                }
            }
            if (word.BaseForm == null || entries.Count == 0 || !word.BaseForm.EqualsOrdinalIgnore(word.Surface))
            {                
                if(entries.Count == 0 || !isSkipSurfaceIfHasBaseForm)
                    AddAllDictionaryEntry(word.Surface + queryCommmand, dictionary, entries);
            }

            return entries.Values.ToList();
        }

        private static void AddPossibleGodanVerbsDictionaryEntry(List<string> searchWord, Database dictionary, Dictionary<int, JmdictEntity> entries, string conjungation)
        {
            var list = JmdictEntity.GetJapMatchListWordAndPOS(searchWord, JmdictEntity.POS_GODAN, dictionary);
            AddDictionaryEntry(list, entries, conjungation);
        }

        private static void AddPossibleSpecialSuruVerb(WordInformation word, Database dictionary, string conjungation, Dictionary<int, JmdictEntity> entries)
        {
            if (conjungation != null && word.BaseForm.EndsWith("す") && !word.Surface.EndsWith("せば"))
            {
                var newBaseform = word.BaseForm.Remove(word.BaseForm.Length - 1) + "する";
                var list = JmdictEntity.GetJapMatchWordAndPOS(newBaseform, dictionary, JmdictEntity.POS_VERB_SURU);
                AddDictionaryEntry(list, entries, conjungation);
            }
        }

        private static void HandleIAdjective(WordInformation word, Database dictionary, string conjungation, Dictionary<int, JmdictEntity> entries)
        {
            AddIAdjectiveDictionaryEntry(word.BaseForm, dictionary, entries, conjungation);
            if (word.BaseForm.EndsWith("たい"))
            {
                var maybeVerb = word.BaseForm.Remove(word.BaseForm.Length - 2, 2);
                maybeVerb += "る";
                string newConjungation = WordInformation.ToConjungationTag("-tai") + " ";
                if (conjungation != null)
                    newConjungation += conjungation;
                AddIchidanVerbDictionaryEntry(maybeVerb, dictionary, entries, newConjungation);
            }
        }

        private static void HandleVerb(WordInformation word, Database dictionary, string conjungation, Dictionary<int, JmdictEntity> entries)
        {
            if (!word.Reading.StartsWith("ナ", StringComparison.OrdinalIgnoreCase)
                && WordInformation.IsSpecialSuVerb(word.FirstConjugationType)
                && (word.BaseForm.Equals("為る", StringComparison.OrdinalIgnoreCase)
                    || word.BaseForm.Equals("する", StringComparison.OrdinalIgnoreCase)))
            { //Deal with suru verb alone to avoid showing noun with conjungation                        
                AddSuruVerb(word, dictionary, conjungation, entries);
            }
            else if ((word.BaseForm.Equals("来る", StringComparison.OrdinalIgnoreCase)
                        || word.BaseForm.Equals("くる", StringComparison.OrdinalIgnoreCase))
                   && WordInformation.IsSpecialKuVerb(word.FirstConjugationType))
            { //Deal with kuru alone
                AddKuruVerb(word, dictionary, conjungation, entries);
            }
            else
            {
                AddVerbExceptSuruKuruDictionaryEntry(word, dictionary, entries, conjungation);
                if (word.BaseForm.Equals("くる", StringComparison.OrdinalIgnoreCase))
                    AddKuruSpecialOnly(null, entries, dictionary);
                else if ((word.Surface.StartsWith("こら", StringComparison.OrdinalIgnoreCase) && word.BaseForm.Equals("こる", StringComparison.OrdinalIgnoreCase))
                       || (word.Surface.StartsWith("来ら", StringComparison.OrdinalIgnoreCase) && word.BaseForm.Equals("来る", StringComparison.OrdinalIgnoreCase)))
                {
                    if (conjungation != null && conjungation.Contains("[passive]"))
                    {
                        string newConjun = conjungation.Replace("[passive]", "[passive or potential]");
                        AddKuruSpecialOnly(newConjun, entries, dictionary);
                    }
                }
                else if (word.Surface.StartsWith("来さ", StringComparison.OrdinalIgnoreCase) && word.BaseForm.Equals("来す", StringComparison.OrdinalIgnoreCase))
                {
                    if (conjungation != null && conjungation.Contains("causative"))
                        AddKuruSpecialOnly(conjungation, entries, dictionary);
                }
                else if (word.Surface.StartsWith("こさ", StringComparison.OrdinalIgnoreCase) && word.BaseForm.Equals("こす", StringComparison.OrdinalIgnoreCase))
                {
                    if (conjungation != null && conjungation.Contains("causative"))                                            
                        AddKuruSpecialOnly(conjungation, entries, dictionary);                    
                }
            }
        }

        private static void AddVerbDictionaryEntry(string searchWord, Database dictionary, Dictionary<int, JmdictEntity> entries, string conjungation)
        {
            var list = JmdictEntity.GetJapMatchVerb(searchWord, dictionary);
            AddDictionaryEntry(list, entries, conjungation);
        }

        private static void AddAllDictionaryEntry(string searchWord, Database dictionary, Dictionary<int, JmdictEntity> entries)
        {
            var list = JmdictEntity.GetJapMatchAll(searchWord, dictionary);
            AddDictionaryEntry(list, entries, null);
        }

        private static void AddSuruVerb(WordInformation word, Database dictionary, string conjungation, Dictionary<int, JmdictEntity> entries)
        {
            if (word.Surface.Equals("すれば", StringComparison.OrdinalIgnoreCase))
            {
                var list = JmdictEntity.GetJapMatchVerb(word.BaseForm, dictionary);
                AddDictionaryEntry(list, entries, conjungation);
            }
            else
            {
                var entry = JmdictEntity.GetSpecialSuruVerb(dictionary);
                entry.Conjugation = conjungation;
                entries[entry.EntrySequence] = entry;
            }
        }

        private static void AddKuruVerb(WordInformation word, Database dictionary, string conjungation, Dictionary<int, JmdictEntity> entries)
        {
            if (word.Reading.StartsWith("コラ", StringComparison.OrdinalIgnoreCase))
            {
                AddKuruSpecialOnly(conjungation, entries, dictionary);

                string searchWord;                
                if (StringHelper.IsHaveKanji(word.BaseForm))
                    searchWord = word.BaseForm;
                else
                    searchWord = "こる";
                var list = JmdictEntity.GetJapMatchGodanVerb(searchWord, dictionary);
                var newConjungation = conjungation.Replace("[passive or potential]", "[passive]");
                AddDictionaryEntry(list, entries, newConjungation);
            }
            else if (word.Reading.StartsWith("コサ", StringComparison.OrdinalIgnoreCase))
            {
                AddKuruSpecialOnly(conjungation, entries, dictionary);

                string searchWord;
                if (StringHelper.IsHaveKanji(word.BaseForm))
                    searchWord = "来す";
                else
                    searchWord = "こす";                
                var list = JmdictEntity.GetJapMatchGodanVerb(searchWord, dictionary);
                AddDictionaryEntry(list, entries, conjungation);
            }
            else if (word.Reading.StartsWith("コ", StringComparison.OrdinalIgnoreCase))
            {
                AddKuruSpecialOnly(conjungation, entries, dictionary);
            }
            else if (word.Reading.StartsWith("キ", StringComparison.OrdinalIgnoreCase))
            {
                AddKuruSpecialOnly(conjungation, entries, dictionary);
                if (word.Surface.StartsWith("き", StringComparison.OrdinalIgnoreCase))
                {
                    var list = JmdictEntity.GetJapMatchIChidan("きる", dictionary);
                    AddDictionaryEntry(list, entries, conjungation);
                }
            }
            else
            {
                var list = JmdictEntity.GetJapMatchVerb(word.BaseForm, dictionary);
                AddDictionaryEntry(list, entries, conjungation);
            }
        }

        private static void AddKuruSpecialOnly(string conjungation, Dictionary<int, JmdictEntity> entries, Database dictionary)
        {
            var entry = JmdictEntity.GetSpecialKuruVerb(dictionary);
            entry.Conjugation = conjungation;
            entries[entry.EntrySequence] = entry;                
        }

        private static void AddIAdjectiveDictionaryEntry(string searchWord, Database dictionary, Dictionary<int, JmdictEntity> entries, string conjungation)
        {
            var list = JmdictEntity.GetJapMatchIAdjective(searchWord, dictionary);
            AddDictionaryEntry(list, entries, conjungation);
        }

        private static void AddVerbExceptSuruKuruDictionaryEntry(WordInformation word, Database dictionary, Dictionary<int, JmdictEntity> entries, string conjungation)
        {
            List<JmdictEntity> list;
            if (word.IsBaseForm())
            {
                list = JmdictEntity.GetJapMatchWordAndPOS(word.BaseForm, dictionary, JmdictEntity.POS_VERB_EXCEPT_SURU_KURU);
                AddDictionaryEntry(list, entries, conjungation);
                return;
            }
            else if (word.IsGodanConjugation())
            {
                list = JmdictEntity.GetJapMatchWordAndPOS(word.BaseForm, dictionary, JmdictEntity.POS_VERB_GODAN_EXCEPT_SURU_KURU);
                AddDictionaryEntry(list, entries, conjungation);
                AddPossibleSpecialSuruVerb(word, dictionary, conjungation, entries);
                return;
            }
            else if (word.IsIchidanConjugation())
            {
                list = JmdictEntity.GetJapMatchWordAndPOS(word.BaseForm, dictionary, JmdictEntity.POS_VERB_ICHIDAN_EXCEPT_SURU_KURU);
                AddDictionaryEntry(list, entries, conjungation);
                return;
            }
            else if(word.BaseForm.EndsWith("する") || word.BaseForm.EndsWith("くる") || word.BaseForm.EndsWith("来る"))
            { //Special Suru Verb
                list = JmdictEntity.GetJapMatchVerb(word.BaseForm, dictionary);
                AddDictionaryEntry(list, entries, conjungation);
                return;
            }                    
        }

        private static void AddIchidanVerbDictionaryEntry(string searchWord, Database dictionary, Dictionary<int, JmdictEntity> entries, string conjungation)
        {
            var list = JmdictEntity.GetJapMatchIChidan(searchWord, dictionary);
            AddDictionaryEntry(list, entries, conjungation);
        }

        private static void AddDictionaryEntryWithDefaultMethod(string searchWord, Database dictionary, Dictionary<int, JmdictEntity> entries)
        {
            var list = DefaultGetJapMethod(searchWord, dictionary);
            AddDictionaryEntry(list, entries, null);
        }

        private static void AddDictionaryEntry(List<JmdictEntity> source, Dictionary<int, JmdictEntity> destination, string conjungation)
        {
            foreach (var entry in source)
            {
                entry.Conjugation = conjungation;
                destination[entry.EntrySequence] = entry;
            }
        }

        private static List<string> GetPossibleGodanVerb(WordInformation word)
        {
            List<string> results = new List<string>();
            int lastIndex = word.BaseForm.Length - 1;
            var lastLetter = word.BaseForm[lastIndex];
            if (lastLetter.Equals('ぶ') || lastLetter.Equals('む') || lastLetter.Equals('ぬ'))
            {
                StringBuilder builder = new StringBuilder(word.BaseForm);
                AddNewGodanVerb(results, lastIndex, builder, 'ぶ');
                AddNewGodanVerb(results, lastIndex, builder, 'む');
                AddNewGodanVerb(results, lastIndex, builder, 'ぬ');
            }
            else if (lastLetter.Equals('る') || lastLetter.Equals('つ') || lastLetter.Equals('う') 
                     || word.BaseForm.EqualsOrdinalIgnore("いく")
                     || (word.BaseForm[0].Equals('行') && word.BaseForm.Length == 2))
            {                
                if (word.BaseForm.Length == 2)
                {
                    if (word.BaseForm[0].Equals('い') || word.BaseForm[0].Equals('行'))
                        results.Add("行く"); //iku exception                                        
                }

                StringBuilder builder = new StringBuilder(word.BaseForm);
                AddNewGodanVerb(results, lastIndex, builder, 'る');
                AddNewGodanVerb(results, lastIndex, builder, 'つ');
                AddNewGodanVerb(results, lastIndex, builder, 'う'); 
            }
            else
                results.Add(word.BaseForm);

            return results;
        }

        private static void AddNewGodanVerb(List<string> results, int index, StringBuilder builder, char endLetter)
        {
            builder[index] = endLetter;
            results.Add(builder.ToString());
        }

    }
    
}
