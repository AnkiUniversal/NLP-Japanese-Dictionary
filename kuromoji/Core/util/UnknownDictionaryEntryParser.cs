/**
 * Copyright © 2010-2017 Atilika Inc. and contributors (see CONTRIBUTORS.md)
 * 
 * Modifications copyright (C) 2017 - 2018 Anki Universal Team <ankiuniversal@gmail.com>
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

using NLPJapaneseDictionary.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.util
{
    public class UnknownDictionaryEntryParser : DictionaryEntryLineParser
    {

        // NOTE: Currently this code is the same as the IPADIC dictionary entry parser,
        // which is okay for all the dictionaries supported so far...
        public GenericDictionaryEntry parse(String entry)
        {
            string[] fields = ParseLine(entry);

            string surface = fields[0];
            short leftId = short.Parse(fields[1]);
            short rightId = short.Parse(fields[2]);
            short wordCost = short.Parse(fields[3]);

            string[] pos = new string[6];
            Array.Copy(fields, 4, pos, 0, pos.Length);

            string[] features = new string[fields.Length - 10];
            Array.Copy(fields, 10, features, 0, features.Length);

            GenericDictionaryEntry.Builder builder = new GenericDictionaryEntry.Builder()
            {
            Surface = surface,
            LeftId = leftId,
            RightId = rightId,
            WordCost = wordCost,
            PartOfSpeechFeatures = pos,
            OtherFeatures = features
            };

            GenericDictionaryEntry dictionaryEntry = new GenericDictionaryEntry(builder);
            return dictionaryEntry;
        }
    }
}
