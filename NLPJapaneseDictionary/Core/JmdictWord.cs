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

using Newtonsoft.Json.Linq;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.NLPJDictCore.Interfaces;
using NLPJDict.NLPJDictCore.JmdictElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore
{
    public class JmdictWord
    {
        public const string NO_PRIORITY = "unranked";

        public int EntrySequence { get; set; }
        public int HighestFrequency { get; set; }
        public bool IsHaveExample { get; set; }
        public string Conjungation { get; set; }

        public IRepresentWord FirstWord { get; set; }

        public string OtherKanjiForms { get; set; }
        public string ReadingApplyToAll { get; set; }
        public string RestrictReading { get; set; }
        public string[] KanjiLetters { get; set; }

        public List<SenseElement> SenseElements { get; set; } = new List<SenseElement>();

        public JmdictWord(JmdictEntity entry)
        {            
            EntrySequence = entry.EntrySequence;
            HighestFrequency = entry.HighestFrequency;
            IsHaveExample = entry.IsHaveExample;
            KanjiLetters = entry.KanjiLetters.Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
            Conjungation = entry.Conjugation;                        

            var readElements = ParseReadElement(entry);    
            var kanjiElements = ParseKanjiElement(entry);
            if (kanjiElements != null && kanjiElements.Count > 0)
            {
                FirstWord = kanjiElements[0];                
                kanjiElements.RemoveAt(0);
                GetOtherKanjiFormIfNeeded(kanjiElements);
            }
            else
            {
                FirstWord = readElements[0];
                readElements.RemoveAt(0);
            }

            FirstWord.Word = entry.RepresentWord;
            FirstWord.Priority = JmdictEntity.GetPriorityInString(HighestFrequency);

            GetReading(readElements);

            SenseElements = ParseSenseElement(entry);
        }

        private void GetOtherKanjiFormIfNeeded(List<KanjiElement> kanjiElements)
        {
            if (kanjiElements.Count > 0)
            {
                StringBuilder otherForms = new StringBuilder();
                for(int i = 0; i < kanjiElements.Count - 1; i++)
                {
                    AppendKanjiElement(kanjiElements, otherForms, i);
                    otherForms.Append("\n");
                }
                AppendKanjiElement(kanjiElements, otherForms, kanjiElements.Count - 1);
                OtherKanjiForms = otherForms.ToString();
            }            
        }

        private static void AppendKanjiElement(List<KanjiElement> kanjiElements, StringBuilder otherForms, int i)
        {
            otherForms.Append(kanjiElements[i].Word);

            if (kanjiElements[i].Information != null)
            {
                otherForms.Append(" (");
                otherForms.Append(kanjiElements[i].Information);
                otherForms.Append(")");
            }
        }

        private void GetReading(List<ReadElement> readElements)
        {
            StringBuilder readingApplyToAll = new StringBuilder();
            StringBuilder restrictReading = new StringBuilder();
            for (int i = 0; i < readElements.Count; i++)
            {
                if(readElements[i].Retricts != null && readElements[i].Retricts.Length > 0)
                {
                    GetRestrictReading(readElements[i], restrictReading);
                }
                else
                {
                    GetReadingApplyToAll(readElements, readingApplyToAll, i);
                }
            }
            ReadingApplyToAll = readingApplyToAll.ToString().Trim();

            if (restrictReading.Length > 0)
                RestrictReading = restrictReading.ToString().Trim();
        }

        private void GetRestrictReading(ReadElement readElement, StringBuilder restrictReading)
        {
            restrictReading.Append(readElement.Word);
            restrictReading.Append(" (");
            int j = 0;
            while (true)
            {
                restrictReading.Append(readElement.Retricts[j]);
                j++;
                if (j < readElement.Retricts.Length)
                    restrictReading.Append("; ");
                else
                    break;
            }
            restrictReading.Append(")");

           AppendReadInformationIfHas(readElement, restrictReading);
           restrictReading.Append("\n");
        }

        private static void GetReadingApplyToAll(List<ReadElement> readElements, StringBuilder readingApplyToAll, int i)
        {
            readingApplyToAll.Append(readElements[i].Word);            
            AppendReadInformationIfHas(readElements[i], readingApplyToAll);
            readingApplyToAll.Append("\n");
        }

        private static void AppendReadInformationIfHas(ReadElement readElement, StringBuilder reading)
        {
            if (readElement.Information != null && !String.IsNullOrWhiteSpace(readElement.Information))
            {
                reading.Append(" (");
                reading.Append(readElement.Information);
                reading.Append(")");
            }
        }

        private static List<KanjiElement> ParseKanjiElement(JmdictEntity entry)
        {            
            if (entry.KanjiElement == null)
                return null;
            JArray kanjiArray = JArray.Parse(entry.KanjiElement);

            if (kanjiArray == null)
                return null;

            List<KanjiElement> kanjiElements = new List<KanjiElement>();
            foreach (var kanji in kanjiArray)
                kanjiElements.Add(new KanjiElement((JObject)kanji));
            return kanjiElements;
        }

        public static List<ReadElement> ParseReadElement(JmdictEntity entry)
        {
            JArray readArray = JArray.Parse(entry.ReadElement);
            List<ReadElement> readElements = new List<ReadElement>();
            foreach (var read in readArray)
                readElements.Add(new ReadElement((JObject)read));
            return readElements;
        }

        private static List<SenseElement> ParseSenseElement(JmdictEntity entry)
        {
            List<SenseElement> senseElements = new List<SenseElement>();
            var senseArray = JArray.Parse(entry.SenseElement);
            int order = 1;
            foreach (var sense in senseArray)
            {
                var senseElement = new SenseElement((JObject)sense, order);
                senseElements.Add(senseElement);
                order++;
            }
            return senseElements;
        }
    }
}
