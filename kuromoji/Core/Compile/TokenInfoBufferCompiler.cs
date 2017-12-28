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

using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class TokenInfoBufferCompiler : ICompiler, IDisposable
    {
        private MemoryStreamWrapper buffer;

        public TokenInfoBufferCompiler(List<BufferEntry> entries)
        {
            PutEntries(entries);
        }

        public void PutEntries(List<BufferEntry> entries)
        {
            int size = CalculateEntriesSize(entries) * 2;

            this.buffer = new MemoryStreamWrapper(new MemoryStream(size + Constant.INTEGER_BYTES * 4));

            buffer.WriteInt32(size);
            buffer.WriteInt32(entries.Count);
            BufferEntry firstEntry = entries[0];

            buffer.WriteInt32(firstEntry.TokenInfo.Count);
            buffer.WriteInt32(firstEntry.PosInfo.Count);
            buffer.WriteInt32(firstEntry.Features.Count);

            foreach (BufferEntry entry in entries)
            {
                foreach (short s in entry.TokenInfo)
                {
                    buffer.WriteInt16(s);
                }

                 buffer.Write(entry.PosInfo.ToArray(), 0, entry.PosInfo.Count);

                foreach (int feature in entry.Features)
                {
                    buffer.WriteInt32(feature);
                }
            }
        }

        private int CalculateEntriesSize(List<BufferEntry> entries)
        {
            if (entries.Count == 0)
            {
                return 0;
            }
            else
            {
                int size = 0;
                BufferEntry entry = entries[0];
                size += entry.TokenInfo.Count * Constant.SHORT_BYTES + Constant.SHORT_BYTES;
                size += entry.PosInfo.Count;
                size += entry.Features.Count * Constant.INTEGER_BYTES;
                size *= entries.Count;
                return size;
            }
        }

        public void Compile(Stream output)
        {
            try
            {
                ByteBufferIO.Write(output, buffer.Stream);
            }
            catch (IOException ex)
            {
                throw new IOException("TokenInfoBufferCompiler.Compile: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if(buffer != null)
                buffer.Dispose();
        }
    }
}
