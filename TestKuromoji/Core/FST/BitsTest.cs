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
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.FST;

namespace NLPJDictTest.kuromoji.Core.FST
{
    [TestClass]
    public class BitsTest
    {
        [TestMethod]
        public void TestBits()
        {
            byte[] bytes = new byte[] { 90, unchecked((byte)-1), 0, 0, 0, unchecked((byte)-112) , 0, 0, 0, 6, 0, 5, 1 };
            Assert.AreEqual(1, Bits.GetByte(bytes, bytes.Length - 1));
            Assert.AreEqual(5, Bits.GetShort(bytes, bytes.Length - (1 + 1)));
            Assert.AreEqual(6, Bits.GetInt(bytes, bytes.Length - (1 + 1 + 2)));
            Assert.AreEqual(144, Bits.GetInt(bytes, bytes.Length - (1 + 1 + 2 + 4)));
        }
    }
}
