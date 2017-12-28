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
using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.FST;
using NLPJDict.Kuromoji.Core.util;

namespace NLPJDictTest.kuromoji.Core.Utils
{
    [TestClass]
    public class DictionaryEntryLineParserTest
    {
        [TestMethod]
        public void TestTrivial()
        {
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[]{
                "日本経済新聞", "日本 経済 新聞", "ニホン ケイザイ シンブン", "カスタム名詞"
                },
                DictionaryEntryLineParser.ParseLine("日本経済新聞,日本 経済 新聞,ニホン ケイザイ シンブン,カスタム名詞")
            ));
        }

        [TestMethod]
        public void TestQuotes()
        {
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new String[]{
                "Java Platform, Standard Edition",
                "Java Platform, Standard Edition",
                "Java Platform, Standard Edition",
                "カスタム名詞"
                },
                DictionaryEntryLineParser.ParseLine(
                    "\"Java Platform, Standard Edition\",\"Java Platform, Standard Edition\",\"Java Platform, Standard Edition\",カスタム名詞"
                )
            ));
        }

        [TestMethod]
        public void TtestQuotedQuotes()
        {
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new String[]{
                "Java \"Platform\"",
                "Java \"Platform\"",
                "Java \"Platform\"",
                "カスタム名詞"
                },
                DictionaryEntryLineParser.ParseLine(
                    "\"Java \"\"Platform\"\"\",\"Java \"\"Platform\"\"\",\"Java \"\"Platform\"\"\",カスタム名詞"
                )
            ));
        }

        [TestMethod]
        public void TestEmptyQuotedQuotes()
        {
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new String[]{
                "\"",
                "\"",
                "quote",
                "punctuation"
                },
                DictionaryEntryLineParser.ParseLine(
                    "\"\"\"\",\"\"\"\",quote,punctuation"
                )
            ));
        }

        [TestMethod]
        public void TestCSharp()
        {
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new String[]{
                "C#",
                "C #",
                "シーシャープ",
                "プログラミング言語"
                },
                DictionaryEntryLineParser.ParseLine(
                    "\"C#\",\"C #\",シーシャープ,プログラミング言語"
                )
            ));
        }

        [TestMethod]
        public void TestTab()
        {
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new String[]{
                "A\tB",
                "A B",
                "A B",
                "tab"
                },
                DictionaryEntryLineParser.ParseLine(
                    "A\tB,A B,A B,tab"
                )
            ));
        }

        [TestMethod]
        public void TestFrancoisWhiteBuffaloBota()
        {

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new String[]{
                "フランソワ\"ザホワイトバッファロー\"ボタ",
                "フランソワ\"ザホワイトバッファロー\"ボタ",
                "フランソワ\"ザホワイトバッファロー\"ボタ",
                "名詞"
                },
                DictionaryEntryLineParser.ParseLine(
                    "\"フランソワ\"\"ザホワイトバッファロー\"\"ボタ\",\"フランソワ\"\"ザホワイトバッファロー\"\"ボタ\",\"フランソワ\"\"ザホワイトバッファロー\"\"ボタ\",名詞"
                )
            ));
        }

        [TestMethod]
        public void TestSingleQuote()
        {
            try
            {
                DictionaryEntryLineParser.ParseLine("this is an entry with \"unmatched quote");
                Assert.Fail();
            }
            catch
            {

            }
        }

        [TestMethod]
        public void testUnmatchedQuote()
        {
            try
            {
                DictionaryEntryLineParser.ParseLine("this is an entry with \"\"\"unmatched quote");
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void TestEscapeRoundTrip()
        {
            string original = "3,\"14";

            Assert.AreEqual("\"3,\"\"14\"", DictionaryEntryLineParser.Escape(original));
            Assert.AreEqual(original,
                DictionaryEntryLineParser.Unescape(DictionaryEntryLineParser.Escape(original))
            );
        }

        [TestMethod]
        public void TestUnescape()
        {
            Assert.AreEqual("A", DictionaryEntryLineParser.Unescape("\"A\""));
            Assert.AreEqual("\"A\"", DictionaryEntryLineParser.Unescape("\"\"\"A\"\"\""));

            Assert.AreEqual("\"", DictionaryEntryLineParser.Unescape("\"\"\"\""));
            Assert.AreEqual("\"\"", DictionaryEntryLineParser.Unescape("\"\"\"\"\"\""));
            Assert.AreEqual("\"\"\"", DictionaryEntryLineParser.Unescape("\"\"\"\"\"\"\"\""));
            Assert.AreEqual("\"\"\"\"\"", DictionaryEntryLineParser.Unescape("\"\"\"\"\"\"\"\"\"\"\"\""));
        }

        // TODO: these tests should be checked, right now they are documenting what is happening.
        [TestMethod]
        public void TestParseInputString()
        {
            string input = "日本経済新聞,1292,1292,4980,名詞,固有名詞,組織,*,*,*,日本経済新聞,ニホンケイザイシンブン,ニホンケイザイシンブン";
            string expected = (new String[]{"日本経済新聞", "1292", "1292", "4980",
            "名詞", "固有名詞", "組織", "*", "*", "*", "日本経済新聞", "ニホンケイザイシンブン", "ニホンケイザイシンブン"}.Array2String());
            Assert.AreEqual(expected, Given(input));
        }

        [TestMethod]
        public void TestParseInputStringWithQuotes()
        {
            string input = "日本経済新聞,1292,1292,4980,名詞,固有名詞,組織,*,*,\"1,0\",日本経済新聞,ニホンケイザイシンブン,ニホンケイザイシンブン";
            string expected = new string[]{"日本経済新聞", "1292", "1292", "4980",
            "名詞", "固有名詞", "組織", "*", "*", "1,0", "日本経済新聞", "ニホンケイザイシンブン", "ニホンケイザイシンブン"}.Array2String();
            Assert.AreEqual(expected, Given(input));
        }

        [TestMethod]
        public void TestQuoteEscape()
        {
            String input = "日本経済新聞,1292,1292,4980,名詞,固有名詞,組織,*,*,\"1,0\",日本経済新聞,ニホンケイザイシンブン,ニホンケイザイシンブン";
            String expected = "\"日本経済新聞,1292,1292,4980,名詞,固有名詞,組織,*,*,\"\"1,0\"\",日本経済新聞,ニホンケイザイシンブン,ニホンケイザイシンブン\"";
            Assert.AreEqual(expected, DictionaryEntryLineParser.Escape(input));
        }

        private string Given(string input)
        {
            return DictionaryEntryLineParser.ParseLine(input).Array2String();
        }
    }
}
