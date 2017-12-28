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

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class MultiSearchMerger
    {

        private int baseCost;
        private List<int> suffixCostLowerBounds;
        private int maxCount;
        private int costSlack;

        public MultiSearchMerger(int maxCount, int costSlack)
        {
            this.maxCount = maxCount;
            this.costSlack = costSlack;
        }

        public MultiSearchResult Merge(List<MultiSearchResult> results)
        {
            if (results.Count == 0)
            {
                return new MultiSearchResult();
            }

            suffixCostLowerBounds = new List<int>();
            for (int i = 0; i < results.Count; i++)
            {
                suffixCostLowerBounds.Add(0);
            }
            suffixCostLowerBounds[suffixCostLowerBounds.Count - 1] = results[results.Count - 1].GetCost(0);
            for (int i = results.Count - 2; i >= 0; i--)
            {
                suffixCostLowerBounds[i] = results[i].GetCost(0) + suffixCostLowerBounds[i + 1];
            }
            baseCost = suffixCostLowerBounds[0];

            MultiSearchResult ret = new MultiSearchResult();
            List<MergeBuilder> builders = new List<MultiSearchMerger.MergeBuilder>();
            for (int i = 0; i < results[0].Count; i++)
            {
                if (GetCostLowerBound(results[0].GetCost(i), 0) - baseCost > costSlack || i == maxCount)
                {
                    break;
                }

                MergeBuilder newBuilder = new MergeBuilder(results);
                newBuilder.Add(i);
                builders.Add(newBuilder);
            }

            for (int i = 1; i < results.Count; i++)
            {
                builders = MergeStep(builders, results, i);
            }

            foreach (MergeBuilder builder in builders)
            {
                ret.Add(builder.BuildList(), builder.Cost);
            }

            return ret;
        }

        private List<MergeBuilder> MergeStep(List<MergeBuilder> builders, List<MultiSearchResult> results, int currentIndex)
        {
            MultiSearchResult nextResult = results[currentIndex];
            PriorityQueue<MergePair> pairHeap = new PriorityQueue<MergePair>();
            List<MergeBuilder> ret = new List<MultiSearchMerger.MergeBuilder>();

            if ((builders.Count == 0) || (nextResult.Count == 0))
            {
                return ret;
            }

            pairHeap.Add(new MergePair(0, 0, builders[0].Cost + nextResult.GetCost(0)));

            HashSet<int> visited = new HashSet<int>();

            while (ret.Count < maxCount && pairHeap.Count > 0)
            {
                MergePair top = pairHeap.Poll();

                if (GetCostLowerBound(top.Cost, currentIndex) - baseCost > costSlack)
                {
                    break;
                }

                int i = top.LeftIndex;
                int j = top.RightIndex;

                MergeBuilder nextBuilder = new MergeBuilder(results, builders[i].Indices);
                nextBuilder.Add(j);
                ret.Add(nextBuilder);

                if (i + 1 < builders.Count)
                {
                    MergePair newMergePair = new MergePair(i + 1, j, builders[i + 1].Cost + nextResult.GetCost(j));
                    int positionValue = GetPositionValue(i + 1, j);
                    if (!visited.Contains(positionValue))
                    {
                        pairHeap.Add(newMergePair);
                        visited.Add(positionValue);
                    }
                }
                if (j + 1 < nextResult.Count)
                {
                    MergePair newMergePair = new MergePair(i, j + 1, builders[i].Cost + nextResult.GetCost(j + 1));
                    int positionValue = GetPositionValue(i, j + 1);
                    if (!visited.Contains(positionValue))
                    {
                        pairHeap.Add(newMergePair);
                        visited.Add(positionValue);
                    }
                }
            }

            return ret;
        }

        private int GetPositionValue(int i, int j)
        {
            return (maxCount + 1) * i + j;
        }

        private int GetCostLowerBound(int currentCost, int index)
        {
            if (index + 1 < suffixCostLowerBounds.Count)
            {
                return currentCost + suffixCostLowerBounds[index + 1];
            }
            return currentCost;
        }

        private class MergeBuilder : IComparable<MergeBuilder>
        {
            public int Cost { get; private set; }
            public List<int> Indices { get; private set; }
            private List<MultiSearchResult> results;

            public MergeBuilder(List<MultiSearchResult> results)
            {
                this.results = results;
                Cost = 0;
                Indices = new List<int>();
            }

            public MergeBuilder(List<MultiSearchResult> results, List<int> indices) : this(results)
            {
                foreach (int index in indices)
                {
                    Add(index);
                }
            }

            public LinkedList<ViterbiNode> BuildList()
            {
                LinkedList<ViterbiNode> ret = new LinkedList<ViterbiNode>();
                for (int i = 0; i < Indices.Count; i++)
                {
                    var result = results[i].GetTokenizedResult(Indices[i]);
                    foreach (var node in result)
                    {
                        ret.AddLast(node);
                    }
                }
                return ret;
            }

            public void Add(int index)
            {
                Indices.Add(index);
                Cost += results[Indices.Count - 1].GetCost(index);
            }

            public int CompareTo(MergeBuilder o)
            {
                return Cost - o.Cost;
            }
        }

        private class MergePair : IComparable<MergePair>
        {
            public int LeftIndex { get; private set; }
            public int RightIndex { get; private set; }
            public int Cost { get; private set; }

            public MergePair(int leftIndex, int rightIndex, int cost)
            {
                this.LeftIndex = leftIndex;
                this.RightIndex = rightIndex;
                this.Cost = cost;
            }

            public int CompareTo(MergePair o)
            {
                return Cost - o.Cost;
            }
        }
    }
}
