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
    public class BitsFormatterTest
    {
        BitsFormatter formatter = new BitsFormatter();
        
        [TestMethod]
        public void TestFormatCompiled()
        {
            string[] inputValues = { "cat", "cats", "rats" };
            int[] outputValues = { 10, 20, 30 };

            using (Builder builder = new Builder())
            {
                builder.Build(inputValues, outputValues);

                using (Compiler compiledFST = builder.GetCompiler())
                {

                    string expected = "" +
                        "  50: MATCH\n" +
                        "  47:\tr -> 30\t(JMP: 39)\n" +
                        "  43:\tc -> 10\t(JMP: 21)\n" +
                        "  39: MATCH\n" +
                        "  36:\ta -> 0\t(JMP: 33)\n" +
                        "  33: MATCH\n" +
                        "  30:\tt -> 0\t(JMP: 27)\n" +
                        "  27: MATCH\n" +
                        "  24:\ts -> 0\t(JMP: 2)\n" +
                        "  21: MATCH\n" +
                        "  18:\ta -> 0\t(JMP: 15)\n" +
                        "  15: MATCH\n" +
                        "  12:\tt -> 0\t(JMP: 9)\n" +
                        "   9: ACCEPT\n" +
                        "   6:\ts -> 10\t(JMP: 2)\n" +
                        "   2: ACCEPT\n";
                    string actual = formatter.Format(compiledFST.GetBytes());
                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
