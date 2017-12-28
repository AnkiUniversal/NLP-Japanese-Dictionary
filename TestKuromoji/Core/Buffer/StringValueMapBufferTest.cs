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
using NLPJDict.Kuromoji.Core.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.kuromoji.Core
{
    [TestClass]
    public class StringValueMapBufferTest
    {
        [TestMethod]
        public void TestInsertIntoMap()
        {
            SortedDictionary<int, string> input = new SortedDictionary<int, string>();

            input.Add(1, "hello");
            input.Add(2, "日本");
            input.Add(3, "カタカナ");
            input.Add(0, "Bye");

            using (StringValueMapBuffer values = new StringValueMapBuffer(input))
            {

                Assert.AreEqual("Bye", values.Get(0));
                Assert.AreEqual("hello", values.Get(1));
                Assert.AreEqual("日本", values.Get(2));
                Assert.AreEqual("カタカナ", values.Get(3));
            }
        }
    }
}
