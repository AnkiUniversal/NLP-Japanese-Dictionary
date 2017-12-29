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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Viterbi
{
    public class ViterbiBuilder : IDisposable
    {

        private readonly FST.FST fst;
        private readonly TokenInfoDictionary dictionary;
        private readonly UnknownDictionary unknownDictionary;
        private readonly UserDictionary userDictionary;
        private readonly CharacterDefinitions characterDefinitions;
        private readonly bool useUserDictionary;
        private bool searchMode;

        /**
         * Constructor
         *
         * @param fst  FST with surface forms
         * @param dictionary  token info dictionary
         * @param unknownDictionary  unknown word dictionary
         * @param userDictionary  user dictionary
         * @param mode  tokenization {@link Mode mode}
         */
        public ViterbiBuilder(FST.FST fst,
                              TokenInfoDictionary dictionary,
                              UnknownDictionary unknownDictionary,
                              UserDictionary userDictionary,
                              Mode mode)
        {
            this.fst = fst;
            this.dictionary = dictionary;
            this.unknownDictionary = unknownDictionary;
            this.userDictionary = userDictionary;

            this.useUserDictionary = (userDictionary != null);

            if (mode == Mode.SEARCH || mode == Mode.EXTENDED)
            {
                searchMode = true;
            }
            this.characterDefinitions = unknownDictionary.GetCharacterDefinition();
        }
        
        /**
         * Build lattice from input text
         *
         * @param text  source text for the lattice
         * @return built lattice, not null
         */
        public ViterbiLattice Build(string text)
        {
            int textLength = text.Length;
            ViterbiLattice lattice = new ViterbiLattice(textLength + 2);

            lattice.AddBos();

            int unknownWordEndIndex = -1; // index of the last character of unknown word

            for (int startIndex = 0; startIndex < textLength; startIndex++)
            {
                // If no token ends where current token starts, skip this index
                if (lattice.TokenEndsWhereCurrentTokenStarts(startIndex))
                {

                    string suffix = text.Substring(startIndex);
                    bool found = ProcessIndex(lattice, startIndex, suffix);

                    // In the case of normal mode, it doesn't process unknown word greedily.
                    if (searchMode || unknownWordEndIndex <= startIndex)
                    {
                        int[] categories = characterDefinitions.LookupCategories(suffix[0]);

                        for (int i = 0; i < categories.Length; i++)
                        {
                            int category = categories[i];
                            unknownWordEndIndex = ProcessUnknownWord(category, i, lattice, unknownWordEndIndex, startIndex, suffix, found);
                        }
                    }
                }
            }

            if (useUserDictionary)
            {
                ProcessUserDictionary(text, lattice);
            }

            lattice.AddEos();

            return lattice;
        }

        private bool ProcessIndex(ViterbiLattice lattice, int startIndex, string suffix)
        {
            bool found = false;
            for (int endIndex = 1; endIndex < suffix.Length + 1; endIndex++)
            {
                string prefix = suffix.Substring(0, endIndex);

                int result = fst.Lookup(prefix);

                if (result > 0)
                {
                    found = true; // Don't produce unknown word starting from this index
                    foreach (int wordId in dictionary.LookupWordIds(result))
                    {
                        ViterbiNode node = new ViterbiNode(wordId, prefix, dictionary, startIndex, ViterbiNode.NodeType.KNOWN);
                        lattice.AddNode(node, startIndex + 1, startIndex + 1 + endIndex);
                    }
                }
                else if (result < 0)
                { // If result is less than zero, continue to next position
                    break;
                }
            }
            return found;
        }

        private int ProcessUnknownWord(int category, int i, ViterbiLattice lattice, int unknownWordEndIndex, int startIndex, String suffix, bool found)
        {
            int unknownWordLength = 0;
            int[] definition = characterDefinitions.LookupDefinition(category);

            if (definition[CharacterDefinitions.INVOKE] == 1 || found == false)
            {
                if (definition[CharacterDefinitions.GROUP] == 0)
                {
                    unknownWordLength = 1;
                }
                else
                {
                    unknownWordLength = 1;
                    for (int j = 1; j < suffix.Length; j++)
                    {
                        char c = suffix[j];

                        int[] categories = characterDefinitions.LookupCategories(c);

                        if (categories == null)
                        {
                            break;
                        }

                        if (i < categories.Length && category == categories[i])
                        {
                            unknownWordLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (unknownWordLength > 0)
            {
                string unkWord = suffix.Substring(0, unknownWordLength);
                int[] wordIds = unknownDictionary.LookupWordIds(category); // characters in input text are supposed to be the same

                foreach (int wordId in wordIds)
                {
                    ViterbiNode node = new ViterbiNode(wordId, unkWord, unknownDictionary, startIndex, ViterbiNode.NodeType.UNKNOWN);
                    lattice.AddNode(node, startIndex + 1, startIndex + 1 + unknownWordLength);
                }
                unknownWordEndIndex = startIndex + unknownWordLength;
            }

            return unknownWordEndIndex;
        }

        /**
         * Find token(s) in input text and set found token(s) in arrays as normal tokens
         *
         * @param text
         * @param lattice
         */
        private void ProcessUserDictionary(string text, ViterbiLattice lattice)
        {
            List<UserDictionary.UserDictionaryMatch> matches = userDictionary.FindUserDictionaryMatches(text);

            foreach (UserDictionary.UserDictionaryMatch match in matches)
            {
                int wordId = match.GetWordId();
                int index = match.GetMatchStartIndex();
                int length = match.GetMatchLength();

                string word = text.Substring(index, length);

                ViterbiNode node = new ViterbiNode(wordId, word, userDictionary, index, ViterbiNode.NodeType.USER);
                int nodeStartIndex = index + 1;
                int nodeEndIndex = nodeStartIndex + length;

                lattice.AddNode(node, nodeStartIndex, nodeEndIndex);

                if (IsLatticeBrokenBefore(nodeStartIndex, lattice))
                {
                    RepairBrokenLatticeBefore(lattice, index);
                }

                if (IsLatticeBrokenAfter(nodeStartIndex + length, lattice))
                {
                    RepairBrokenLatticeAfter(lattice, nodeEndIndex);
                }
            }
        }

        /**
         * Checks whether there exists any node in the lattice that connects to the newly inserted entry on the left side
         * (before the new entry).
         *
         * @param nodeIndex
         * @param lattice
         * @return whether the lattice has a node that ends at nodeIndex
         */
        private bool IsLatticeBrokenBefore(int nodeIndex, ViterbiLattice lattice)
        {
            ViterbiNode[][] nodeEndIndices = lattice.EndIndexArr;

            return nodeEndIndices[nodeIndex] == null;
        }

        /**
         * Checks whether there exists any node in the lattice that connects to the newly inserted entry on the right side
         * (after the new entry).
         *
         * @param endIndex
         * @param lattice
         * @return whether the lattice has a node that starts at endIndex
         */
        private bool IsLatticeBrokenAfter(int endIndex, ViterbiLattice lattice)
        {
            ViterbiNode[][] nodeStartIndices = lattice.StartIndexArr;

            return nodeStartIndices[endIndex] == null;
        }

        /**
         * Tries to repair the lattice by creating and adding an additional Viterbi node to the LEFT of the newly
         * inserted user dictionary entry by using the substring of the node in the lattice that overlaps the least
         *
         * @param lattice
         * @param index
         */
        private void RepairBrokenLatticeBefore(ViterbiLattice lattice, int index)
        {
            ViterbiNode[][] nodeStartIndices = lattice.StartIndexArr;

            for (int startIndex = index; startIndex > 0; startIndex--)
            {
                if (nodeStartIndices[startIndex] != null)
                {
                    ViterbiNode glueBase = FindGlueNodeCandidate(index, nodeStartIndices[startIndex], startIndex);
                    if (glueBase != null)
                    {
                        int length = index + 1 - startIndex;
                        String surface = glueBase.Surface.Substring(0, length);
                        ViterbiNode glueNode = MakeGlueNode(startIndex, glueBase, surface);
                        lattice.AddNode(glueNode, startIndex, startIndex + glueNode.Surface.Length);
                        return;
                    }
                }
            }
        }

        /**
         * Tries to repair the lattice by creating and adding an additional Viterbi node to the RIGHT of the newly
         * inserted user dictionary entry by using the substring of the node in the lattice that overlaps the least
         *  @param lattice
         * @param nodeEndIndex
         */
        private void RepairBrokenLatticeAfter(ViterbiLattice lattice, int nodeEndIndex)
        {
            ViterbiNode[][] nodeEndIndices = lattice.EndIndexArr;

            for (int endIndex = nodeEndIndex + 1; endIndex < nodeEndIndices.Length; endIndex++)
            {
                if (nodeEndIndices[endIndex] != null)
                {
                    ViterbiNode glueBase = FindGlueNodeCandidate(nodeEndIndex, nodeEndIndices[endIndex], endIndex);
                    if (glueBase != null)
                    {
                        int delta = endIndex - nodeEndIndex;
                        String glueBaseSurface = glueBase.Surface;
                        String surface = glueBaseSurface.Substring(glueBaseSurface.Length - delta);
                        ViterbiNode glueNode = MakeGlueNode(nodeEndIndex, glueBase, surface);
                        lattice.AddNode(glueNode, nodeEndIndex, nodeEndIndex + glueNode.Surface.Length);
                        return;
                    }
                }
            }
        }

        /**
         * Tries to locate a candidate for a "glue" node that repairs the broken lattice by looking at all nodes at the
         * current index.
         *
         * @param index
         * @param latticeNodes
         * @param startIndex
         * @return new ViterbiNode that can be inserted to glue the graph if such a node exists, else null
         */
        private ViterbiNode FindGlueNodeCandidate(int index, ViterbiNode[] latticeNodes, int startIndex)
        {
            List<ViterbiNode> candidates = new List<ViterbiNode>();

            foreach (ViterbiNode viterbiNode in latticeNodes)
            {
                if (viterbiNode != null)
                {
                    candidates.Add(viterbiNode);
                }
            }
            if (!(candidates.Count == 0))
            {
                ViterbiNode glueBase = null;
                int length = index + 1 - startIndex;
                foreach (ViterbiNode candidate in candidates)
                {
                    if (IsAcceptableCandidate(length, glueBase, candidate))
                    {
                        glueBase = candidate;
                    }
                }
                if (glueBase != null)
                {
                    return glueBase;
                }
            }
            return null;
        }

        /**
         * Check whether a candidate for a glue node is acceptable.
         * The candidate should be as short as possible, but long enough to overlap with the inserted user entry
         *
         * @param targetLength
         * @param glueBase
         * @param candidate
         * @return whether candidate is acceptable
         */
        private bool IsAcceptableCandidate(int targetLength, ViterbiNode glueBase, ViterbiNode candidate)
        {
            return (glueBase == null || candidate.Surface.Length < glueBase.Surface.Length) &&
                candidate.Surface.Length >= targetLength;
        }

        /**
         * Create a glue node to be inserted based on ViterbiNode already in the lattice.
         * The new node takes the same parameters as the node it is based on, but the word is truncated to match the
         * hole in the lattice caused by the new user entry
         *
         * @param startIndex
         * @param glueBase
         * @param surface
         * @return new ViterbiNode to be inserted as glue into the lattice
         */
        private ViterbiNode MakeGlueNode(int startIndex, ViterbiNode glueBase, String surface)
        {
            return new ViterbiNode(
                glueBase.WordId,
                surface,
                glueBase.LeftId,
                glueBase.RightId,
                glueBase.WordCost,
                startIndex,
                ViterbiNode.NodeType.INSERTED
            );
        }

        public void Dispose()
        {
            if(dictionary != null)
                dictionary.Dispose();
        }
    }
}
