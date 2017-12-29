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
using NLPJDict.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Core.JmdictElements
{
    public class ReadElement : IRepresentWord
    {
        public string Word { get; set; }
        public string Priority { get; set; }
        public string[] Retricts { get; set; }
        public string Information { get; set; }

        public ReadElement(JObject readElement)
        {
            JToken value;

            if(readElement.TryGetValue("reb", out value))            
                Word = value.ToString();
            
            if (readElement.TryGetValue("re_inf", out value))            
                Information = value.ToString();
            
            if (readElement.TryGetValue("re_pri", out value))            
                Priority = value.ToString();

            if (readElement.TryGetValue("re_restr", out value))
                Retricts = value.ToString().Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
