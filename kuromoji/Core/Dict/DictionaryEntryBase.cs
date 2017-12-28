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
    public abstract class DictionaryEntryBase
    {
        protected readonly string surface;

        protected readonly short leftId;

        protected readonly short rightId;

        protected readonly short wordCost;

        public DictionaryEntryBase(string surface, short leftId, short rightId, int wordCost)
        {
            this.surface = surface;
            this.leftId = leftId;
            this.rightId = rightId;

            // TODO: Temporary work-around for UniDic NEologd to deal with costs outside the short value range
            if (wordCost < Int16.MinValue)
            {
                this.wordCost = Int16.MinValue;
            }
            else if (wordCost > Int16.MaxValue)
            {
                this.wordCost = Int16.MaxValue;
            }
            else
            {
                this.wordCost = (short)wordCost;
            }
        }

        public string GetSurface()
        {
            return surface;
        }

        public short GetLeftId()
        {
            return leftId;
        }

        public short GetRightId()
        {
            return rightId;
        }

        public short GetWordCost()
        {
            return wordCost;
        }
    }
}
