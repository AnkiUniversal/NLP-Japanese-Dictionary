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
    public class WordListReducerTest
    {
        private static Tokenizer tokenizer;
        private static readonly string RESOURCE = "./NLPJDictCore/Resource/";
        private static Database dictionary;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            tokenizer = new Tokenizer(Locations.ABS_DICT_COMPILED_PATH);
            dictionary = new Database(Locations.ABS_DICT_CONVERT_PATH + "JapEngDict.db");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            tokenizer.Dispose();
            dictionary.Dispose();
        }

        [TestMethod]
        public void TestHiraganaOnlyWord()
        {
            var tokens = tokenizer.Tokenize("そうかもしれない");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual(3, words.Count);
            Assert.AreEqual("そう", words[0].Surface);
            Assert.AreEqual("かも", words[1].Surface);
            Assert.AreEqual("しれない", words[2].Surface);

            Assert.AreEqual("ソウ", words[0].Reading);
            Assert.AreEqual("カモ", words[1].Reading);
            Assert.AreEqual("シレナイ", words[2].Reading);

            Assert.AreEqual("ソー", words[0].Pronunciation);
            Assert.AreEqual("カモ", words[1].Pronunciation);
            Assert.AreEqual("シレナイ", words[2].Pronunciation);
        }

        [TestMethod]
        public void TestSpecialChar()
        {
            var tokens = tokenizer.Tokenize("そこで|私たち*を待って&いる^。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual(9, words.Count);
            Assert.AreEqual("そこで", words[0].Surface);
            Assert.AreEqual("|", words[1].Surface);
            Assert.AreEqual("私たち", words[2].Surface);
            Assert.AreEqual("*", words[3].Surface);
            Assert.AreEqual("を", words[4].Surface);
            Assert.AreEqual("待って", words[5].Surface);
            Assert.AreEqual("&", words[6].Surface);
            Assert.AreEqual("いる", words[7].Surface);
            Assert.AreEqual("^。", words[8].Surface);
        }

        [TestMethod]
        public void TestNiWaInComplete()
        {
            var tokens = tokenizer.Tokenize("そこに");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual(2, words.Count);
            Assert.AreEqual("そこ", words[0].Surface);
            Assert.AreEqual("に", words[1].Surface);

            Assert.AreEqual("ソコ", words[0].Reading);
            Assert.AreEqual("ニ", words[1].Reading);

            Assert.AreEqual("ソコ", words[0].Pronunciation);
            Assert.AreEqual("ニ", words[1].Pronunciation);
        }

        [TestMethod]
        public void TestFullAndPartialWordMatchInDictionary()
        {
            var tokens = tokenizer.Tokenize("だいがく");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual(1, words.Count);
            Assert.AreEqual("だいがく", words[0].Surface);
            Assert.AreEqual("ダイガク", words[0].Reading);
            Assert.AreEqual("ダイガク", words[0].Pronunciation);
            Assert.IsTrue(words[0].IsInDictionary);

            tokens = tokenizer.Tokenize("だいが");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual(2, words.Count);

            Assert.AreEqual("だい", words[0].Surface);
            Assert.AreEqual("が", words[1].Surface);

            Assert.AreEqual("ダイ", words[0].Reading);
            Assert.AreEqual("ガ", words[1].Reading);

            Assert.AreEqual("ダイ", words[0].Pronunciation);
            Assert.AreEqual("ガ", words[1].Pronunciation);
        }

        [TestMethod]
        public void TestSimplePhrase()
        {
            var tokens = tokenizer.Tokenize("そこで私たちを待っている幸福が、私たちが望むような幸福ではないかもしれない。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual("そこで", words[0].Surface);
            Assert.AreEqual("私たち", words[1].Surface);
            Assert.AreEqual("を", words[2].Surface);
            Assert.AreEqual("待って", words[3].Surface);
            Assert.AreEqual("いる", words[4].Surface);
            Assert.AreEqual("幸福", words[5].Surface);
            Assert.AreEqual("が", words[6].Surface);
            Assert.AreEqual("、", words[7].Surface);
            Assert.AreEqual("私たち", words[8].Surface);
            Assert.AreEqual("が", words[9].Surface);
            Assert.AreEqual("望む", words[10].Surface);
            Assert.AreEqual("ような", words[11].Surface);
            Assert.AreEqual("幸福", words[12].Surface);
            Assert.AreEqual("ではない", words[13].Surface);
            Assert.AreEqual("かも", words[14].Surface);
            Assert.AreEqual("しれない", words[15].Surface);
            Assert.AreEqual("。", words[16].Surface);

            Assert.AreEqual("ソコデ", words[0].Reading);
            Assert.AreEqual("ワタシタチ", words[1].Reading);
            Assert.AreEqual("ヲ", words[2].Reading);
            Assert.AreEqual("マッテ", words[3].Reading);
            Assert.AreEqual("イル", words[4].Reading);
            Assert.AreEqual("コウフク", words[5].Reading);
            Assert.AreEqual("ガ", words[6].Reading);
            Assert.AreEqual("、", words[7].Reading);
            Assert.AreEqual("ワタシタチ", words[8].Reading);
            Assert.AreEqual("ガ", words[9].Reading);
            Assert.AreEqual("ノゾム", words[10].Reading);
            Assert.AreEqual("ヨウナ", words[11].Reading);
            Assert.AreEqual("コウフク", words[12].Reading);
            Assert.AreEqual("デハナイ", words[13].Reading);
            Assert.AreEqual("カモ", words[14].Reading);
            Assert.AreEqual("シレナイ", words[15].Reading);
            Assert.AreEqual("。", words[16].Reading);

            Assert.AreEqual("ソコデ", words[0].Pronunciation);
            Assert.AreEqual("ワタシタチ", words[1].Pronunciation);
            Assert.AreEqual("ヲ", words[2].Pronunciation);
            Assert.AreEqual("マッテ", words[3].Pronunciation);
            Assert.AreEqual("イル", words[4].Pronunciation);
            Assert.AreEqual("コーフク", words[5].Pronunciation);
            Assert.AreEqual("ガ", words[6].Pronunciation);
            Assert.AreEqual("、", words[7].Pronunciation);
            Assert.AreEqual("ワタシタチ", words[8].Pronunciation);
            Assert.AreEqual("ガ", words[9].Pronunciation);
            Assert.AreEqual("ノゾム", words[10].Pronunciation);
            Assert.AreEqual("ヨーナ", words[11].Pronunciation);
            Assert.AreEqual("コーフク", words[12].Pronunciation);
            Assert.AreEqual("デワナイ", words[13].Pronunciation);
            Assert.AreEqual("カモ", words[14].Pronunciation); 
            Assert.AreEqual("シレナイ", words[15].Pronunciation);
            Assert.AreEqual("。", words[16].Pronunciation);
        }

        [TestMethod]
        public void TestParticle()
        {
            var tokens = tokenizer.Tokenize("ここら辺には見るべきところがたくさんあります。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            words = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual("ここら辺", words[0].Surface);
            Assert.AreEqual("に", words[1].Surface);
            Assert.AreEqual("は", words[2].Surface);
            Assert.AreEqual("見る", words[3].Surface);
            Assert.AreEqual("べき", words[4].Surface);
            Assert.AreEqual("ところ", words[5].Surface);
            Assert.AreEqual("が", words[6].Surface);
            Assert.AreEqual("たくさん", words[7].Surface);
            Assert.AreEqual("あります", words[8].Surface);
            Assert.AreEqual("。", words[9].Surface);

            Assert.AreEqual("ココラヘン", words[0].Reading);
            Assert.AreEqual("ニ", words[1].Reading);
            Assert.AreEqual("ハ", words[2].Reading);
            Assert.AreEqual("ミル", words[3].Reading);
            Assert.AreEqual("ベキ", words[4].Reading);
            Assert.AreEqual("トコロ", words[5].Reading);
            Assert.AreEqual("ガ", words[6].Reading);
            Assert.AreEqual("タクサン", words[7].Reading);
            Assert.AreEqual("アリマス", words[8].Reading);
            Assert.AreEqual("。", words[9].Reading);

            Assert.AreEqual("ココラヘン", words[0].Pronunciation);
            Assert.AreEqual("ニ", words[1].Pronunciation);
            Assert.AreEqual("ワ", words[2].Pronunciation);
            Assert.AreEqual("ミル", words[3].Pronunciation);
            Assert.AreEqual("ベキ", words[4].Pronunciation);
            Assert.AreEqual("トコロ", words[5].Pronunciation);
            Assert.AreEqual("ガ", words[6].Pronunciation);
            Assert.AreEqual("タクサン", words[7].Pronunciation);
            Assert.AreEqual("アリマス", words[8].Pronunciation);
            Assert.AreEqual("。", words[9].Pronunciation);
        }

        [TestMethod]
        public void TestAllSampleSentences()
        {
            using (var examples = File.OpenRead(RESOURCE + "/TestReducer.csv"))
            using(var reader = new StreamReader(examples))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    var split = line.Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    var tokens = tokenizer.Tokenize(split[0]);
                    var actualWords = WordConjungateCombiner.Combine(tokens, dictionary);
                    WordListReducer reducer = new WordListReducer();
                    actualWords = reducer.ReduceAll(actualWords, dictionary);

                    var expectedWords = split[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < expectedWords.Length; i++)
                    {
                        Assert.AreEqual(expectedWords[i], actualWords[i].Surface);
                    }
                }
            }
        }

        [TestMethod]
        public void TestReduceOnce()
        {
            var tokens = tokenizer.Tokenize("そこで私たちを待っている幸福が、私たちが望むような幸福ではないかもしれない。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            int index = words.FindIndex(0, words.Count, (x) => { return x.Surface.Contains("かも"); });
            WordListReducer reducer = new WordListReducer();
            var newWord = reducer.ReduceOnce(index, words, dictionary, true);

            Assert.AreEqual("かもしれない", newWord.Surface);
            Assert.AreEqual("カモシレナイ", newWord.Pronunciation);
            Assert.AreEqual("カモシレナイ", newWord.Reading);
        }

        [TestMethod]
        public void TestReduceWithBaseForm()
        {
            var tokens = tokenizer.Tokenize("始めてみることにした");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(5, words.Count);
            int index = words.FindIndex(0, words.Count, (x) => { return x.Surface.Contains("こと"); });
            WordListReducer reducer = new WordListReducer();
            var newWord = reducer.ReduceOnce(index, words, dictionary, true);
            Assert.AreEqual("ことにした", newWord.Surface);
            Assert.AreEqual("コトニシタ", newWord.Pronunciation);
            Assert.AreEqual("コトニシタ", newWord.Reading);
            Assert.AreEqual("ことにする", newWord.BaseForm);

            tokens = tokenizer.Tokenize("というものであった");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(3, words.Count);
            index = words.FindIndex(0, words.Count, (x) => { return x.Surface.Contains("もの"); });
            reducer = new WordListReducer();
            newWord = reducer.ReduceOnce(index, words, dictionary, true);
            Assert.AreEqual("ものであった", newWord.Surface);
            Assert.AreEqual("モノデアッタ", newWord.Pronunciation);
            Assert.AreEqual("モノデアッタ", newWord.Reading);
            Assert.AreEqual("ものである", newWord.BaseForm);
        }

        [TestMethod]
        public void TestReduceUnkownWord()
        {
            var tokens = tokenizer.Tokenize("洋ゲーで日本語ローカライズされてない");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            var newWords = reducer.ReduceAll(words, dictionary);

            Assert.IsTrue(newWords[0].IsInDictionary);
            Assert.IsTrue(!newWords[0].IsUnknownWord);
            Assert.AreEqual("洋ゲー", newWords[0].Surface);
            Assert.AreEqual("ヨーゲー", newWords[0].Pronunciation);
            Assert.AreEqual("ヨウゲー", newWords[0].Reading);

            Assert.IsTrue(newWords[3].IsInDictionary);
            Assert.IsTrue(!newWords[3].IsUnknownWord);
        }

        [TestMethod]
        public void TestWrongReadingAfterReduce()
        {
            var tokens = tokenizer.Tokenize("今日は一歩も");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            var newWords = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual("一歩", newWords[2].Surface);
            Assert.AreEqual("イッポ", newWords[2].Pronunciation);
            Assert.AreEqual("イッポ", newWords[2].Reading);
        }

        [TestMethod]
        public void TestIfRemoveWrongEndIndex()
        {
            var tokens = tokenizer.Tokenize("私だからな");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            var newWord = reducer.ReduceOnce(1, words, dictionary, true);
            Assert.AreEqual("だから", newWord.Surface);
            Assert.AreEqual("ダカラ", newWord.Pronunciation);
            Assert.AreEqual("ダカラ", newWord.Reading);
        }

        [TestMethod]
        public void TestLinkGroupCount()
        {
            var tokens = tokenizer.Tokenize("そわそわしだしたア");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            var newWords = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual("し", newWords[1].Surface);
            Assert.AreEqual("だした", newWords[2].Surface);
            Assert.AreEqual(1, newWords[1].LinkWordGroup);
            Assert.AreEqual(1, newWords[2].LinkWordGroup);
        }

        [TestMethod]
        public void TestNotValidVerb()
        {
            var tokens = tokenizer.Tokenize("依代とした");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            WordListReducer reducer = new WordListReducer();
            var newWords = reducer.ReduceAll(words, dictionary);
            Assert.AreEqual(3, newWords.Count);
            Assert.AreEqual("依代", newWords[0].Surface);
            Assert.AreEqual(0, newWords[0].LinkWordGroup);
        }
    }
}
