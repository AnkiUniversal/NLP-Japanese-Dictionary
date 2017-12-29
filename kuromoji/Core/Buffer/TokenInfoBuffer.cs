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

namespace NLPJapaneseDictionary.Kuromoji.Core.Buffer
{
    public class TokenInfoBuffer : IDisposable
    {
        private MemoryStreamWrapper buffer;

        private readonly int tokenInfoCount;
        private readonly int posInfoCount;
        private readonly int featureCount;

        private readonly int entrySize;

        public TokenInfoBuffer(Stream inputStream)
        {
            try
            {
                if (buffer != null)
                    buffer.Dispose();

                buffer = new MemoryStreamWrapper(ByteBufferIO.Read(inputStream));
                tokenInfoCount = GetTokenInfoCount();
                posInfoCount = GetPosInfoCount();
                featureCount = GetFeatureCount();
                entrySize = GetEntrySize(tokenInfoCount, posInfoCount, featureCount);
            }
            catch (Exception ex)
            {
                throw new IOException("TokenInfoBuffer Constructor: " + ex.Message);
            }
        }

        public BufferEntry LookupEntry(int offset)
        {
            BufferEntry entry = new BufferEntry();

            entry.TokenInfos = new short[tokenInfoCount];
            entry.PosInfos = new byte[posInfoCount];
            entry.FeatureInfos = new int[featureCount];

            int position = GetPosition(offset, entrySize);

            // Get left id, right id and word cost
            for (int i = 0; i < tokenInfoCount; i++)
            {
                entry.TokenInfos[i] = buffer.ReadInt16(position + i * Constant.SHORT_BYTES);
            }

            // Get part of speech tags values (not strings yet)
            for (int i = 0; i < posInfoCount; i++)
            {
                entry.PosInfos[i] = buffer.ReadByteAt(position + tokenInfoCount * Constant.SHORT_BYTES + i);
            }

            // Get field value references (string references)
            for (int i = 0; i < featureCount; i++)
            {
                entry.FeatureInfos[i] = buffer.ReadInt32(position + tokenInfoCount * Constant.SHORT_BYTES + posInfoCount + i * Constant.INTEGER_BYTES);
            }

            return entry;
        }

        public int LookupTokenInfo(int offset, int i)
        {
            int position = GetPosition(offset, entrySize);
            return buffer.ReadInt16(position + i * Constant.SHORT_BYTES);
        }

        public int LookupPartOfSpeechFeature(int offset, int i)
        {
            int position = GetPosition(offset, entrySize);

            return 0xff & buffer.ReadByteAt(position + tokenInfoCount * Constant.SHORT_BYTES + i);
        }

        public int LookupFeature(int offset, int i)
        {
            int position = GetPosition(offset, entrySize);

            return buffer.ReadInt32(position + tokenInfoCount * Constant.SHORT_BYTES + posInfoCount + (i - posInfoCount) * Constant.INTEGER_BYTES);
        }

        public bool IsPartOfSpeechFeature(int i)
        {
            int posInfoCount = GetPosInfoCount();
            return (i < posInfoCount);
        }

        private int GetTokenInfoCount()
        {
            return buffer.ReadInt32(Constant.INTEGER_BYTES * 2);
        }

        private int GetPosInfoCount()
        {
            return buffer.ReadInt32(Constant.INTEGER_BYTES * 3);
        }

        private int GetFeatureCount()
        {
            return buffer.ReadInt32(Constant.INTEGER_BYTES * 4);
        }

        private int GetEntrySize(int tokenInfoCount, int posInfoCount, int featureCount)
        {
            return tokenInfoCount * Constant.SHORT_BYTES + posInfoCount + featureCount * Constant.INTEGER_BYTES;
        }

        private int GetPosition(int offset, int entrySize)
        {
            return offset * entrySize + Constant.INTEGER_BYTES * 5;
        }

        public void Dispose()
        {
            if(buffer != null)
                buffer.Dispose();
        }
    }
}
