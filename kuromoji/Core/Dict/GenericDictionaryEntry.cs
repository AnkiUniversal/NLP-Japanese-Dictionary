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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public class GenericDictionaryEntry : DictionaryEntryBase
    {

        private readonly string[] partOfSpeechFeatures;
        private readonly string[] otherFeatures;

        public GenericDictionaryEntry(Builder builder) : base(builder.Surface, builder.LeftId, builder.RightId, builder.WordCost)
        {
            partOfSpeechFeatures = builder.PartOfSpeechFeatures.ToArray(); ;
            otherFeatures = builder.OtherFeatures.ToArray();
        }

        public string[] GetPartOfSpeechFeatures()
        {
            return partOfSpeechFeatures;
        }

        public string[] GetOtherFeatures()
        {
            return otherFeatures;
        }

        public class Builder
        {
            public string Surface { get; set; }
            public short LeftId { get; set; }
            public short RightId { get; set; }
            public short WordCost { get; set; }
            public string[] PartOfSpeechFeatures { get; set; }
            public string[] OtherFeatures { get; set; }
        }
    }
}
