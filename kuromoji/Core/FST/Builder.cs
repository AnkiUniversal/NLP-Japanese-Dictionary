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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.FST
{
    public class Builder : IDisposable
    {
        // Note that FST only allows the presorted dictionaries as input.
        private Dictionary<int, LinkedList<State>> statesDictionary;

        private Compiler compiler = new Compiler();

        private List<State> tempStates;

        public Builder()
        {
            LinkedList<State> stateList = new LinkedList<State>();
            stateList.AddLast(new State());
            this.statesDictionary = new Dictionary<int, LinkedList<State>>();
            this.statesDictionary[0] = stateList; // temporary setting the start state

            tempStates = new List<State>();
            tempStates.Add(this.GetStartState()); // initial state
        }

        /**
         * Applies the transducer over the input text
         *
         * @param input  input text to transduce
         * @return corresponding value on a match and -1 otherwise
         */
        public int Transduce(string input)
        {
            State currentState = this.GetStartState();
            int output = 0; // assuming that output is a int type

            // transitioning according to input
            for (int i = 0; i < input.Length; i++)
            {
                char currentTransition = input[i];
                Arc nextArc = currentState.FindArc(currentTransition);
                if (nextArc == null)
                {
                    return -1;
                }
                currentState = nextArc.GetDestination();
                output += nextArc.GetOutput();
            }

            return output;
        }

        /**
         * Get starting state. Note that only start state uses 0 as the key for states dictionary.
         *
         * @return start state
         */
        public State GetStartState()
        {
            return this.statesDictionary[0].First();
        }

        /**
         * For this method, once it reads the string, it throws away.
         *
         * @param reader  reader to read dictionary entries from
         * @throws IOException in case of an IO error
         */
        public void CreateDictionaryIncremental(StreamReader reader)
        {
            try
            {
                string previousWord = "";

                int outputValue = 1; // Initialize output value

                string line;
                while (!reader.EndOfStream)
                {
                    line = Regex.Replace(reader.ReadLine(), "#.*$", "");

                    if (String.IsNullOrWhiteSpace(line.Trim()))
                    {
                        continue;
                    }
                    string inputWord = line;
                    CreateDictionaryCommon(inputWord, previousWord, outputValue);
                    previousWord = inputWord;
                    outputValue++; // allocate the next wordID
                }

                HandleLastWord(previousWord);
            }
            catch (IOException ex)
            {
                throw new Exception("Builder.CreateDictionaryIncremental: " + ex.Message);
            }
        }


        /**
         * builds FST given input words and output values
         *
         * @param inputWords  array of input strings, not null
         * @param outputValues  array of output values
         * @throws IOException in case of an IO error
         */
        public void Build(string[] inputWords, int[] outputValues)
        {
            try
            {
                string previousWord = "";

                for (int inputWordIdx = 0; inputWordIdx < inputWords.Length; inputWordIdx++)
                {
                    string inputWord = inputWords[inputWordIdx];
                    CreateDictionaryCommon(
                        inputWord,
                        previousWord,
                        outputValues == null ? inputWordIdx + 1 : outputValues[inputWordIdx]
                    );
                    previousWord = inputWord;
                }

                HandleLastWord(previousWord);
            }
            catch (IOException ex)
            {
                throw new Exception("Builder.Build: " + ex.Message);
            }
        }

        private void CreateDictionaryCommon(string inputWord, string previousWord, int currentOutput)
        {
            try
            {
                int commonPrefixLengthPlusOne = CommonPrefixIndice(previousWord, inputWord) + 1;
                // We minimize the states from the suffix of the previous word

                // Dynamically adding additional temporary states if necessary
                if (inputWord.Length >= tempStates.Count)
                {
                    for (int j = tempStates.Count; j <= inputWord.Length; j++)
                    {
                        tempStates.Add(new State());
                    }
                }

                for (int i = previousWord.Length; i >= commonPrefixLengthPlusOne; i--)
                {
                    FreezeAndPointToNewState(previousWord, i);
                }

                for (int i = commonPrefixLengthPlusOne; i <= inputWord.Length; i++)
                {
                    tempStates[i] = new State(); // clearing and assigning new state
                    tempStates[i - 1].SetArc(inputWord[i - 1], tempStates[i]);
                }
                tempStates[(inputWord.Length)].SetFinal();

                // dealing with common prefix between previous word and the current word
                // (also note that its output must have common prefix too.)
                State currentState = tempStates[0];

                for (int i = 0; i < commonPrefixLengthPlusOne - 1; i++)
                {
                    Arc nextArc = currentState.FindArc(inputWord[i]);
                    currentOutput = ExcludePrefix(currentOutput, nextArc.GetOutput());
                    currentState = nextArc.GetDestination();
                }

                // currentOutput is the difference of outputs
                State suffixHeadState = tempStates[commonPrefixLengthPlusOne - 1];
                suffixHeadState.FindArc(inputWord[commonPrefixLengthPlusOne - 1]).SetOutput(currentOutput);
            }
            catch (IOException ex)
            {
                throw new Exception("Builder.Build: " + ex.Message);
            }
        }

        /**
         * Freeze a new state if there is no equivalent state in the states dictionary.
         *
         * @param previousWord
         * @param i
         */
        private void FreezeAndPointToNewState(string previousWord, int i)
        {
            try
            {
                State state = tempStates[i - 1];
                char previousWordChar = previousWord[i - 1];
                int output = state.FindArc(previousWordChar).GetOutput();
                state.Arcs.Remove(state.FindArc(previousWordChar));
                Arc arcToFrozenState = state.SetArc(previousWordChar, output, FindEquivalentState(tempStates[i]));

                compiler.CompileState(arcToFrozenState.GetDestination());
            }
            catch (IOException ex)
            {
                throw new Exception("Builder.FreezeAndPointToNewState: " + ex.Message);
            }
        }

        /**
         * Freezing temp states which represent the last word of the input words
         *
         * @param previousWord
         */
        private void HandleLastWord(string previousWord)
        {
            try
            {
                for (int i = previousWord.Length; i > 0; i--)
                {
                    FreezeAndPointToNewState(previousWord, i);
                }
                CompileStartingState();
                FindEquivalentState(tempStates[0]); // not necessary when compiling is enabled
            }
            catch (IOException ex)
            {
                throw new Exception("Builder.HandleLastWord: " + ex.Message);
            }
        }

        /**
         * Compiles and caches the outgoing arcs from the starting state
         */
        private void CompileStartingState()
        {
            try
            {
                compiler.CompileState(tempStates[0]);
            }
            catch (IOException ex)
            {
                throw new Exception("Builder.CompileStartingState: " + ex.Message);
            }
        }

        /**
         * Returns the indice of common prefix + 1
         *
         * @param prevWord
         * @param currentWord
         * @return
         */
        private int CommonPrefixIndice(String prevWord, String currentWord)
        {
            int i = 0;

            while (i < prevWord.Length && i < currentWord.Length)
            {
                if (prevWord[i] != currentWord[i])
                {
                    break;
                }
                i += 1;
            }
            return i;
        }

        /**
         * Exclude output of the common prefix from the current output
         *
         * @param word
         * @param prefix
         * @return
         */
        private int ExcludePrefix(int word, int prefix)
        {
            return word - prefix;
        }

        /**
         * Find the equivalent state by checking its destination states to when collided.
         *
         * @param state  state to check for equivalence
         * @return returns an equivalent state which is already in the stateDicitonary. If there is no equivalent state,
         * then a new state will created and put into statesDictionary.
         */
        private State FindEquivalentState(State state)
        {
            int key = state.GetHashCode(); // this is going to be the hashCode.

            if (statesDictionary.ContainsKey(key))
            {

                if (state.Arcs.Count == 0)
                {
                    // the dead end state (which is unique!)
                    return statesDictionary[key].First();
                }

                // Here, there are multiple states that has the same hashcode. Linear Probing the collidedStates.
                foreach (State collidedState in statesDictionary[key])
                {
                    if (state.Equals(collidedState))
                    {
                        return collidedState;
                    }
                }
            }
            // At this point, we know that there is no equivalent compiled (finalized) node
            State newStateToDic = new State(state); // deep copy
            LinkedList<State> stateList = new LinkedList<State>();
            if (statesDictionary.ContainsKey(key))
            {
                stateList = statesDictionary[key];
                // adding new state to a key
            }
            stateList.AddLast(newStateToDic);
            statesDictionary[key] = stateList;

            return newStateToDic;
        }

        public Compiler GetCompiler()
        {
            return compiler;
        }

        public void Dispose()
        {
            if(compiler != null)
                compiler.Dispose();
        }
    }
}
