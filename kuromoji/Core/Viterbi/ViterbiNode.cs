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

using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class ViterbiNode
    {

        public enum NodeType
        {
            KNOWN,
            UNKNOWN,
            USER,
            INSERTED
        }

        private readonly int wordId;
        public int WordId { get { return wordId; } }

        private readonly string surface;
        public string Surface { get { return surface; } }

        private readonly int leftId;
        public int LeftId { get { return leftId; } }

        private readonly int rightId;
        public int RightId { get { return rightId; } }

        /**
         * word cost for this node
         */
        private readonly int wordCost;
        public int WordCost { get { return wordCost; } }

        /**
         * minimum path cost found thus far
         */
        public int PathCost { get; set; }
        public ViterbiNode LeftNode { get; set; }

        private readonly NodeType type;
        public NodeType Type { get { return type; } }

        private readonly int startIndex;
        public int StartIndex { get { return startIndex; } }

        public ViterbiNode(int wordId, String surface, int leftId, int rightId, int wordCost, int startIndex, NodeType type)
        {
            this.wordId = wordId;
            this.surface = surface;
            this.leftId = leftId;
            this.rightId = rightId;
            this.wordCost = wordCost;
            this.startIndex = startIndex;
            this.type = type;
        }

        public ViterbiNode(int wordId, string word, IDictionary dictionary, int startIndex, NodeType type)
        : this(wordId, word, dictionary.GetLeftId(wordId), dictionary.GetRightId(wordId), dictionary.GetWordCost(wordId), startIndex, type)
        {
        }

    }
}
