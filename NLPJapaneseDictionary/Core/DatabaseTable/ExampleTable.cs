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

using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace NLPJapaneseDictionary.DatabaseTable.NLPJDictCore
{
    public class ExampleTable
    {
        private static readonly Regex readingRegex = new Regex(@"(\([^()]*\))", RegexOptions.Compiled);
        public static Regex ReadingRegex { get { return readingRegex; } }

        private static readonly Regex senseRegex = new Regex(@"(\[[^\[\]]*\])", RegexOptions.Compiled);
        public static Regex SenseRegex { get { return senseRegex; } }

        private static readonly Regex formRegex = new Regex(@"(\{[^\{\}]*\})", RegexOptions.Compiled);
        public static Regex FormRegex { get { return formRegex; } }

        [PrimaryKey]
        public int Id { get; set; }
        public string JapaneseSentence { get; set; }
        public string EnglishSentence { get; set; }
        public string TokenizedSentence { get; set; }

        public static List<ExampleTable> GetExample(string word, int limit, Database dict)
        {
            return dict.QueryColumn<ExampleTable>("SELECT JapaneseSentence, EnglishSentence FROM ExampleTable WHERE TokenizedSentence MATCH ? LIMIT ? ", word, limit);
        }

        public static List<ExampleTable> FindExampleIds(string word, Database dict)
        {
            return dict.QueryColumn<ExampleTable>("SELECT Id FROM ExampleTable WHERE TokenizedSentence MATCH ?", word);
        }

        public static List<ExampleTable> GetExamplesFormEntries(List<ExampleTable> entries, int startIndex, int length, Database dict)
        {
            string idList = ToIdList(entries, startIndex, length);
            return dict.QueryColumn<ExampleTable>("SELECT JapaneseSentence, EnglishSentence FROM ExampleTable WHERE Id in " + idList);
        }
        
        private static string ToIdList(List<ExampleTable> entries, int startIndex, int getLength)
        {
            int length = entries.Count > (startIndex + getLength) ? getLength : (entries.Count - startIndex);
            StringBuilder builder = new StringBuilder();
            builder.Append("(");
            for (int i = startIndex; i < length - 1; i++)
            {
                builder.Append(entries[i].Id);
                builder.Append(",");
            }
            builder.Append(entries[length - 1].Id);
            builder.Append(")");
            return builder.ToString();
        }
    }
}
