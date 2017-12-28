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

namespace NLPJDict.Kuromoji.Core.Buffer
{
    public class BufferEntry
    {
        public List<short> TokenInfo { get; set; } = new List<short>();
        public List<int> Features { get; set; } = new List<int>();
        public List<byte> PosInfo { get; set; } = new List<byte>();

        public short[] TokenInfos { get; set; } // left id, right id, word cost values
        public int[] FeatureInfos { get; set; } // references to string features
        public byte[] PosInfos { get; set; } // part-of-speech tag values

    }
}
