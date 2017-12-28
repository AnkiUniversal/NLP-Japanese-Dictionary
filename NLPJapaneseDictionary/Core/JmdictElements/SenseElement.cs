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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.JmdictElements
{
    public class SenseElement
    {
        public int Order { get; set; }
        public string[] OnlyForKanji { get; set; }
        public string[] OnlyForRead { get; set; }
        public string CrossReference { get; set; }
        public string Antonym { get; set; }
        public string PartOfSpeech { get; set; }
        public string Field { get; set; }
        public string Misc { get; set; }
        public string Dialect { get; set; }
        public string Gloss { get; set; }

        public SenseElement(JObject senseElement, int order)
        {            
            Order = order;
            JToken value;

            if (senseElement.TryGetValue("stagK", out value))
            {
                var stagK = value.ToString();
                OnlyForKanji = stagK.Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
            }

            if (senseElement.TryGetValue("stagR", out value))
            {
                var stagr = value.ToString();
                OnlyForRead = stagr.Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
            }

            if (senseElement.TryGetValue("xref", out value))
                CrossReference = value.ToString();

            if (senseElement.TryGetValue("ant", out value))
                Antonym = value.ToString();

            if (senseElement.TryGetValue("field", out value))
                Field = value.ToString();

            if (senseElement.TryGetValue("misc", out value))
                Misc = value.ToString();

            if (senseElement.TryGetValue("dialect", out value))
                Dialect = value.ToString();

            if (senseElement.TryGetValue("gloss", out value))
                Gloss = value.ToString();

            if (senseElement.TryGetValue("pos", out value))
                PartOfSpeech = value.ToString();
        }
    }
}