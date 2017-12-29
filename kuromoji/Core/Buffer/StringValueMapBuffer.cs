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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJapaneseDictionary.Kuromoji.Core;
using NLPJapaneseDictionary.Kuromoji.Core.util;

namespace NLPJapaneseDictionary.Kuromoji.Core.Buffer
{
    public class StringValueMapBuffer : IDisposable
    {
        private const short KATAKANA_FLAG = unchecked((short)0x8000);

        private const short KATAKANA_LENGTH_MASK = unchecked((short)0x7fff);

        private const char KATAKANA_BASE = '\u3000'; // Katakana start at U+30A0

        private MemoryStreamWrapper buffer;
        private byte[] streamArray;

        private int size;

        public StringValueMapBuffer(SortedDictionary<int, string> features)
        {
            Put(features);
            streamArray = buffer.Stream.ToArray();
        }

        public StringValueMapBuffer(Stream inputStream)
        {
            buffer = new MemoryStreamWrapper(ByteBufferIO.Read(inputStream));
            streamArray = buffer.Stream.ToArray();
            size = buffer.ReadInt32();
        }

        public string Get(int key)
        {
            Debug.Assert(key >= 0 && key < size);

            int keyIndex = (key + 1) * Constant.INTEGER_BYTES;
            int valueIndex = buffer.ReadInt32(keyIndex);
            int length = buffer.ReadInt16(valueIndex);

            if ((length & KATAKANA_FLAG) != 0)
            {
                length &= KATAKANA_LENGTH_MASK;
                return GetKatakanaString(valueIndex + Constant.SHORT_BYTES, length);
            }
            else
            {
                return GetString(valueIndex + Constant.SHORT_BYTES, length);
            }
        }

        private string GetKatakanaString(int valueIndex, int length)
        {
            char[] str = new char[length];

            for (int i = 0; i < length; i++)
            {
                str[i] = (char)(KATAKANA_BASE + (streamArray[valueIndex + i] & 0xff));
            }

            return new String(str);
        }

        private string GetString(int valueIndex, int length)
        {
            return Encoding.Unicode.GetString(streamArray, valueIndex, length);
        }

        public void Write(Stream output)
        {
            ByteBufferIO.Write(output, buffer.Stream);
        }

        private void Put(SortedDictionary<int, string> strings)
        {
            int bufferSize = CalculateSize(strings);
            size = strings.Count;

            if (buffer != null)
                buffer.Dispose();

            buffer = new MemoryStreamWrapper(new MemoryStream(bufferSize));
            buffer.WriteInt32(size); // Set entries

            int keyIndex = Constant.INTEGER_BYTES; // First key index is past size
            int entryIndex = keyIndex + size * Constant.INTEGER_BYTES;

            foreach (string str in strings.Values)
            {
                buffer.WriteInt32(entryIndex, keyIndex);
                entryIndex = Put(entryIndex, str);
                keyIndex += Constant.INTEGER_BYTES;
            }
        }

        private int Put(int index, string value)
        {
            bool katakana = StringHelper.IsKatakanaOnly(value);
            byte[] bytes;
            short length;

            if (katakana)
            {
                bytes = GetKatakanaBytes(value);
                length = (short)(bytes.Length | KATAKANA_FLAG & 0xffff);
            }
            else
            {
                bytes = GetBytes(value);
                length = (short)bytes.Length;
            }

            buffer.Stream.Position = index;
            buffer.WriteInt16(length, index);
            buffer.Stream.Position = index + Constant.SHORT_BYTES;
            buffer.Write(bytes, 0, bytes.Length);

            return index + Constant.SHORT_BYTES + bytes.Length;
        }

        private int CalculateSize(SortedDictionary<int, string> values)
        {
            int size = Constant.INTEGER_BYTES + values.Count * Constant.INTEGER_BYTES;

            foreach (string value in values.Values)
            {
                size += Constant.SHORT_BYTES + GetByteSize(value);
            }
            return size;
        }

        private int GetByteSize(string str)
        {
            if (StringHelper.IsKatakanaOnly(str))
            {
                return str.Length;
            }

            return GetBytes(str).Length;
        }

        private byte[] GetKatakanaBytes(string str)
        {
            int length = str.Length;
            byte[] bytes = new byte[length];

            for (int i = 0; i < length; i++)
            {
                char c = str[i];

                bytes[i] = (byte)(c - KATAKANA_BASE);
            }

            return bytes;
        }

        private byte[] GetBytes(string str)
        {
            return Encoding.Unicode.GetBytes(str);
        }

        public void Dispose()
        {
            if(buffer != null)
                buffer.Dispose();
            buffer = null;
        }
    }
}
