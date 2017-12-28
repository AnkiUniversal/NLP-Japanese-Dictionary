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
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class TestCompiler
    {
        private string inputDir = @"./KuromojiIpadic/IpadicResource";
        private string outputDir = "./IpadicCompiled";

        /// <summary>
        /// Use this method to compile/create Ipadic dict databases (.bin files) for Kuromoji 
        /// </summary>
        [TestMethod, Ignore]
        public void TestDictionaryCompiler()
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
            Directory.CreateDirectory(outputDir);

            NLPJDict.KuromojiIpadic.Compile.DictionaryCompiler.StartCompile(new string[] { inputDir, outputDir, "euc-jp" }, CodePagesEncodingProvider.Instance);
            Assert.IsTrue(true);
        }
    }
}
