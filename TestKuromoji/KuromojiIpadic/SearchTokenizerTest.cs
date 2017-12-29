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
using NLPJapaneseDictionary.Kuromoji.Core;
using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core.Buffer;
using NLPJapaneseDictionary.Kuromoji.Core.FST;
using NLPJapaneseDictionary.Kuromoji.Core.util;
using NLPJapaneseDictionary.KuromojiIpadic.Ipadic;
using System.Text.RegularExpressions;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class SearchTokenizerTest
    {
        private static Tokenizer tokenizer;

        [TestCleanup]
        public void Clean()
        {
            tokenizer.Dispose();
        }

        [TestInitialize]
        public void BeforeClass()
        {
            var builder = new Tokenizer.Builder(TestUtils.AbsoluteIpadicResourcePath);
            builder.Mode = Mode.SEARCH;
            tokenizer = new Tokenizer(builder);
        }

        [TestMethod]
        public void TestCompoundSplitting()
        {
            AssertSegmentation("search-segmentation-tests.txt");
        }

        public void AssertSegmentation(string testFilename)
        {
            using (var stream = File.OpenRead(TestUtils.AbsoluteIpadicResourcePath + testFilename))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    // Remove comments
                    string line = Regex.Replace(reader.ReadLine(), "#.*$", "");
                    // Skip empty lines or comment lines
                    if (String.IsNullOrWhiteSpace(line.Trim()))
                    {
                        continue;
                    }

                    string[] fields = line.Split(new string[] { "\t" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    string text = fields[0];
                    List<string> expectedSurfaces = fields[1].SplitSpace().ToList();

                    AssertSegmentation(text, expectedSurfaces);
                }
            }
        }

        public void AssertSegmentation(String text, List<String> expectedSurfaces)
        {
            List<Token> tokens = tokenizer.Tokenize(text);

            Assert.AreEqual(expectedSurfaces.Count, tokens.Count, "Input: " + text);

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedSurfaces[i], tokens[i].Surface);
            }
        }

        private Stream GetResourceAsStream(string fileName)
        {
            return File.OpenRead(TestUtils.AbsoluteIpadicResourcePath + fileName);
        }
    }
}
