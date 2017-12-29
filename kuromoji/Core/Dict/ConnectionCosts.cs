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

namespace NLPJapaneseDictionary.Kuromoji.Core.Dict
{
    public class ConnectionCosts : IDisposable
    {

        public const string CONNECTION_COSTS_FILENAME = "connectionCosts.bin";

        private int size;

        private MemoryStreamWrapper costs;

        public ConnectionCosts(int size, MemoryStreamWrapper costs)
        {
            this.size = size;
            this.costs = costs;
        }

        public int Get(int forwardId, int backwardId)
        {
            return costs.ReadInt16((backwardId + forwardId * size)*Constant.SHORT_BYTES);
        }

        public static ConnectionCosts NewInstance(string resourceAbsolutePath)
        {
            try
            {
                using (Stream stream = File.OpenRead(resourceAbsolutePath + Path.DirectorySeparatorChar + CONNECTION_COSTS_FILENAME))
                {
                    return Read(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ConnectionCosts.NewInstance: " + ex.Message);
            }
        }

        private static ConnectionCosts Read(Stream input)
        {
            try
            {
                lock (input)
                {
                    using (BinaryReader reader = new BinaryReader(input))
                    {
                        int size = reader.ReadRawInt32();                        
                        var costs = new MemoryStreamWrapper(ByteBufferIO.Read(reader));
                        return new ConnectionCosts(size, costs);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ConnectionCosts.Read: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if(costs != null)
                costs.Dispose();
        }
    }
}
