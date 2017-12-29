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

namespace NLPJapaneseDictionary.Kuromoji.Core.Viterbi
{
    public class ViterbiLattice
    {

        static readonly string BOS = "BOS";
        static readonly string EOS = "EOS";

        private readonly int dimension;
        private readonly ViterbiNode[][] startIndexArr;
        private readonly ViterbiNode[][] endIndexArr;
        private readonly int[] startSizeArr;
        private readonly int[] endSizeArr;

        public ViterbiNode[][] StartIndexArr { get { return startIndexArr; } }
        public ViterbiNode[][] EndIndexArr { get { return endIndexArr; } }
        public int[] StartSizeArr { get { return startSizeArr; } }
        public int[] EndSizeArr { get { return endSizeArr; } }

        public ViterbiLattice(int dimension)
        {
            this.dimension = dimension;
            startIndexArr = new ViterbiNode[dimension][];
            endIndexArr = new ViterbiNode[dimension][];
            startSizeArr = new int[dimension];
            endSizeArr = new int[dimension];
        }

        public void AddBos()
        {
            ViterbiNode bosNode = new ViterbiNode(-1, BOS, 0, 0, 0, -1, ViterbiNode.NodeType.KNOWN);
            AddNode(bosNode, 0, 1);
        }

        public void AddEos()
        {
            ViterbiNode eosNode = new ViterbiNode(-1, EOS, 0, 0, 0, dimension - 1, ViterbiNode.NodeType.KNOWN);
            AddNode(eosNode, dimension - 1, 0);
        }

        public void AddNode(ViterbiNode node, int start, int end)
        {
            AddNodeToArray(node, start, StartIndexArr, StartSizeArr);
            AddNodeToArray(node, end, EndIndexArr, EndSizeArr);
        }

        private void AddNodeToArray(ViterbiNode node, int index, ViterbiNode[][] arr, int[] sizes)
        {
            int count = sizes[index];

            ExpandIfNeeded(index, arr, count);

            arr[index][count] = node;
            sizes[index] = count + 1;
        }

        private void ExpandIfNeeded(int index, ViterbiNode[][] arr, int count)
        {
            if (count == 0)
            {
                arr[index] = new ViterbiNode[10];
            }

            if (arr[index].Length <= count)
            {
                arr[index] = ExtendArray(arr[index]);
            }
        }

        private ViterbiNode[] ExtendArray(ViterbiNode[] array)
        {
            ViterbiNode[] newArray = new ViterbiNode[array.Length * 2];
            Array.Copy(array, 0, newArray, 0, array.Length);
            return newArray;
        }

        public bool TokenEndsWhereCurrentTokenStarts(int startIndex)
        {
            return EndSizeArr[startIndex + 1] != 0;
        }
    }
}
