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

using NLPJapaneseDictionary.Kuromoji.Core.Dict;
using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Viterbi
{
    public class ViterbiSearcher : IDisposable
    {

        private const int DEFAULT_COST = int.MaxValue;

        private readonly ConnectionCosts costs;
        private readonly UnknownDictionary unknownDictionary;

        private int kanjiPenaltyLengthThreshold;
        private int otherPenaltyLengthThreshold;
        private int kanjiPenalty;
        private int otherPenalty;

        private readonly Mode mode;

        private MultiSearcher multiSearcher;

        public void Dispose()
        {
            if(costs != null)
                costs.Dispose();     
            
            if(multiSearcher != null)
                multiSearcher.Dispose();
        }

        public ViterbiSearcher(Mode mode,
                               ConnectionCosts costs,
                               UnknownDictionary unknownDictionary,
                               List<int> penalties)
        {
            if (!(penalties.Count == 0))
            {
                this.kanjiPenaltyLengthThreshold = penalties[0];
                this.kanjiPenalty = penalties[1];
                this.otherPenaltyLengthThreshold = penalties[2];
                this.otherPenalty = penalties[3];
            }

            this.mode = mode;
            this.costs = costs;
            this.unknownDictionary = unknownDictionary;
            multiSearcher = new MultiSearcher(costs, mode, this);
        }

        /**
         * Find best path from input lattice.
         *
         * @param lattice the result of build method
         * @return List of ViterbiNode which consist best path
         */
        public List<ViterbiNode> Search(ViterbiLattice lattice)
        {

            ViterbiNode[][] endIndexArr = CalculatePathCosts(lattice);
            LinkedList<ViterbiNode> result = BacktrackBestPath(endIndexArr[0][0]);
            return result.ToList();
        }

        /**
         * Find the best paths with cost at most OPT + costSlack, where OPT is the optimal solution. At most maxCount paths will be returned. The paths are ordered by cost in ascending order.
         *
         * @param lattice  the result of a build method
         * @param maxCount  the maximum number of paths to find
         * @param costSlack  the maximum cost slack of a path
         * @return  MultiSearchResult containing the shortest paths and their costs
         */
        public MultiSearchResult SearchMultiple(ViterbiLattice lattice, int maxCount, int costSlack)
        {
            CalculatePathCosts(lattice);
            MultiSearchResult result = multiSearcher.GetShortestPaths(lattice, maxCount, costSlack);
            return result;
        }

        private ViterbiNode[][] CalculatePathCosts(ViterbiLattice lattice)
        {
            ViterbiNode[][] startIndexArr = lattice.StartIndexArr;
            ViterbiNode[][] endIndexArr = lattice.EndIndexArr;

            for (int i = 1; i < startIndexArr.Length; i++)
            {

                if (startIndexArr[i] == null || endIndexArr[i] == null)
                {    // continue since no array which contains ViterbiNodes exists. Or no previous node exists.
                    continue;
                }

                foreach (ViterbiNode node in startIndexArr[i])
                {
                    if (node == null)
                    {    // If array doesn't contain ViterbiNode any more, continue to next index
                        break;
                    }

                    UpdateNode(endIndexArr[i], node);
                }
            }
            return endIndexArr;
        }

        private void UpdateNode(ViterbiNode[] viterbiNodes, ViterbiNode node)
        {
            int backwardConnectionId = node.LeftId;
            int wordCost = node.WordCost;
            int leastPathCost = DEFAULT_COST;

            foreach (ViterbiNode leftNode in viterbiNodes)
            {
                // If array doesn't contain any more ViterbiNodes, continue to next index
                if (leftNode == null)
                {
                    return;
                }
                else
                {
                    // cost = [total cost from BOS to previous node] + [connection cost between previous node and current node] + [word cost]
                    int pathCost = leftNode.PathCost +
                        costs.Get(leftNode.RightId, backwardConnectionId) +
                        wordCost;

                    // Add extra cost for long nodes in "Search mode".
                    if (mode == Mode.SEARCH || mode == Mode.EXTENDED)
                    {
                        pathCost += GetPenaltyCost(node);
                    }

                    // If total cost is lower than before, set current previous node as best left node (previous means left).
                    if (pathCost < leastPathCost)
                    {
                        leastPathCost = pathCost;
                        node.PathCost = leastPathCost;
                        node.LeftNode = leftNode;
                    }
                }
            }
        }

        public int GetPenaltyCost(ViterbiNode node)
        {
            int pathCost = 0;
            string surface = node.Surface;
            int length = surface.Length;

            if (length > kanjiPenaltyLengthThreshold)
            {
                if (StringHelper.IsKanjiOnly(surface))
                {    // Process only Kanji keywords
                    pathCost += (length - kanjiPenaltyLengthThreshold) * kanjiPenalty;
                }
                else if (length > otherPenaltyLengthThreshold)
                {
                    pathCost += (length - otherPenaltyLengthThreshold) * otherPenalty;
                }
            }
            return pathCost;
        }

        private LinkedList<ViterbiNode> BacktrackBestPath(ViterbiNode eos)
        {
            ViterbiNode node = eos;
            LinkedList<ViterbiNode> result = new LinkedList<ViterbiNode>();

            result.AddLast(node);

            while (true)
            {
                ViterbiNode leftNode = node.LeftNode;

                if (leftNode == null)
                {
                    break;
                }
                else
                {
                    // Extended mode converts unknown word into unigram nodes
                    if (mode == Mode.EXTENDED && leftNode.Type == ViterbiNode.NodeType.UNKNOWN)
                    {
                        LinkedList<ViterbiNode> uniGramNodes = ConvertUnknownWordToUnigramNode(leftNode);
                        result.AddAll(uniGramNodes);
                    }
                    else
                    {
                        result.AddFirst(leftNode);
                    }
                    node = leftNode;
                }
            }
            return result;
        }

        private LinkedList<ViterbiNode> ConvertUnknownWordToUnigramNode(ViterbiNode node)
        {
            LinkedList<ViterbiNode> uniGramNodes = new LinkedList<ViterbiNode>();
            int unigramWordId = 0;
            String surface = node.Surface;

            for (int i = surface.Length; i > 0; i--)
            {
                string word = surface.Substring(i - 1, 1);
                int startIndex = node.StartIndex + i - 1;

                ViterbiNode uniGramNode = new ViterbiNode(unigramWordId, word, unknownDictionary, startIndex, ViterbiNode.NodeType.UNKNOWN);
                uniGramNodes.AddFirst(uniGramNode);
            }

            return uniGramNodes;
        }
    }
}
