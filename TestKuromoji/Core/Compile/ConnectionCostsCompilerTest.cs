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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJapaneseDictionary.Kuromoji.Core.Compile;
using NLPJapaneseDictionary.Kuromoji.Core.IO;
using NLPJapaneseDictionary.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;

namespace NLPJDictTest.kuromoji.Core.Compile
{
    [TestClass]
    public class ConnectionCostsCompilerTest
    {
        private static ConnectionCosts connectionCosts;
        private static string costFile = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "./kuromoji-connectioncosts-.bin";

        [TestCleanup]
        public void Clean()
        {
            connectionCosts.Dispose();
        }

        [TestInitialize]
        public void SetUp()
        {            
            string costs = "" +
            "3 3\n" +
            "0 0 1\n" +
            "0 1 2\n" +
            "0 2 3\n" +
            "1 0 4\n" +
            "1 1 5\n" +
            "1 2 6\n" +
            "2 0 7\n" +
            "2 1 8\n" +
            "2 2 9\n";

            using (ConnectionCostsCompiler compiler = new ConnectionCostsCompiler())
            using (var outputStream = File.Create(costFile))
            {
                var bytes = Encoding.UTF8.GetBytes(costs);
                Stream inputStream = new MemoryStream(bytes);
                compiler.ReadCosts(inputStream);
                compiler.Compile(outputStream);
            }

            using (var readStream = File.OpenRead(costFile))
            using (var reader = new BinaryReader(readStream))
            {
                int size = reader.ReadRawInt32();
                var costsBuffer = new MemoryStreamWrapper(ByteBufferIO.Read(readStream));
                connectionCosts = new ConnectionCosts(size, costsBuffer);
            }
        }

        [TestMethod]
        public void TestCosts()
        {
            int cost = 1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.AreEqual(cost++, connectionCosts.Get(i, j));
                }
            }
        }
    }
}
