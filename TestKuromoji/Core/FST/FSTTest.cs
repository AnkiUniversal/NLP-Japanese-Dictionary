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
    public class FSTTest
    {
        [TestMethod]
        public void TestFST()
        {
            string[] inputValues = {
                "brats", "cat", "dog", "dogs", "rat",
        };

            int[] outputValues = { 1, 3, 5, 7, 11 };

            using (Builder builder = new Builder())
            {
                builder.Build(inputValues, outputValues);

                for (int i = 0; i < inputValues.Length; i++)
                {
                    Assert.AreEqual(outputValues[i], builder.Transduce(inputValues[i]));
                }

                using (Compiler compiledFST = builder.GetCompiler())
                {
                    NLPJDict.Kuromoji.Core.FST.FST fst = new NLPJDict.Kuromoji.Core.FST.FST(compiledFST.GetBytes());

                    Assert.AreEqual(0, fst.Lookup("brat")); // Prefix match
                    Assert.AreEqual(1, fst.Lookup("brats"));
                    Assert.AreEqual(3, fst.Lookup("cat"));
                    Assert.AreEqual(5, fst.Lookup("dog"));
                    Assert.AreEqual(7, fst.Lookup("dogs"));
                    Assert.AreEqual(11, fst.Lookup("rat"));
                    Assert.AreEqual(-1, fst.Lookup("rats")); // No match
                }
            }
        }
    }
}
