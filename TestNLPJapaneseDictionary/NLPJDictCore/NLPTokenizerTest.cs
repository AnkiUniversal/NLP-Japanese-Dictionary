/**
 * Copyright © 2017-2018 Anki Universal Team.
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
using NLPJapaneseDictionary.ConvertClasses;
using NLPJapaneseDictionary.DatabaseTable.NLPJDictCore;
using NLPJapaneseDictionary.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core;
using NLPJapaneseDictionary.KuromojiIpadic.Ipadic;
using NLPJapaneseDictionary.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.NLPJDictCore
{
    [TestClass]
    public class NLPTokenizerTest
    {
        private static Tokenizer tokenizer;
        private static Database dictionary;
        private static NLPTokenizer<Token> NLPTokenizer;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            tokenizer = new Tokenizer(Locations.ABS_DICT_COMPILED_PATH);
            dictionary = new Database(Locations.ABS_DICT_CONVERT_PATH + "JapEngDict.db");
            NLPTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            tokenizer.Dispose();
            dictionary.Dispose();
        }

        [TestMethod]
        public void TestSimplePhrase()
        {
            var words = NLPTokenizer.Tokenize("田中さんはどこへ行きますか？");
            Assert.AreEqual(8, words.Count);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(9, words.Count);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(10, words.Count);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(10, words.Count);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(10, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(10, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(9, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(8, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(8, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(8, words.Count);
        }

        [TestMethod]
        public void TestAllHiraPhrase()
        {
            var words = NLPTokenizer.Tokenize("きょねんはさむかったですね");
            Assert.AreEqual(2, words.Count);            

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("きょねん", words[0].Surface);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("はさむ", words[1].Surface);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(6, words.Count);
            Assert.AreEqual("かっ", words[2].Surface);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("たで", words[3].Surface);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("すね", words[4].Surface);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("すね", words[4].Surface);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(6, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("きょねん", words[0].Surface);
            Assert.AreEqual("は", words[1].Surface);
            Assert.AreEqual("さむかった", words[2].Surface);

            words = NLPTokenizer.UndoTokenizeReducedSentence();
            Assert.AreEqual(2, words.Count);
        }

        [TestMethod]
        public void TestPhraseHaKanji()
        {
            var words = NLPTokenizer.Tokenize("去年はさむかったですね");
            Assert.AreEqual(5, words.Count);

            words = NLPTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(5, words.Count);
            Assert.AreEqual("去年", words[0].Surface);
            Assert.AreEqual("キョネン", words[0].Reading);
            Assert.AreEqual("キョネン", words[0].Pronunciation);

            Assert.AreEqual("はさむ", words[1].Surface);
            Assert.AreEqual("ハサム", words[1].Reading);
            Assert.AreEqual("ハサム", words[1].Pronunciation);
        }
    }
}
