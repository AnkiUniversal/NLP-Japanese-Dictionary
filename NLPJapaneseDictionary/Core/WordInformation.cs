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
using NLPJapaneseDictionary.Kuromoji.Core.util;
using NLPJapaneseDictionary.Kuromoji.Interfaces;
using NLPJapaneseDictionary.Core;
using NLPJapaneseDictionary.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Core
{
    public class WordInformation
    {
        //Translate from Japanese
        //連用形 -masu stem
        //未然形 imperfective form
        //連用タ接続 conjunctive ta connection
        //連用テ接続 conjunctive te connection
        //連用デ接続 conjunctive de connection
        //未然ウ接続 conjunctive u connection
        //未然レル接続 suru - sa(reru) form
        //命令ｉ imperative
        //命令ｒｏ imperative
        //仮定形 hypothetical form    

        public const string JAP_BASE_FORM = "基本形";        
        public const string JAP_SYMBOL = "記号";
        public const string JAP_PRONOUN = "代名詞";
        public const string JAP_NOUN = "名詞";
        public const string JAP_PARTICLE = "助詞";
        public const string JAP_GODAN = "五段";
        public const string JAP_ICHIDAN = "一段";
        public const string JAP_AUXILIARY = "補助";
        public const string JAP_AUXILIARY_VERB = "助動詞";
        public const string JAP_VERB = "動詞";
        public const string JAP_ADJECTIVE = "形容詞";
        public const string JAP_ADVERB = "副詞";
        public const string JAP_KURU_CONJ = "カ変・クル";
        public const string JAP_KURU_KANJI_CONJ = "カ変・来ル";
        public const string JAP_SURU_CONJ = "サ変・スル";
        public const string JAP_HYPOTHETICAL = "仮定形";
        public const string JAP_IMPERATIVE_I = "命令ｉ";
        public const string JAP_IMPERATIVE_YO = "命令ｙｏ";
        public const string JAP_PRENOUN_ADJECTIVE = "連体詞";
        public const string JAP_CONJUGATION = "接続詞";
        public const string JAP_IMPERFECTIVE = "未然形";
        public const string JAP_TAI_FROM = "特殊・タイ";
        public const string JAP_MASU_STEM = "連用形";        

        private const string CAUSATIVE = "causative";
        private const string IMPERATIVE = "imperative";
        private const string POTENTIAL = "potential";

        private const string JAP_TE_FORM = "連用テ接続";
        private const string JAP_IADJ_AUO = "形容詞・アウオ段";
        private const string JAP_OU_FORM = "未然ウ接続";            

        private bool hasCausative = false;
        private bool hasImperative = false;
        private bool hasTeiru = false;

        private bool isSurfaceChange = true;
        private bool isReadingChange = true;
        private bool isPronunChange = true;

        private static readonly Dictionary<string, string> ichidanConjungationMap;
        private static readonly Dictionary<string, string> imperfectiveConjungationMap;
        private static readonly Dictionary<string, string> masuStemConjungationMap;

        private static readonly string[] conjungationList = new string[] { "ます", "たい", "ない", "ん", "ぬ", "た", "だ", "て", "で",
                                                                          "ば", "う", "まい", "だら", "たら", "られる", "させる", "れる", "せる",
                                                                          "なさる", "てる", "でる", "さ", "たり", "だり", "そう", "がる"};

        public string FirstConjugationType { get; private set; }
        public string FirstConjugationForm { get; private set; }

        public bool IsInDictionary { get; set; } = false;

        private StringBuilder surface;
        private string surfaceString;
        public string Surface
        {
            get
            {
                if (surfaceString == null || isSurfaceChange)
                {
                    surfaceString = GetString(surface);
                    isSurfaceChange = false;
                }
                return surfaceString;
            }
        }

        private StringBuilder conjugation;
        public string Conjugation
        { get { return GetConjunction(); } }

        private StringBuilder reading;
        private string readingString;
        public string Reading
        {
            get
            {
                if (readingString == null || isReadingChange)
                {
                    readingString = GetString(reading);
                    isReadingChange = false;
                }
                return readingString;
            }
        }

        private StringBuilder pronunciation;
        private string pronunString;
        public string Pronunciation
        {
            get
            {
                if (pronunString == null || isPronunChange)
                {
                    pronunString = GetString(pronunciation);
                    isPronunChange = false;
                }
                return pronunString;
            }
        }        

        public bool IsHaveSplitWords { get; private set; } = false;
        private List<WordInformation> splitWords = null;
        public List<WordInformation> SplitWords
        {
            get
            {
                if (splitWords == null)                
                    splitWords = new List<WordInformation>();

                return splitWords;
            }
        }

        public string BaseForm { get; private set; }
        public string PartOfSpeech { get; private set; }        

        public bool IsUnknownWord { get; set; }        
        public int LinkWordGroup { get; set; } 

        static WordInformation()
        {                             
            imperfectiveConjungationMap = new Dictionary<string, string>();
            imperfectiveConjungationMap.Add("ない", "negative");
            imperfectiveConjungationMap.Add("れる", "passive");
            imperfectiveConjungationMap.Add("せる", "causative");
            imperfectiveConjungationMap.Add("ん", "negative");
            imperfectiveConjungationMap.Add("ぬ", "negative");            

            masuStemConjungationMap = new Dictionary<string, string>();
            masuStemConjungationMap.Add("ます", "polite");
            masuStemConjungationMap.Add("たい", "-tai");
            masuStemConjungationMap.Add("そう", "-sou");
            masuStemConjungationMap.Add("まい", "negative volitional");
            masuStemConjungationMap.Add("なさる", "imperative");

            ichidanConjungationMap = new Dictionary<string, string>();
            ichidanConjungationMap.Add("られる", "passive or potential");
            ichidanConjungationMap.Add("させる", "causative");
            ichidanConjungationMap.Add("ない", "negative");
            ichidanConjungationMap.Add("ん", "negative");
            ichidanConjungationMap.Add("ぬ", "negative");
            ichidanConjungationMap.Add("た", "past");
            ichidanConjungationMap.Add("て", "-te");
            ichidanConjungationMap.Add("まい", "negative volitional");
            ichidanConjungationMap.Add("てる", "-te iru");
            ichidanConjungationMap.Add("たり", "-tari");
        }

        public WordInformation(string firstConjugationType, string firstConjugationForm, string baseForm, string partOfSpeech, bool isUnknowWord = false, string conjugation = "")
        {
            surface = new StringBuilder();
            this.conjugation = new StringBuilder(conjugation);
            reading = new StringBuilder();
            pronunciation = new StringBuilder();
            BaseForm = baseForm;
            PartOfSpeech = partOfSpeech;
            IsUnknownWord = isUnknowWord;
            FirstConjugationForm = firstConjugationForm;
            FirstConjugationType = firstConjugationType;

            SetAllChange();
        }

        /// <summary>
        /// This function should be called when needed to convert tokens to a WordInformation list
        /// </summary>
        /// <param name="token">Token to convert</param>
        /// <param name="nextToken">Next token in the list</param>
        /// <param name="database">Japanese-English dictionary database</param>
        public WordInformation(IToken token, IToken nextToken, Database database)
        {
            FirstConjugationType = token.ConjugationType;
            FirstConjugationForm = token.ConjugationForm;            
            BaseForm = token.BaseForm;
            PartOfSpeech = token.PartOfSpeech;
            surface = new StringBuilder(token.Surface);            
            reading = new StringBuilder(token.Reading);
            pronunciation = new StringBuilder(token.Pronunciation);
            conjugation = new StringBuilder();
            SetAllChange();

            if (token.NodeType == Kuromoji.Core.Viterbi.ViterbiNode.NodeType.UNKNOWN)
                IsUnknownWord = true;

            if (TryAppendImperativeOrPotentialConjungationIfNeeded(token, nextToken, database))
                return;
        }

        /// <summary>
        /// This function should be called after completely processing all tokens to a WordInformation list
        /// to make sure all undetect or mismatch words are correct.
        /// </summary>
        /// <param name="database">Japanese-English dictionary database</param>
        public void FinalizeWord(Database database)
        {
            ClearSplitWordsList();

            if (TrySuruUndetectConjugation())
                return;

            if (TryKuruUndetectConjugation())
                return;

            CheckConjunction();
            SetIsInDictionary(database);

            if (TryUndetectAdverbConjugation(database))
                return;

            TryCorrectUnknownWordStatusIfNeeded();

            TryCorrectPronunIfNeeded();

            TrySplitUnknowWordIfNeeded(database);
        }

        private void ClearSplitWordsList()
        {
            splitWords = null;
            IsHaveSplitWords = false;
        }
        private bool TrySuruUndetectConjugation()
        {
            if (!BaseForm.Equals("したら") || !Surface.Equals("したら"))
                return false;

            if (!IsHave(FirstConjugationType) && !IsHave(FirstConjugationForm))
            {
                AppendConjungation("-tara");
                BaseForm = "為る";
                FirstConjugationType = JAP_SURU_CONJ;
                FirstConjugationForm = JAP_HYPOTHETICAL;
                PartOfSpeech = JAP_VERB;
                IsInDictionary = true;
                return true;
            }

            return false;
        }
        private bool TryKuruUndetectConjugation()
        {
            if ((BaseForm.Equals("こい") && Surface.Equals("こい")))
            {
                AppendConjungation(IMPERATIVE);
                BaseForm = "くる";
                FirstConjugationType = JAP_KURU_CONJ;
                FirstConjugationForm = JAP_IMPERATIVE_I;
                PartOfSpeech = JAP_VERB;
                IsInDictionary = true;
                return true;
            }
            else if (BaseForm.Equals("来い") && Surface.Equals("来い"))
            {
                AppendConjungation(IMPERATIVE);
                BaseForm = "来る";
                FirstConjugationType = JAP_KURU_KANJI_CONJ;
                FirstConjugationForm = JAP_IMPERATIVE_I;
                PartOfSpeech = JAP_VERB;
                IsInDictionary = true;
                return true;
            }
            return false;
        }
        private void SetIsInDictionary(Database database)
        {
            if (!IsInDictionary)
            {
                if (IsHave(BaseForm))
                {
                    string searchWord = BaseForm + "*" + " OR " + Surface + "*";
                    IsInDictionary = JmdictEntity.HasJapWord(searchWord, database);
                }
                else
                    IsInDictionary = JmdictEntity.HasJapWord(Surface + "*", database);

                if (IsInDictionary)
                    return;

                if(StringHelper.IsKatakanaOnly(Surface))
                {
                    var hira = KataHiraConvert.ConvertKataToHira(Surface);
                    IsInDictionary = JmdictEntity.HasJapWord(hira + "*", database);
                    if (IsInDictionary)
                        return;
                }
            }
        }  
        private bool TryUndetectAdverbConjugation(Database database)
        {
            if ( IsInDictionary
                || IsHave(FirstConjugationForm)                
                || IsHave(FirstConjugationType)
                || !IsAdverb()     
                || !BaseForm.Equals(Surface, StringComparison.OrdinalIgnoreCase))
                return false;

            if (BaseForm.EndsWith("く", StringComparison.OrdinalIgnoreCase))
            {
                string newBaseBorm = BaseForm.Remove(BaseForm.Length - 1) + "い";
                if (!JmdictEntity.HasJapPOS(newBaseBorm, database, JmdictEntity.POS_IADJ))
                    return false;

                AppendConjungation("adverb");
                BaseForm = newBaseBorm;
                FirstConjugationType = JAP_IADJ_AUO;
                FirstConjugationForm = JAP_TE_FORM;
                IsInDictionary = true;
                return true;
            }
            else if(BaseForm.EndsWith("と", StringComparison.OrdinalIgnoreCase))
            {
                string newBaseBorm = BaseForm.Remove(BaseForm.Length - 1);
                if (!JmdictEntity.HasJapPOS(newBaseBorm, database, JmdictEntity.POS_ADV))
                    return false;
                
                BaseForm = newBaseBorm;
                IsInDictionary = true;
                return true;
            }

            return false;
        }
        private void CheckConjunction()
        {
            if (conjugation.Length == 0)
            {
                if (Surface.Equals(BaseForm, StringComparison.OrdinalIgnoreCase))
                    return;

                if (TryAppendImperative(Surface, FirstConjugationForm))
                    return;

                if (TryAppendIAdjective(Surface))
                    return;

                if (IsMasuStemForm()
                    && (IsGodanConjugation() || IsIchidanConjugation() || IsKuruOrSuruConjugation()))
                {
                    AppendConjungation("masu stem");
                    return;
                }
            }

            if (TryAppendIchidanYoImperative())
                return;
        }
        private bool TryCorrectUnknownWordStatusIfNeeded()
        {
            if (!IsUnknownWord)
                return false;

            string result;
            if (StringHelper.IsKatakanaOnly(Surface))
                result = Surface;
            else if (StringHelper.IsHiraganaOnly(Surface))
                result = KataHiraConvert.ConvertHiraToKata(Surface);
            else if (IsInDictionary && StringHelper.IsKanjiOnly(Surface))            
                result = "*";                
            else
                return false;

            reading.Clear();
            reading.Append(result);
            pronunciation.Clear();
            pronunciation.Append(result);

            isReadingChange = true;
            isPronunChange = true;
            IsUnknownWord = false;
            return true;
        }
        private bool TryCorrectPronunIfNeeded()
        {
            if (IsHave(Pronunciation))
                return false;

            pronunciation.Clear();
            pronunciation.Append(Reading);
            
            isPronunChange = true;            
            return true;
        }
        private bool TryAppendIAdjective(string original)
        {
            if (IsIAdjectiveConjugation(FirstConjugationType))
            {
                if (original.EndsWith("さ", StringComparison.OrdinalIgnoreCase))
                {
                    AppendConjungation("i-adj. as noun");
                    return true;
                }
                else if (original.EndsWith("く", StringComparison.OrdinalIgnoreCase))
                {
                    AppendConjungation("adverb");
                    return true;
                }
            }
            return false;
        }
        private bool TryAppendIchidanYoImperative()
        {
            if (IsIchidanConjugation() && Surface.EndsWith("よ") && !hasImperative)
            {
                AppendConjungation(IMPERATIVE);
                hasImperative = true;
                return true;
            }
            return false;
        }
        private void TrySplitUnknowWordIfNeeded(Database database)
        {
            if (IsInDictionary)
                return;

            if ((Surface.Length != 1)
                            && !IsSymbol()
                            && !StringHelper.IsKanjiOnly(Surface)
               )
            {
                TrySplitUnknownWord(database);
            }
        }
        private void TrySplitUnknownWord(Database database)
        {
            for (int i = Surface.Length - 1; i > 0; i--)
            {
                string firstHalf = surface.ToString(0, i);
                var firstHalfMatches = JmdictEntity.GetJapMatchAll(firstHalf, database);
                if (firstHalfMatches.Count > 0)
                {
                    string secondHalf = surface.ToString(i, Surface.Length - i);
                    var secondHalfmatches = JmdictEntity.GetJapMatchAll(secondHalf, database);
                    if (secondHalfmatches.Count > 0)
                    {
                        AddFirstHalfWord(firstHalf, firstHalfMatches);
                        AddSecondHalfWord(secondHalf, secondHalfmatches);
                        break;
                    }
                }
            }
        }
        private void AddFirstHalfWord(string searchWord, List<JmdictEntity> matches)
        {
            string katakanaReading = GetKatakanaReading(searchWord, matches);

            surface.Clear();
            reading.Clear();
            pronunciation.Clear();
            surface.Append(searchWord);
            reading.Append(katakanaReading);
            pronunciation.Append(katakanaReading);
            IsInDictionary = true;
            SetAllChange();
        }
        private void AddSecondHalfWord(string searchWord, List<JmdictEntity> matches)
        {
            string katakanaReading = GetKatakanaReading(searchWord, matches);

            WordInformation splitWord = new WordInformation(null, null, searchWord, null);
            splitWord.AddWordPart(searchWord, katakanaReading, katakanaReading);
            SplitWords.Add(splitWord);
            IsHaveSplitWords = true;
        }
        private static string GetKatakanaReading(string searchWord, List<JmdictEntity> matches)
        {
            string katakanaReading;
            if (StringHelper.IsKatakanaOnly(searchWord))
                katakanaReading = searchWord;
            else if (!StringHelper.IsHaveKanji(searchWord))
                katakanaReading = KataHiraConvert.ConvertHiraToKata(searchWord);
            else
                katakanaReading = GetReadingFromDictEntry(matches);
            return katakanaReading;
        }
        private static string GetReadingFromDictEntry(List<JmdictEntity> matches)
        {
            if (!StringHelper.IsHaveKanji(matches[0].RepresentWord))
                return KataHiraConvert.ConvertHiraToKata(matches[0].RepresentWord);

            var readingElements = JmdictWord.ParseReadElement(matches[0]);
            var firstCorrectReading = KataHiraConvert.ConvertHiraToKata(readingElements[0].Word);
            return firstCorrectReading;
        }

        public void AddWordPart(IToken token)
        {
            surface.Append(token.Surface);
            reading.Append(token.Reading);
            pronunciation.Append(token.Pronunciation);

            SetAllChange();
        }

        public void AddWordPart(string surface, string reading, string pronunciation)
        {
            this.surface.Append(surface);
            this.reading.Append(reading);
            this.pronunciation.Append(pronunciation);

            SetAllChange();
        }     

        public bool TryAddConjungationPart(IToken previousToken, IToken token, IToken nextToken)
        {
            if (IsAdjective())
            {
                if (TryCommonAdjConjungation(previousToken, token))
                    return true;

                return false;
            }

            if (TryTaraConjungation(previousToken, token))
                return true;

            if (TryRareruConjungation(previousToken, token))
            {
                TryAppendRoImperative(token.Surface, token.ConjugationForm);
                return true; 
            }

            if (TryDeIruConjungation(previousToken, token, nextToken))
                return true;

            if (TryVerbCommonConjungation(previousToken, token, nextToken))
            {
                TryAppendRoImperative(token.Surface, token.ConjugationForm);
                return true;
            }

            if (TryMasendeConjungation(previousToken, token))
                return true;

            if (TryDewanaiCombine(previousToken, token, nextToken))
                return true;

            if (TryDeAruCombine(previousToken, token))
                return true;

            return TryAppendImperative(token.Surface, token.ConjugationForm);
        }

        private bool TryAppendImperativeOrPotentialConjungationIfNeeded(IToken token, IToken nextToken, Database database)
        {
            if (FirstConjugationType.ContainsExtend("一段", StringComparison.OrdinalIgnoreCase))
            {
                if (!JmdictEntity.HasJapWord(token.BaseForm, database))
                {
                    int startCutPosition = surface.Length - 1;
                    bool isSurfaceInBaseForm;
                    bool isYoForm = false;

                    if (token.ConjugationForm.Equals(JAP_IMPERATIVE_YO, StringComparison.OrdinalIgnoreCase) 
                        && token.Surface.EndsWith("よ"))
                    {
                        if ( (nextToken == null)
                            || (nextToken.NodeType == Kuromoji.Core.Viterbi.ViterbiNode.NodeType.UNKNOWN)
                            || IsSymbol(nextToken))                        
                            SplitYoWord(startCutPosition);                       
                        else
                            isYoForm = true;
                        startCutPosition--;
                        isSurfaceInBaseForm = false;                        
                    }
                    else
                    {
                        isSurfaceInBaseForm = token.ConjugationForm.Equals(JAP_BASE_FORM, StringComparison.OrdinalIgnoreCase);
                        if (isSurfaceInBaseForm) //Baseform final letter is "る" so move back one letter
                            startCutPosition--;
                    }

                    string hiraLetter = surface[startCutPosition].ToString();
                    if (!StringHelper.IsHiraganaOnly(hiraLetter))
                        return false;

                    var romaji = RomaConvert.ConvertOneHiraToRoma(hiraLetter);
                    if (romaji.EndsWith("e", StringComparison.OrdinalIgnoreCase))
                    {
                        string uFormHira;
                        if (romaji.Length == 1)
                            uFormHira = "う";
                        else if (romaji.Equals("te", StringComparison.OrdinalIgnoreCase))
                            uFormHira = "つ";
                        else
                        {
                            string uFormRoma = romaji.Remove(romaji.Length - 1, 1) + "u";
                            uFormHira = RomaConvert.ConvertOneRomaToHira(uFormRoma);
                        }
                        var realBaseForm = surface.ToString(0, startCutPosition) + uFormHira;
                        if (JmdictEntity.HasJapWord(realBaseForm, database))
                        {
                            if(isYoForm)
                            {
                                AppendConjungation(POTENTIAL);
                                if (!hasImperative)
                                {
                                    AppendConjungation(IMPERATIVE);
                                    hasImperative = true;
                                }
                            }
                            else if (isSurfaceInBaseForm 
                                || (nextToken != null && IsInConjugationList(nextToken.BaseForm)))
                                AppendConjungation(POTENTIAL);
                            else if (!hasImperative)
                            {
                                AppendConjungation(IMPERATIVE);
                                hasImperative = true;
                            }
                            IsInDictionary = true;
                            BaseForm = realBaseForm;
                            FirstConjugationType = JAP_GODAN;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void SplitYoWord(int startCutPosition)
        {
            WordInformation splitWord = new WordInformation(null, null, "よ", JAP_PARTICLE);
            splitWord.AddWordPart("よ", "ヨ", "ヨ");
            surface.Remove(startCutPosition, 1);
            reading.Remove(reading.Length - 1, 1);
            pronunciation.Remove(pronunciation.Length - 1, 1);
            SplitWords.Add(splitWord);
            IsHaveSplitWords = true;
        }

        public static WordInformation TryRemoveGodanPotential(WordInformation word, Database database)
        {            
            //If word is in potential conjugation already then no process to avoid double potential
            if (word.IsIchidanConjugation() && !word.Conjugation.ContainsExtend("potential", StringComparison.OrdinalIgnoreCase))
            {
                int startCutPosition = word.BaseForm.Length - 2;                                

                string hiraLetter = word.BaseForm[startCutPosition].ToString();
                if (!StringHelper.IsHiraganaOnly(hiraLetter))
                    return null;

                var romaji = RomaConvert.ConvertOneHiraToRoma(hiraLetter);
                if (romaji.EndsWith("e", StringComparison.OrdinalIgnoreCase))
                {
                    string uFormHira;
                    if (romaji.Length == 1)
                        uFormHira = "う";
                    else if (romaji.Equals("te", StringComparison.OrdinalIgnoreCase))
                        uFormHira = "つ";
                    else
                    {
                        string uFormRoma = romaji.Remove(romaji.Length - 1, 1) + "u";
                        uFormHira = RomaConvert.ConvertOneRomaToHira(uFormRoma);
                    }
                    var realBaseForm = word.surface.ToString(0, startCutPosition) + uFormHira;
                    if (JmdictEntity.HasJapWord(realBaseForm, database))
                    {
                        string conjugation;
                        if (word.surface.Length >= word.BaseForm.Length)
                            conjugation = "[" + POTENTIAL + "] " + word.Conjugation;
                        else
                            conjugation = "[" + IMPERATIVE + "] ";                                                    
                        string BaseForm = realBaseForm;
                        string FirstConjugationType = JAP_GODAN;

                        WordInformation newWord = new WordInformation(FirstConjugationType, word.FirstConjugationForm,
                                                                      BaseForm, word.PartOfSpeech, false, conjugation);
                        newWord.IsInDictionary = true;
                        newWord.AddWordPart(word.Surface, word.Reading, word.Pronunciation);
                        return newWord;
                    }
                }
            }
            return null;
        }

        private bool TryTaraConjungation(IToken previousToken, IToken token)
        {
            if (!IsTokenInPossibleTaForm(previousToken))
                return false;

            if (token.ConjugationForm.Equals("仮定形", StringComparison.OrdinalIgnoreCase))
            {
                if(IsGodanConjugation(previousToken) && IsBaseformEndInMuNuBuGu(previousToken))
                {
                    if (!token.Surface.Equals("だら", StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                else if (!token.Surface.Equals("たら", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                AppendConjungation("-tara");
                return true;
            }
            return false;
        }

        private bool TryRareruConjungation(IToken previousToken, IToken token)
        {
            if (IsIchidanConjugation(previousToken) && token.BaseForm.Equals("られる", StringComparison.OrdinalIgnoreCase))
            {
                if (hasCausative)
                    AppendConjungation("passive");
                else
                    AppendConjungation("passive or potential");
                return true;
            }
            return false;
        }

        private bool TryDeIruConjungation(IToken previousToken, IToken token, IToken nextToken)
        {
            if (!hasTeiru && !IsIchidanConjugation(previousToken) && !IsBaseformEndInMuNuBuGu(previousToken))
                return false;

            if(IsDeIru(token, nextToken))
            {
                if (!hasTeiru)
                {
                    AppendConjungation("-te iru");      
                    hasTeiru = true;
                    return true;
                }
                else
                    return false;
            }

            if(hasTeiru && previousToken.Surface.Equals("で", StringComparison.OrdinalIgnoreCase))
            {
                if (token.Surface.Equals("た", StringComparison.OrdinalIgnoreCase))
                    AppendConjungation("past");
                else if (token.Surface.Equals("て", StringComparison.OrdinalIgnoreCase))
                    AppendConjungation("-te");

                return true;
            }

            return false;
        }

        private bool TryCommonAdjConjungation(IToken previousToken, IToken token)
        {
            string conjugation = null;
            if (IsGaruForm(previousToken))
            {
                if (IsIAdjectiveConjugation(previousToken.ConjugationType))
                {
                    if (token.Surface.Equals("さ"))
                        conjugation = "i-adj. as noun";
                    else if (token.Surface.Equals("そう"))
                        conjugation = "-sou";
                    else if (token.Surface.Equals("がる"))
                        conjugation = "-garu";
                }
            }
            else if (IsTeForm(previousToken))
            {
                if ((token.BaseForm.Equals("ない") || token.BaseForm.Equals("ぬ") || token.BaseForm.Equals("ん")))
                    conjugation = "negative";
                else if (token.Surface.Equals("て", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-te";
                else
                    return false;
            }
            else if (IsTaForm(previousToken))
            {
                if (token.Surface.Equals("た", StringComparison.OrdinalIgnoreCase))
                    conjugation = "past";
                else if (token.Surface.Equals("たり", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-tari";
                else if (token.Surface.Equals("たら", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-tara";
                else
                    return false;
            }
            else if (IsHypotheticalForm(previousToken))
            {
                if (token.Surface.Equals("ば", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-ba";
                else
                    return false;
            }
            else if(IsOuForm(previousToken))
            {
                if (token.Surface.Equals("う", StringComparison.OrdinalIgnoreCase))
                {                    
                    conjugation = "volitional";
                }
                else
                    return false;
            }

            if (conjugation != null)
            {
                AppendConjungation(conjugation);
                return true;
            }
            return false;
        }

        private bool TryVerbCommonConjungation(IToken previousToken, IToken token, IToken nextToken)
        {
            string conjugation = null;

            if (IsTeForm(previousToken))
            {
                if (IsTaiConjugation(previousToken) &&
                   (token.BaseForm.Equals("ない") || token.BaseForm.Equals("ぬ") || token.BaseForm.Equals("ん")))
                    conjugation = "negative";
                else if (token.Surface.Equals("て", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsDaOrDeConjugation(previousToken))
                        conjugation = "-te";
                    else
                        return false;
                }
                else if (token.Surface.Equals("で", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsDaOrDeConjugation(previousToken) || previousToken.BaseForm.EndsWith("ない"))
                        conjugation = "-te";
                    else
                        return false;
                }
                else
                    return false;
            }
            else if (IsTaForm(previousToken))
            {
                if (token.Surface.Equals("た", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsDaOrDeConjugation(previousToken))
                        conjugation = "past";
                    else
                        return false;
                }
                else if (token.Surface.Equals("だ", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsDaOrDeConjugation(previousToken))
                        conjugation = "past";
                    else
                        return false;
                }
                else if (token.BaseForm.Equals("てる", StringComparison.OrdinalIgnoreCase) || token.BaseForm.Equals("でる", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-te iru"; //ta form in classic also include te form
                else if (token.Surface.Equals("て", StringComparison.OrdinalIgnoreCase) || token.Surface.Equals("で", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-te"; //ta form in classic also include te form
                else if (token.Surface.Equals("たり", StringComparison.OrdinalIgnoreCase) || token.Surface.Equals("だり", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-tari";
                else
                    return false;
            }
            else if (IsOuForm(previousToken) || IsDeshouForm(previousToken, token))
            {
                if (token.Surface.Equals("う", StringComparison.OrdinalIgnoreCase))
                    conjugation = "volitional";
                else
                    return false;
            }
            else if (IsHypotheticalForm(previousToken))
            {
                if (token.Surface.Equals("ば", StringComparison.OrdinalIgnoreCase))
                    conjugation = "-ba";
                else
                    return false;
            }
            else if (IsImperfectiveForm(previousToken))
            {
                if (IsIchidanConjugation(previousToken) || IsSpecialKuVerb(previousToken))
                {
                    if (!ichidanConjungationMap.TryGetValue(token.BaseForm, out conjugation))
                        return false;
                }
                else if (!imperfectiveConjungationMap.TryGetValue(token.BaseForm, out conjugation))
                    return false;
            }
            else if (IsMasuStemForm(previousToken))
            {
                if (!masuStemConjungationMap.TryGetValue(token.BaseForm, out conjugation))
                {
                    if (IsVerbConjungateInMasuStemForm(previousToken))
                    {
                        if (!ichidanConjungationMap.TryGetValue(token.BaseForm, out conjugation))
                            return false;
                    }
                    else if (IsTaSouForm(token, nextToken))                    
                        conjugation = "-tai";                    
                    else
                        return false;
                }
            }
            else if(IsGaruForm(previousToken) && IsTaiConjugation(previousToken))
            {
                if (token.BaseForm.Equals("がる"))
                    conjugation = "-garu";
            }
            else if(IsSpecialSuVerb(previousToken))
            {
                if (token.BaseForm.Equals("れる", StringComparison.OrdinalIgnoreCase))
                    conjugation = "passive";
                else if (token.BaseForm.Equals("せる", StringComparison.OrdinalIgnoreCase))
                    conjugation = "causative";
                else if (token.Surface.Equals("ず", StringComparison.OrdinalIgnoreCase)
                        && previousToken.Surface.Equals("せ", StringComparison.OrdinalIgnoreCase))
                    conjugation = "negative";
            }

            if (conjugation != null)
            {
                if (!hasCausative && conjugation.Equals(CAUSATIVE, StringComparison.OrdinalIgnoreCase))
                    hasCausative = true;
                if (!hasImperative && conjugation.Equals(IMPERATIVE, StringComparison.OrdinalIgnoreCase))
                    hasImperative = true;
                if (conjugation.Equals("-te iru", StringComparison.OrdinalIgnoreCase))
                {
                    //If already have then it's actually the te form of iru
                    if (hasTeiru)
                        conjugation = "-te";                    
                }      
                AppendConjungation(conjugation);
                return true;
            }
            return false;
        }

        private bool TryMasendeConjungation(IToken previousToken, IToken token)
        {
            if (token.Surface.Equals("んで", StringComparison.OrdinalIgnoreCase))
            {
                if (previousToken.Surface.Equals("ませ", StringComparison.OrdinalIgnoreCase))
                {
                    AppendConjungation("negative");
                    AppendConjungation("-te");
                    return true;
                }
            }
            else if (previousToken.Surface.Equals("ん", StringComparison.OrdinalIgnoreCase))
            {
                                
                if (token.BaseForm.Equals("で", StringComparison.OrdinalIgnoreCase))
                {                    
                    AppendConjungation("-te");
                    return true;
                }
                else if (token.BaseForm.Equals("です"))
                {
                    //Not enought information to append now
                    //will append in next token
                    return true;
                }
            }                               
            
            return false;
        }

        private bool TryDewanaiCombine(IToken previousToken, IToken token, IToken nextToken)
        {
            if (nextToken == null)
                return false;

            if (conjugation.Length != 0)
                return false;

            if (previousToken.Surface.Equals("で", StringComparison.OrdinalIgnoreCase)
                && token.Surface.Equals("は", StringComparison.OrdinalIgnoreCase)
                &&  nextToken.BaseForm.Equals("ない", StringComparison.OrdinalIgnoreCase)
                )
            {
                BaseForm = "では";
                return true;
            }

            return false;
        }

        private bool TryDeAruCombine(IToken previousToken, IToken token)
        {
            if (conjugation.Length != 0)
                return false;

            if (previousToken.Surface.Equals("で", StringComparison.OrdinalIgnoreCase)
                && token.BaseForm.Equals("ある", StringComparison.OrdinalIgnoreCase))
            {
                BaseForm = "である";
                return true;
            }
            return false;
        }

        private void TryAppendRoImperative(string surface, string conjugationForm)
        {
            if (hasImperative)
                return;
            if (conjugationForm.Equals("命令ｒｏ", StringComparison.OrdinalIgnoreCase))
            {
                AppendConjungation(IMPERATIVE);
                hasImperative = true;                
            }            
        }

        private bool TryAppendImperative(string surface, string conjugationForm)
        {
            if (hasImperative)
                return false;

            if (conjugationForm.ContainsExtend("命令", StringComparison.OrdinalIgnoreCase)
                || surface.Equals("ください", StringComparison.OrdinalIgnoreCase))
            {
                AppendConjungation(IMPERATIVE);
                hasImperative = true;
                return true;
            }
            return false;
        }

        private bool IsBaseformEndInMuNuBuGu(IToken token)
        {
            if (token.BaseForm == null)
                return false;

            char word = token.BaseForm[token.BaseForm.Length - 1];
            if (word.Equals('む') 
                || word.Equals('ぶ') 
                || word.Equals('ぬ') 
                || word.Equals('ぐ'))
                return true;

            return false;
        }

        public bool IsDaOrDeConjugation(IToken token)
        {
            if (token.BaseForm.EndsWith("む")
                || token.BaseForm.EndsWith("ぶ")
                || token.BaseForm.EndsWith("ぐ")
                || token.BaseForm.EndsWith("ぬ"))
                return true;
            else
                return false;
        }

        private bool IsTokenInPossibleTaForm(IToken token)
        {
            if (IsVerbConjungateInMasuStemForm(token))
            {
                if (!IsMasuStemForm(token))
                    return false;
            }
            else
            {
                if (!IsTaForm(token))
                    return false;
            }
            return true;
        }

        private string GetString(StringBuilder builder)
        {
            if (builder == null)
                return "";
            else
                return builder.ToString();
        }

        private string GetConjunction()
        {
            return conjugation.ToString();
        }  

        private void AppendConjungation(string str)
        {
            conjugation.Append("[");
            conjugation.Append(str);
            conjugation.Append("] ");
        }

        public static string ToConjungationTag(string str)
        {
            return "[" + str + "]";            
        }

        public bool IsMaybeAmbiguousGodan()
        {
            if (!IsGodanConjugation())
                return false;
            
            var fistConjugation = conjugation.ToString().Split(StringHelper.SPACE_STRING_ARRAY, 2, StringSplitOptions.None)[0];
            if (fistConjugation.ContainsExtend("-te", StringComparison.OrdinalIgnoreCase)
                || fistConjugation.ContainsExtend("-tara", StringComparison.OrdinalIgnoreCase)
                || fistConjugation.ContainsExtend("past", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public bool IsGodanConjugation()
        {
            return FirstConjugationType != null && FirstConjugationType.ContainsExtend(JAP_GODAN, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsGodanConjugation(IToken token)
        {
            return token.ConjugationType != null && token.ConjugationType.ContainsExtend(JAP_GODAN, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsIchidanConjugation()
        {
            return FirstConjugationType != null && FirstConjugationType.ContainsExtend(JAP_ICHIDAN, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsIchidanConjugation(IToken token)
        {
            return token.ConjugationType != null && token.ConjugationType.ContainsExtend(JAP_ICHIDAN, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsBaseForm()
        {
            return FirstConjugationForm != null && FirstConjugationForm.Equals(JAP_BASE_FORM, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsKuruOrSuruConjugation()
        {
            return FirstConjugationType != null && 
                (FirstConjugationType.ContainsExtend("カ変", StringComparison.OrdinalIgnoreCase)
                || FirstConjugationType.ContainsExtend("サ変", StringComparison.OrdinalIgnoreCase));
        }

        public bool IsIAdjectiveConjugation()
        {
            return FirstConjugationType != null && FirstConjugationType.ContainsExtend(JAP_ADJECTIVE, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsIAdjectiveConjugation(string conjungationType)
        {
            return conjungationType != null && conjungationType.ContainsExtend(JAP_ADJECTIVE, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTaSouForm(IToken token, IToken nextToken)
        {
            return token.Surface.EqualsOrdinalIgnore("た") && nextToken != null && nextToken.Surface.EqualsOrdinalIgnore("そう");
        }

        public bool IsVerb()
        {
            return PartOfSpeech != null && PartOfSpeech.Equals(JAP_VERB, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsAuxiliaryVerb()
        {
            return PartOfSpeech != null && PartOfSpeech.Equals(JAP_AUXILIARY_VERB, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsParticle()
        {
            return PartOfSpeech != null && PartOfSpeech.Equals(JAP_PARTICLE, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSymbol()
        {
            return PartOfSpeech != null && PartOfSpeech.Equals(JAP_SYMBOL, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSymbol(IToken token)
        {
            return token.PartOfSpeech != null && token.PartOfSpeech.Equals(JAP_SYMBOL, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsAdjective()
        {
            return PartOfSpeech != null && PartOfSpeech.Equals(JAP_ADJECTIVE, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAdverb()
        {
            return PartOfSpeech != null && PartOfSpeech.Equals(JAP_ADVERB, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHave(string property)
        {
            return !String.IsNullOrWhiteSpace(property) && !property.Equals("*", StringComparison.OrdinalIgnoreCase);            
        }

        public static bool IsDeIru(IToken token, IToken nextToken)
        {
            return token.Surface.Equals("で", StringComparison.OrdinalIgnoreCase) && nextToken != null &&
                   (nextToken.Surface.Equals("た", StringComparison.OrdinalIgnoreCase) 
                   || nextToken.Surface.Equals("る", StringComparison.OrdinalIgnoreCase) 
                   || nextToken.Surface.Equals("て", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsMasuStemForm()
        {
            return FirstConjugationForm != null && FirstConjugationForm.Equals(JAP_MASU_STEM, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMasuStemForm(IToken token)
        {
            return token.ConjugationForm != null && token.ConjugationForm.Equals(JAP_MASU_STEM, StringComparison.OrdinalIgnoreCase);
        }        

        public bool IsMasuConjugation()
        {
            return Conjugation.Equals("[masu stem] ", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsInConjugationList(string word)
        {
            return word != null && conjungationList.Contains(word, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsTaForm(IToken token)
        {
            return token.ConjugationForm != null && 
                (token.ConjugationForm.Equals("連用タ接続", StringComparison.OrdinalIgnoreCase) || token.ConjugationForm.Equals("連用ダ接続", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsTeForm(IToken token)
        {
            return token.ConjugationForm != null &&
                (token.ConjugationForm.Equals("連用テ接続", StringComparison.OrdinalIgnoreCase) || token.ConjugationForm.Equals("連用デ接続", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsOuForm(IToken token)
        {
            return token.ConjugationForm != null && token.ConjugationForm.Equals(JAP_OU_FORM, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDeshouForm(IToken previousToken, IToken token)
        {
            return token.Surface.EqualsOrdinalIgnore("う") 
                && (previousToken.Surface.EqualsOrdinalIgnore("でしょ")
                    || previousToken.Surface.EqualsOrdinalIgnore("だろ"));
        }

        private static bool IsImperfectiveForm(IToken token)
        {
            return token.ConjugationForm != null && token.ConjugationForm.Equals(JAP_IMPERFECTIVE, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsHypotheticalForm(IToken token)
        {
            return token.ConjugationForm != null && token.ConjugationForm.Equals(JAP_HYPOTHETICAL, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTaiConjugation(IToken token)
        {
            return token.ConjugationType != null && token.ConjugationType.Equals(JAP_TAI_FROM, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsGodanSu(IToken token)
        {
            return token.ConjugationType != null && token.ConjugationType.Equals("五段・サ行", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsGaruForm(IToken token)
        {
            return token.ConjugationForm != null && token.ConjugationForm.Equals("ガル接続", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsVerbConjungateInMasuStemForm(IToken token)
        {
            return IsIchidanConjugation(token)
                            || token.BaseForm.Equals("ます")
                            || token.BaseForm.Equals("です")
                            || IsGodanSu(token)
                            || IsSpecialSuVerb(token)
                            || IsSpecialKuVerb(token);
        }

        private static bool IsSpecialSuVerb(IToken token)
        {
            return token.ConjugationType != null && token.ConjugationType.Equals(JAP_SURU_CONJ, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSpecialKuVerb(IToken token)
        {
            return token.ConjugationType != null &&
                (token.ConjugationType.Equals(JAP_KURU_CONJ, StringComparison.OrdinalIgnoreCase)
                || token.ConjugationType.Equals(JAP_KURU_KANJI_CONJ, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsSpecialSuVerb(string conjugationType)
        {
            return conjugationType != null && conjugationType.Equals(JAP_SURU_CONJ, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSpecialKuVerb(string conjugationType)
        {
            return conjugationType != null &&
                (conjugationType.Equals(JAP_KURU_CONJ, StringComparison.OrdinalIgnoreCase)
                || conjugationType.Equals(JAP_KURU_KANJI_CONJ, StringComparison.OrdinalIgnoreCase));
        }

        public string GetPartOfSpeechInEnglish()
        {
            if (String.IsNullOrWhiteSpace(PartOfSpeech))
                return null;
            else if (PartOfSpeech.Equals(JAP_VERB, StringComparison.OrdinalIgnoreCase))
                return "verb";
            else if (PartOfSpeech.Equals(JAP_ADJECTIVE, StringComparison.OrdinalIgnoreCase))
                return "adjective";
            else if (PartOfSpeech.Equals(JAP_PARTICLE, StringComparison.OrdinalIgnoreCase))
                return "particle";
            if (PartOfSpeech.Equals(JAP_PRONOUN, StringComparison.OrdinalIgnoreCase))
                return "pronoun";            
            else if (PartOfSpeech.Equals(JAP_AUXILIARY_VERB, StringComparison.OrdinalIgnoreCase))
                return "auxiliary verb";            
            else if (PartOfSpeech.Equals(JAP_ADVERB, StringComparison.OrdinalIgnoreCase))
                return "adverb";
            else if (PartOfSpeech.Equals(JAP_PRENOUN_ADJECTIVE, StringComparison.OrdinalIgnoreCase))
                return "pre-noun adjectival";
            else if (PartOfSpeech.Equals(JAP_CONJUGATION, StringComparison.OrdinalIgnoreCase))
                return "conjunction";
            else
                return null;
        }

        private void SetAllChange()
        {
            isReadingChange = true;
            isPronunChange = true;
            isSurfaceChange = true;
        }
    }
}
