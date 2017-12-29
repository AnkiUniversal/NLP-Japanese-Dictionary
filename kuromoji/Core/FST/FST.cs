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

using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.FST
{
    public class FST
    {
        public const string FST_FILENAME = "fst.bin";

        private byte[] fst;

        private int[] jumpCache = new int[65536];

        private int[] outputCache = new int[65536];

        public FST(byte[] compiled)
        {
            this.fst = compiled;
            InitCache();
        }

        public FST(Stream input) : this(ByteBufferIO.Read(input).ToArray())
        {
        }

        private void InitCache()
        {
            jumpCache.Fill(-1);
            outputCache.Fill(-1);

            int address = fst.Length - 1;

            byte stateType = Bits.GetByte(fst, address);
            address -= 1;

            int jumpBytes = (stateType & 0x03) + 1;
            int outputBytes = (stateType & 0x03 << 3) >> 3;

            int arcs = Bits.GetShort(fst, address);
            address -= 2;

            for (int i = 0; i < arcs; i++)
            {
                int output = Bits.GetInt(fst, address, outputBytes);
                address -= outputBytes;

                int jump = Bits.GetInt(fst, address, jumpBytes);
                address -= jumpBytes;

                char label = (char)Bits.GetShort(fst, address);
                address -= 2;

                jumpCache[label] = jump;
                outputCache[label] = output;
            }
        }

        public int Lookup(string input)
        {
            int length = input.Length;
            int address = fst.Length - 1;
            int accumulator = 0;
            int index = 0;

            while (true)
            {
                byte stateTypByte = Bits.GetByte(fst, address);

                // The number of bytes in the target address (always larger than zero)
                int jumpBytes = (stateTypByte & 0x03) + 1;

                // The number of bytes in the output value
                int outputBytes = (stateTypByte & 0x03 << 3) >> 3;

                int arcSize = 2 + jumpBytes + outputBytes;

                byte stateType = (byte)(stateTypByte & 0x80);
                address -= 1;

                if (index == length)
                {
                    if (stateType == Compiler.STATE_TYPE_MATCH)
                    {
                        accumulator = 0; // Prefix match
                    }
                    return accumulator;
                }

                bool matched = false;
                char c = input[index];

                if (index == 0)
                {
                    //
                    // Processes cached root arcs - transition directly to the next state on a match
                    //
                    int jump = jumpCache[c];

                    if (jump == -1)
                    {
                        return -1;
                    }

                    int output = outputCache[c];
                    accumulator += output;

                    address = jump;
                    matched = true;
                }
                else
                {
                    //
                    // Transition to the next state by binary searching the output arcs
                    //
                    int numberOfArcs = Bits.GetShort(fst, address);
                    address -= 2;

                    if (numberOfArcs == 0)
                    {
                        return -1;
                    }

                    int high = numberOfArcs - 1;
                    int low = 0;

                    while (low <= high)
                    {
                        int middle = low + (high - low) / 2;
                        int arcAddr = address - middle * arcSize;

                        char label = GetArcLabel(arcAddr, outputBytes, jumpBytes);

                        if (label == c)
                        {
                            matched = true;
                            address = GetArcJump(arcAddr, outputBytes, jumpBytes);
                            accumulator += GetArcOutput(arcAddr, outputBytes, jumpBytes);
                            break;
                        }
                        else if (label > c)
                        {
                            low = middle + 1;
                        }
                        else
                        {
                            high = middle - 1;
                        }
                    }
                }

                if (matched == false)
                {
                    return -1;
                }

                index++;
            }
        }

        private char GetArcLabel(int arcAddress, int accumulateBytes, int jumpBytes)
        {
            return (char)Bits.GetShort(fst, arcAddress - (accumulateBytes + jumpBytes));
        }

        private int GetArcJump(int arcAddress, int accumulateBytes, int jumpBytes)
        {
            return Bits.GetInt(fst, arcAddress - accumulateBytes, jumpBytes);
        }

        private int GetArcOutput(int arcAddress, int accumulateBytes, int jumpBytes)
        {
            return Bits.GetInt(fst, arcAddress, accumulateBytes);
        }

        public static FST NewInstance(string absoluteResourcePath)
        {
            using (var stream = File.OpenRead(absoluteResourcePath + Path.DirectorySeparatorChar + FST_FILENAME))
            {
                return new FST(stream);
            }
        }
    }
}
