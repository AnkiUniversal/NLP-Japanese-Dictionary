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
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public int Count { get { return data.Count; } }

        public PriorityQueue()
        {
            this.data = new List<T>();
        }

        public void Add(T item)
        {
            data.Add(item);
            int childIndex = data.Count - 1;
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
                    break;

                SwapParentAndChild(parentIndex, childIndex);
                childIndex = parentIndex;
            }
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }

        public T Peek()
        {
            T frontItem = data[0];
            return frontItem;
        }

        public T Poll()
        {
            if (IsEmpty())
                return default(T);

            // assumes pq is not empty; up to calling code
            int lastIndex = data.Count - 1; 
            T frontItem = data[0];   // fetch the front
            data[0] = data[lastIndex];
            data.RemoveAt(lastIndex);

            --lastIndex; 
            int parentIndex = 0; 
            while (true)
            {
                int leftChildIndex = parentIndex * 2 + 1; // left child index of parent
                if (leftChildIndex > lastIndex)
                    break;  // no children so done

                int rightChild = leftChildIndex + 1;

                // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                if (rightChild <= lastIndex && data[rightChild].CompareTo(data[leftChildIndex]) < 0)
                    leftChildIndex = rightChild;

                if (data[parentIndex].CompareTo(data[leftChildIndex]) <= 0)
                    break; // parent is smaller than (or equal to) smallest child so done

                SwapParentAndChild(parentIndex, leftChildIndex);
                parentIndex = leftChildIndex;
            }
            return frontItem;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (data.Count == 0)
                return true;
            int li = data.Count - 1; // last index
            for (int pi = 0; pi < data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && data[pi].CompareTo(data[lci]) > 0)
                    return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && data[pi].CompareTo(data[rci]) > 0) 
                        return false; // check the right child too.
            }
            return true; 
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < data.Count; ++i)
            {
                s.Append(data[i].ToString());
                s.Append(" ");
            }
            s.Append("count = " + data.Count);
            return s.ToString();
        }

        private void SwapParentAndChild(int parentIndex, int childIndex)
        {
            T tmp = data[parentIndex];
            data[parentIndex] = data[childIndex];
            data[childIndex] = tmp;
        }
    }
}
