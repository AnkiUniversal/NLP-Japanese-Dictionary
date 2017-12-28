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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using SQLite;
using NLPJDict.Kuromoji.Core.util;
using NLPJDict.NLPJDictCore;

namespace NLPJDict.DatabaseTable.NLPJDictCore
{
    public class JmdictEntity
    {
        private const int KURU_VERB_ENTRY = 1547720;
        private const int SURU_VERB_ENTRY = 1157170;

        private static Regex kanjiRegex = new Regex(@"[\p{IsCJKUnifiedIdeographs}]", RegexOptions.Compiled);
        public static readonly EntrySequenceComparer EqualComparer = new EntrySequenceComparer();
        
        private const string TABLE_COLUMN_QUERY = "SELECT EntrySequence, HighestFrequency, PartOfSpeech, IsHaveExample, RepresentWord, KanjiElement, ReadElement, SenseElement, KanjiLetters FROM JmdictEntity WHERE JmdictEntity MATCH ? ORDER BY HighestFrequency ASC LIMIT 200";
        private const string TABLE_HAS_QUERY = "SELECT EntrySequence FROM JmdictEntity WHERE JmdictEntity MATCH ? LIMIT 1";        

        public const string POS_VERB = " AND PartOfSpeech:verb";
        public const string POS_VERB_SURU = " AND PartOfSpeech:verb AND PartOfSpeech:suru";
        public const string POS_VERB_EXCEPT_SURU_KURU = " AND PartOfSpeech:verb NOT PartOfSpeech:suru NOT EntrySequence:1547720";
        public const string POS_VERB_GODAN_EXCEPT_SURU_KURU = " AND PartOfSpeech:Godan NOT PartOfSpeech:suru NOT EntrySequence:1547720";
        public const string POS_VERB_ICHIDAN_EXCEPT_SURU_KURU = " AND PartOfSpeech:Ichidan NOT PartOfSpeech:suru NOT EntrySequence:1547720";
        public const string POS_VERB_EXCEPT_SURU = " AND PartOfSpeech:verb NOT PartOfSpeech:suru";
        public const string POS_IADJ = " AND PartOfSpeech:adjective NOT PartOfSpeech:noun NOT PartOfSpeech:suru NOT PartOfSpeech:no NOT PartOfSpeech:na NOT PartOfSpeech:taru NOT PartOfSpeech:shiku NOT PartOfSpeech:ku";
        public const string POS_ICHIDAN = " AND PartOfSpeech:Ichidan";
        public const string POS_GODAN = " AND PartOfSpeech:Godan";
        public const string POS_ADJ = " AND PartOfSpeech:adjective";
        public const string POS_NOUN = " AND PartOfSpeech:noun";
        public const string POS_ADV = " AND PartOfSpeech:adverb";        

        public const int NO_RANKED = 99;
        public const int LOWEST_COMMON_RANKED = 10;
        public const string SEPARATOR = "; ";

        public static readonly string[] SEPARATOR_ARRAY = new string[] { SEPARATOR };
        public static readonly string[] WHITESPACE_ARRAY = new string[] { " " };

        [SQLite.Ignore]
        public string Conjugation { get; set; }

        [PrimaryKey]
        public int EntrySequence { get; set; }
        public int HighestFrequency { get; set; }
        public bool IsHaveExample { get; set; }
        public string RepresentWord { get; set; }
        public string KanjiElement { get; set; }
        public string ReadElement { get; set; }
        public string SenseElement { get; set; }
        public string FirstGloss { get; set; }
        public string Gloss { get; set; }
        public string KanjiLetters { get; set; }
        public string PartOfSpeech { get; set; }        

        public static bool HasJapWord(string word, Database dict)
        {
            var results = dict.QueryColumn<JmdictEntity>("SELECT EntrySequence FROM JmdictEntity WHERE RepresentWord MATCH ? LIMIT 1", word);
            if (results.Count > 0)
                return true;

            string searchQuery;
            if (IsHasKanji(word))
                searchQuery = "SELECT EntrySequence FROM JmdictEntity WHERE KanjiElement MATCH ? LIMIT 1";
            else
                searchQuery = "SELECT EntrySequence FROM JmdictEntity WHERE ReadElement MATCH ? LIMIT 1";

            results = dict.QueryColumn<JmdictEntity>(searchQuery, word);
            if (results.Count > 0)
                return true;
            else
                return false;
        }

        public static bool HasJapPOS(string word, Database dict, string pos)
        {
            var query = "RepresentWord:" + word + pos;
            var results = dict.QueryColumn<JmdictEntity>(TABLE_HAS_QUERY, query);
            if (results.Count > 0)
                return true;

            if (IsHasKanji(word))
                query = "KanjiElement:" + word + pos;
            else
                query = "ReadElement:" + word + pos;

            results = dict.QueryColumn<JmdictEntity>(TABLE_HAS_QUERY, query);
            if (results.Count > 0)
                return true;
            else
                return false;
        }

        public static JmdictEntity GetSpecialKuruVerb(Database dict)
        {
            return dict.QueryColumn<JmdictEntity>("SELECT * FROM JmdictEntity WHERE EntrySequence MATCH 1547720 LIMIT 1")[0];
        }

        public static JmdictEntity GetSpecialSuruVerb(Database dict)
        {
            return dict.QueryColumn<JmdictEntity>("SELECT * FROM JmdictEntity WHERE EntrySequence MATCH 1157170 LIMIT 1")[0];
        }

        public static List<JmdictEntity> GetJapMatchAll(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, "");
        }

        public static List<JmdictEntity> GetJapMatchNoun(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_NOUN);
        }

        public static List<JmdictEntity> GetJapMatchAdverb(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_ADV);
        }

        public static List<JmdictEntity> GetJapMatchAdjective(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_ADJ);
        }

        public static List<JmdictEntity> GetJapMatchIAdjective(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_IADJ);
        }

        public static List<JmdictEntity> GetJapMatchIChidan(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_ICHIDAN);
        }

        public static List<JmdictEntity> GetJapMatchVerb(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_VERB);
        }

        public static List<JmdictEntity> GetJapMatchVerbExceptSuruVerb(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_VERB_EXCEPT_SURU);
        }

        public static List<JmdictEntity> GetJapMatchGodanVerb(string word, Database dict)
        {
            return GetJapMatchWordAndPOS(word, dict, POS_GODAN);
        }

        public static List<JmdictEntity> GetJapMatchWordAndPOS(string word, Database dict, string pos)
        {
            var query = "RepresentWord:" + word + pos;
            var preferedMatches = dict.QueryColumn<JmdictEntity>(TABLE_COLUMN_QUERY, query);
            var preferedMatchesCount = preferedMatches.Count;

            if (IsHasKanji(word))
                query = "KanjiElement:" + word + pos;
            else
                query = "ReadElement:" + word + pos;
            
            var secondaryMatches = dict.QueryColumn<JmdictEntity>(TABLE_COLUMN_QUERY, query);
            if (secondaryMatches.Count > 0)
            {
                ReoderPreferedMatchesWithFrequencyIfNeeded(preferedMatches, secondaryMatches);
            }

            return preferedMatches;
        }

        public static List<JmdictEntity> GetJapMatchListWordAndPOS(List<string> words, string pos, Database dict)
        {
            var query = CreateRepresentWordOrQuery(words, pos);
            var preferedMatches = dict.QueryColumn<JmdictEntity>(TABLE_COLUMN_QUERY, query);
            var preferedMatchesCount = preferedMatches.Count;

            query = CreateKanjiOrReadElementWordOrQuery(words, pos);            

            var secondaryMatches = dict.QueryColumn<JmdictEntity>(TABLE_COLUMN_QUERY, query);
            if (secondaryMatches.Count > 0)
            {
                ReoderPreferedMatchesWithFrequencyIfNeeded(preferedMatches, secondaryMatches);
            }

            return preferedMatches;
        }

        private static string CreateRepresentWordOrQuery(List<string> words, string pos)
        {
            string query;
            StringBuilder builder = new StringBuilder("(");
            for (int i = 0; i < words.Count - 1; i++)
            {
                builder.Append("RepresentWord:");
                builder.Append(words[i]);
                builder.Append(" OR ");
            }
            builder.Append("RepresentWord:");
            builder.Append(words[words.Count - 1]);
            builder.Append(")");
            builder.Append(pos);
            query = builder.ToString();
            return query;
        }

        private static string CreateKanjiOrReadElementWordOrQuery(List<string> words, string pos)
        {
            string query;
            StringBuilder builder = new StringBuilder("(");
            for (int i = 0; i < words.Count - 1; i++)
            {
                AppendColumnQuery(builder, words[i]);
                builder.Append(" OR ");
            }
            AppendColumnQuery(builder, words[words.Count - 1]);
            builder.Append(")");
            builder.Append(pos);
            query = builder.ToString();
            return query;
        }

        private static void AppendColumnQuery(StringBuilder builder, string word)
        {
            if (IsHasKanji(word))
                builder.Append("KanjiElement:");
            else
                builder.Append("ReadElement:");
            builder.Append(word);
        }

        private static void ReoderPreferedMatchesWithFrequencyIfNeeded(List<JmdictEntity> preferedMatches, List<JmdictEntity> secondaryMatches)
        {
            if (secondaryMatches[0].HighestFrequency > LOWEST_COMMON_RANKED)
            {
                preferedMatches.AddRange(secondaryMatches);
                return;
            }

            List<JmdictEntity> preferUnRankedRemoved = new List<JmdictEntity>();
            for (int i = 0; i < preferedMatches.Count; i++)
            {
                if (preferedMatches[i].HighestFrequency > LOWEST_COMMON_RANKED)
                {                    
                    preferUnRankedRemoved.Add(preferedMatches[i]);
                    preferedMatches.RemoveAt(i);
                }                
            }
            bool isAdded = false;
            for(int i = 0; i < secondaryMatches.Count; i++)
            {
                if (secondaryMatches[i].HighestFrequency > LOWEST_COMMON_RANKED)
                {
                    isAdded = true;
                    secondaryMatches.InsertRange(i, preferUnRankedRemoved);
                    break;
                }
            }
            if(!isAdded)
                secondaryMatches.AddRange(preferUnRankedRemoved);
            preferedMatches.AddRange(secondaryMatches);
        }

        public static List<JmdictEntity> GetByGlossAll(string word, Database dict)
        {
            return GetByGlossMatchWordsAndPOS(word, dict, "");
        }

        public static List<JmdictEntity> GetByGlossVerb(string word, Database dict)
        {
            return GetByGlossMatchWordsAndPOS(word, dict, POS_VERB);
        }

        public static List<JmdictEntity> GetByGlossAdjective(string word, Database dict)
        {
            return GetByGlossMatchWordsAndPOS(word, dict, POS_ADJ);
        }

        public static List<JmdictEntity> GetByGlossAdverb(string word, Database dict)
        {
            return GetByGlossMatchWordsAndPOS(word, dict, POS_ADV);
        }

        public static List<JmdictEntity> GetByGlossNoun(string word, Database dict)
        {
            return GetByGlossMatchWordsAndPOS(word, dict, POS_NOUN);
        }

        public static List<JmdictEntity> GetByGlossMatchWordsAndPOS(string word, Database dict, string pos)
        {            
            string query = CreateWordsQuery(word, pos, "FirstGloss:");
            var preferedMatches = dict.QueryColumn<JmdictEntity>(TABLE_COLUMN_QUERY, query);
            int preferedMatchesCount = preferedMatches.Count;
            bool isSecondaryHasCommonRankedWord = false;

            query = CreateWordsQuery(word, pos, "Gloss:");            
            var secondaryMatches = dict.QueryColumn<JmdictEntity>(TABLE_COLUMN_QUERY, query);
            if (secondaryMatches.Count > 0)
            {
                isSecondaryHasCommonRankedWord = secondaryMatches[0].HighestFrequency <= LOWEST_COMMON_RANKED;
                preferedMatches = preferedMatches.Union(secondaryMatches, EqualComparer).ToList();
            }
            if (isSecondaryHasCommonRankedWord)
                ReOrderPreferedMatchesWithFrequency(preferedMatches, preferedMatchesCount);
            return preferedMatches;
        }      

        private static string CreateWordsQuery(string word, string pos, string columnQuery)
        {
            var words = word.Split(WHITESPACE_ARRAY, StringSplitOptions.RemoveEmptyEntries);
            string query;
            if (words.Length > 1)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < words.Length - 1; i++)
                {
                    builder.Append(columnQuery);
                    builder.Append(words[i]);
                    builder.Append(" NEAR/0 ");
                }
                builder.Append(columnQuery);
                builder.Append(words[words.Length - 1]);
                builder.Append(pos);
                query = builder.ToString();
            }
            else
            {
                query = columnQuery + word + pos;
            }

            return query;
        }

        private static void ReOrderPreferedMatchesWithFrequency(List<JmdictEntity> preferedMatches, int preferedMatchesCount)
        {            
            List<JmdictEntity> preferUnRankedRemoved = new List<JmdictEntity>();
            int index;
            int progress = 0;
            for (index = 0; index < preferedMatches.Count;)
            {
                if (preferedMatches[index].HighestFrequency > LOWEST_COMMON_RANKED)
                {
                    if (progress > preferedMatchesCount)
                        break;
                    preferUnRankedRemoved.Add(preferedMatches[index]);
                    preferedMatches.RemoveAt(index);
                }
                else
                    index++;

                progress++;
            }

            preferedMatches.InsertRange(index, preferUnRankedRemoved);
        }

        private static bool IsHasKanji(string word)
        {
            return kanjiRegex.IsMatch(word);
        }

        private static string MergeEntriesForDatabaseAccess(List<JmdictEntity> entries)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            for (int i = 0; i < entries.Count - 1; i++)
            {
                builder.Append(entries[i].EntrySequence);
                builder.Append(',');
            }
            builder.Append(entries[entries.Count - 1].EntrySequence);
            builder.Append(')');
            return builder.ToString();
        }

        public static int EntryFrequencySort(JmdictEntity compared, JmdictEntity comparer)
        {
            return compared.HighestFrequency - comparer.HighestFrequency;
        }

        public static string GetPriorityInString(int frequency)
        {
            if (frequency <= 2)
                return "very high";
            else if (2 < frequency && frequency <= 4)
                return "high";
            else if (4 < frequency && frequency <= 10)
                return "common";
            else if (10 < frequency && frequency <= 20)
                return "quite low";
            else if (20 < frequency && frequency <= 40)
                return "low";
            else
                return "unranked";
        }
    }

    public class EntrySequenceComparer : IEqualityComparer<JmdictEntity>
    {
        public bool Equals(JmdictEntity x, JmdictEntity y)
        {
            return x.EntrySequence == y.EntrySequence;
        }

        public int GetHashCode(JmdictEntity obj)
        {
            return obj.EntrySequence;
        }
    }
}
