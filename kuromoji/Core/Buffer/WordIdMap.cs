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

using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Buffer
{
    public class WordIdMap
    {

        private readonly int[] indices;

        private readonly int[] wordIds;

        private readonly int[] empty = new int[] { };

        public WordIdMap(Stream input)
        {
            try
            {
                lock (input)
                {
                    BinaryReader reader = new BinaryReader(input);
                    int[][] arrays = IntegerArrayIO.ReadArrays(reader, 2);
                    indices = arrays[0];
                    wordIds = arrays[1];
                }
            }
            catch (Exception ex)
            {
                throw new IOException("WordIdMap Contructor: " + ex.Message);
            }
        }

        public int[] LookUp(int sourceId)
        {
            int index = indices[sourceId];

            if (index == -1)
            {
                return empty;
            }
            int[] subArray = new int[wordIds[index]];
            Array.Copy(wordIds, index + 1, subArray, 0, wordIds[index]);
            return subArray;
        }
    }
}
