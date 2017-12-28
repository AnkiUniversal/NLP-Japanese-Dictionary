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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.FST
{
    public class Compiler : IDisposable
    {

        /**
         * <pre>
         * {@code
         * 1 byte   bit 7: true - accept state, false - match state
         *          bits 3-6 indicate number of bytes in output value (m)
         *          bits 0-2 indicate number of bytes in jump address (n)
         * 2 bytes  number of outgoing arcs
         * [
         *  (
         *   2 bytes label (char),
         *   n bytes jump address,
         *   m bytes accumlator
         *  )
         * ]
         * }
         * </pre>
         */
        public const byte STATE_TYPE_MATCH = (byte)0x00;

        public const byte STATE_TYPE_ACCEPT = (byte)0x80;

        private MemoryStream byteArrayOutput;

        private BinaryWriter dataOutput;

        private int written = 0;

        public Compiler()
        {
            byteArrayOutput = new MemoryStream();
            dataOutput = new BinaryWriter(byteArrayOutput);
        }

        public void CompileState(State state)
        {
            try
            {
                if (state.GetTargetJumpAddress() == -1)
                {
                    int jumpBytes = FindMaxJumpAddressBytes(state);
                    int outputBytes = FindMaxOutputBytes(state);

                    WriteStateArcs(state, outputBytes, jumpBytes);
                    WriteStateType(state, outputBytes, jumpBytes);

                    // The last arc is regarded as a state because we evaluate the FST backwards.
                    state.SetTargetJumpAddress(written - 1);
                }
            }
            catch (IOException ex)
            {
                throw new Exception("Compiler.CompileState: " + ex.Message);
            }
        }

        private void WriteStateType(State state, int outputBytes, int jumpBytes)
        {
            try
            {
                byte stateType;

                if (state.IsFinal)
                {
                    stateType = STATE_TYPE_ACCEPT;
                }
                else
                {
                    stateType = STATE_TYPE_MATCH;
                }

                stateType = (byte)(stateType | (jumpBytes - 1));
                stateType = (byte)(stateType | (outputBytes << 3));

                dataOutput.Write(stateType);

                written += 1;
            }
            catch (IOException ex)
            {
                throw new Exception("Compiler.CompileState: " + ex.Message);
            }
        }

        private void WriteStateArcs(State state, int outputBytes, int jumpBytes)
        {
            try
            {
                List<Arc> arcs = state.Arcs;

                foreach (Arc arc in arcs)
                {
                    WriteStateArc(arc, outputBytes, jumpBytes);
                }

                WriteIntValue(arcs.Count, Constant.SHORT_BYTES);
                written += 2;
            }
            catch (IOException ex)
            {
                throw new Exception("Compiler.CompileState: " + ex.Message);
            }
        }

        private void WriteStateArc(Arc arc, int outputBytes, int jumpBytes)
        {
            try
            {
                State target = arc.GetDestination();
                int arcSize = 2 + jumpBytes + outputBytes; // label + bytes for a jump + output

                WriteIntValue(arc.GetLabel(), Constant.SHORT_BYTES);
                WriteIntValue(target.GetTargetJumpAddress(), jumpBytes);
                WriteIntValue(arc.GetOutput(), outputBytes);

                written += arcSize;
            }
            catch (IOException ex)
            {
                throw new Exception("Compiler.CompileState: " + ex.Message);
            }
        }

        private void WriteIntValue(int value, int bytes)
        {
            try
            {
                switch (bytes)
                {
                    case 0:
                        break;

                    case 1:
                        dataOutput.Write((byte)(value & 0xff));
                        break;

                    case 2:
                        dataOutput.Write((byte)((value >> 8) & 0xff));
                        dataOutput.Write((byte)(value & 0xff));
                        break;

                    case 3:
                        dataOutput.Write((byte)((value >> 16) & 0xff));
                        dataOutput.Write((byte)((value >> 8) & 0xff));
                        dataOutput.Write((byte)(value & 0xff));
                        break;

                    case 4:
                        dataOutput.Write((byte)((value >> 24) & 0xff));
                        dataOutput.Write((byte)((value >> 16) & 0xff));
                        dataOutput.Write((byte)((value >> 8) & 0xff));
                        dataOutput.Write((byte)(value & 0xff));
                        break;

                    default:
                        throw new Exception("Illegal int byte size: " + bytes);
                }                
            }
            catch (IOException ex)
            {
                throw new Exception("Compiler.CompileState: " + ex.Message);
            }
        }

        private int FindMaxJumpAddressBytes(State state)
        {
            int maxJumpAddress = 0;

            foreach (Arc arc in state.Arcs)
            {
                int jumpAddress = arc.GetDestination().GetTargetJumpAddress();

                if (maxJumpAddress < jumpAddress)
                {
                    maxJumpAddress = jumpAddress;
                }
            }

            return FindNumberOfBytes(maxJumpAddress);
        }

        private int FindMaxOutputBytes(State state)
        {
            int maxOutput = 0;

            foreach (Arc arc in state.Arcs)
            {
                int output = arc.GetOutput();

                if (maxOutput < output)
                {
                    maxOutput = output;
                }
            }

            if (maxOutput == 0)
            {
                return 0;
            }

            return FindNumberOfBytes(maxOutput);
        }

        private int FindNumberOfBytes(int value)
        {
            Debug.Assert(value >= 0);
            if (value < 256)
            {
                return 1;
            }

            if (value < 65536)
            {
                return 2;
            }

            if (value < 16777216)
            {
                return 3;
            }

            return 4;
        }

        public byte[] GetBytes()
        {
            return byteArrayOutput.ToArray();
        }

        public void Dispose()
        {
            if(byteArrayOutput != null)
                byteArrayOutput.Dispose();
            if(dataOutput != null)
                dataOutput.Dispose();
        }
    }
}
