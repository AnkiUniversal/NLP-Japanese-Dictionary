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

using NLPJapaneseDictionary.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.FST
{
    public class BitsFormatter
    {  
        public string Format(Stream input)
        {
            using (MemoryStream byteOutput = ByteBufferIO.Read(input))
            {
                return Format(byteOutput.ToArray());
            }
        }

        public string Format(byte[] fst)
        {
            StringBuilder builder = new StringBuilder();

            int address = fst.Length - 1;

            while (address > 0)
            {
                builder.Append(FormatState(fst, address));

                address -= StateSize(fst, address);
            }
            return builder.ToString();
        }

        public int StateSize(byte[] fst, int address)
        {
            byte stateTypByte = Bits.GetByte(fst, address);
            int jumpBytes = (stateTypByte & 0x03) + 1;
            int accumulateBytes = (stateTypByte & 0x03 << 3) >> 3;

            return 1 + 2 + Bits.GetShort(fst, address - 1) * (2 + accumulateBytes + jumpBytes);
        }

        public string FormatState(byte[] fst, int address)
        {
            StringBuilder builder = new StringBuilder();

            byte stateByte = Bits.GetByte(fst, address);
            byte stateType = (byte)(stateByte & 0x80);
            int jumpBytes = (stateByte & 0x03) + 1;
            int accumulateBytes = (stateByte & 0x03 << 3) >> 3;

            builder.Append(FormatStateType(stateType, address));
            builder.Append(FormatArcs(fst, address - 1, accumulateBytes, jumpBytes));

            return builder.ToString();
        }

        public string FormatStateType(byte stateByte, int address)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(FormatAddress(address));
            builder.Append(" ");

            if (stateByte == Compiler.STATE_TYPE_ACCEPT)
            {
                builder.Append("ACCEPT");
            }
            else if (stateByte == Compiler.STATE_TYPE_MATCH)
            {
                builder.Append("MATCH");
            }
            else
            {
                throw new FormatException("Illegal state type: " + stateByte);
            }

            builder.Append("\n");
            return builder.ToString();
        }

        public string FormatAddress(int address)
        {
            return String.Format("{0,4:####}:", address.ToString());
        }

        public string FormatArcs(byte[] fst, int address, int accumulateBytes, int jumpBytes)
        {
            StringBuilder builder = new StringBuilder();
            int arcs = Bits.GetShort(fst, address);

            address -= 2;

            for (int i = 0; i < arcs; i++)
            {
                builder.Append(FormatAddress(address));
                builder.Append(FormatArc(fst, address, accumulateBytes, jumpBytes));
                builder.Append("\n");
                address -= 2 + accumulateBytes + jumpBytes;
            }

            return builder.ToString();
        }

        public string FormatArc(byte[] fst, int address, int accumulateBytes, int jumpBytes)
        {
            StringBuilder builder = new StringBuilder();
            int output = Bits.GetInt(fst, address, accumulateBytes);
            address -= accumulateBytes;

            int jumpAddress = Bits.GetInt(fst, address, jumpBytes);
            address -= jumpBytes;

            char label = (char)Bits.GetShort(fst, address);
            //        address -= 1;

            builder.Append('\t');
            builder.Append(label);
            builder.Append(" -> ");
            builder.Append(output);
            builder.Append("\t(JMP: ");
            builder.Append(jumpAddress);
            builder.Append(')');
            return builder.ToString();
        }
    }
}
