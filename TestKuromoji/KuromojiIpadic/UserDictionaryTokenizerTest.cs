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
using System;
using System.Collections.Generic;
using NLPJDict.Kuromoji.Core.Trie;
using System.Text;
using System.Threading.Tasks;
using NLPJDict.KuromojiIpadic.Ipadic;
using System.IO;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class UserDictionaryTokenizerTest
    {
        private string userDictionary = "" +
                                        "クロ,クロ,クロ,カスタム名詞\n" +
                                        "真救世主,真救世主,シンキュウセイシュ,カスタム名詞\n" +
                                        "真救世主伝説,真救世主伝説,シンキュウセイシュデンセツ,カスタム名詞\n" +
                                        "北斗の拳,北斗の拳,ホクトノケン,カスタム名詞";

        [TestMethod]
        public void TestWhitespace()
        {
            string userDictionary = "iPhone4 S,iPhone4 S,iPhone4 S,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {
                string input = "iPhone4 S";

                TestUtils.AssertTokenSurfacesEquals(new string[] { "iPhone4 S" }, tokenizer.Tokenize(input).ToArray());
            }
        }

        [TestMethod]
        public void TestBadlyFormattedEntry()
        {
            try
            {
                String entry = "関西国際空港,関西 国際 空,カンサイ コクサイクウコウ,カスタム名詞";
                MakeTokenizer(entry);
                Assert.Fail();
            }
            catch { }
        }

        [TestMethod]
        public void TestAcropolis()
        {
            string userDictionary = "クロ,クロ,クロ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                string input = "アクロポリス";

                TestUtils.AssertTokenSurfacesEquals(new string[] { "ア", "クロ", "ポリス" }, tokenizer.Tokenize(input).ToArray());
            }
        }

        [TestMethod]
        public void TestAllFeatures()
        {
            string input = "シロクロ";
            string[]
            surfaces = { "シロ", "クロ" };
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {
                List<Token> tokens = tokenizer.Tokenize(input);

                Assert.AreEqual(surfaces.Length, tokens.Count);
                Token token = tokens[1];
                string actual = token.Surface + "\t" + token.GetAllFeatures();
                Assert.AreEqual("クロ\tカスタム名詞,*,*,*,*,*,*,クロ,*", actual);
            }
        }


        [TestMethod]
        public void TestAcropolisInSentence()
        {
            string userDictionary = "クロ,クロ,クロ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                string input = "この丘はアクロポリスと呼ばれている。";

                TestUtils.AssertTokenSurfacesEquals(
                new string[] { "この", "丘", "は", "ア", "クロ", "ポリス", "と", "呼ば", "れ", "て", "いる", "。" },
                tokenizer.Tokenize(input).ToArray());
            }
        }

        [TestMethod]
        public void TestLatticeBrokenAfterUserDictEntry()
        {
            string userDictionary = "クロ,クロ,クロ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                string input = "アクロア";
                string[]
                surfaces = { "ア", "クロ", "ア" };
                string[]
                features = {
                "*,*,*,*,*,*,*,*,*",
                "カスタム名詞,*,*,*,*,*,*,クロ,*",
                "*,*,*,*,*,*,*,*,*"
                };
                List<Token> tokens = tokenizer.Tokenize(input);

                for (int i = 0; i < tokens.Count; i++)
                {
                    Assert.AreEqual(surfaces[i], tokens[i].Surface);
                    Assert.AreEqual(features[i], tokens[i].GetAllFeatures());
                }
            }
        }

        [TestMethod]
        public void TestLatticeBrokenAfterUserDictEntryInSentence()
        {
            string userDictionary = "クロ,クロ,クロ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                string input = "この丘の名前はアクロアだ。";
                string[]
                surfaces = { "この", "丘", "の", "名前", "は", "ア", "クロ", "ア", "だ", "。" };
                string[]
                features = {
                        "連体詞,*,*,*,*,*,この,コノ,コノ",
                            "名詞,一般,*,*,*,*,丘,オカ,オカ",
                            "助詞,連体化,*,*,*,*,の,ノ,ノ",
                            "名詞,一般,*,*,*,*,名前,ナマエ,ナマエ",
                            "助詞,係助詞,*,*,*,*,は,ハ,ワ",
                            "*,*,*,*,*,*,*,*,*",
                            "カスタム名詞,*,*,*,*,*,*,クロ,*",
                            "*,*,*,*,*,*,*,*,*",
                            "助動詞,*,*,*,特殊・ダ,基本形,だ,ダ,ダ",
                            "記号,句点,*,*,*,*,。,。,。"
                        };
                List<Token> tokens = tokenizer.Tokenize(input);

                for (int i = 0; i < tokens.Count; i++)
                {
                    Assert.AreEqual(surfaces[i], tokens[i].Surface);
                    Assert.AreEqual(features[i], tokens[i].GetAllFeatures());
                }
            }
        }

        [TestMethod]
        public void TestShinKyuseishu()
        {
            string userDictionary = "真救世主,真救世主,シンキュウセイシュ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                Assert.AreEqual("シンキュウセイシュ", tokenizer.Tokenize("真救世主伝説")[0].GetReading());
            }
        }

        [TestMethod]
        public void TestShinKyuseishuDensetsu()
        {
            string userDictionary = "真救世主伝説,真救世主伝説,シンキュウセイシュデンセツ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {
                Assert.AreEqual("シンキュウセイシュデンセツ", tokenizer.Tokenize("真救世主伝説")[0].GetReading());
            }
        }

        [TestMethod]
        public void TestCheckDifferentSpelling()
        {
            string input = "北斗の拳は真救世主伝説の名曲である。";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {
                List<Token> tokens = tokenizer.Tokenize(input);
                string[] expectedReadings = { "ホクトノケン", "ハ", "シンキュウセイシュデンセツ", "ノ", "メイキョク", "デ", "アル", "。" };

                for (int i = 0; i < tokens.Count; i++)
                {
                    Assert.AreEqual(expectedReadings[i], tokens[i].GetReading());
                }
            }
        }

        [TestMethod]
        public void TestLongestActualJapaneseWord()
        {
            string userDictionary = "竜宮の乙姫の元結の切り外し,竜宮の乙姫の元結の切り外し,リュウグウノオトヒメノモトユイノキリハズシ,カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                Assert.AreEqual(
                        "リュウグウノオトヒメノモトユイノキリハズシ",
                        tokenizer.Tokenize("竜宮の乙姫の元結の切り外し")[0].GetReading()
                    );
            }
        }

        [TestMethod]
        public void TestLongestMovieTitle()
        {
            string userDictionary = "マルキ・ド・サドの演出のもとにシャラントン精神病院患者たちによって演じられたジャン＝ポール・マラーの迫害と暗殺,"
                    + "マルキ・ド・サドの演出のもとにシャラントン精神病院患者たちによって演じられたジャン＝ポール・マラーの迫害と暗殺,"
                    + "マルキ・ド・サドノエンシュツノモトニシャラントンセイシンビョウインカンジャタチニヨッテエンジラレタジャン＝ポール・マラーノハクガイトアンサツ,"
                    + "カスタム名詞";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                Assert.AreEqual(
                        "マルキ・ド・サドノエンシュツノモトニシャラントンセイシンビョウインカンジャタチニヨッテエンジラレタジャン＝ポール・マラーノハクガイトアンサツ",
                        tokenizer.Tokenize("マルキ・ド・サドの演出のもとにシャラントン精神病院患者たちによって演じられたジャン＝ポール・マラーの迫害と暗殺")[0].GetReading()
                    );
            }
        }

        [TestMethod]
        public void TestOverlappingUserEntries()
        {
            string userDictionary = "クリ,クリ,クリ,カスタム名詞\n" +
                    "チャン,チャン,チャン,カスタム名詞\n" +
                    "リスチャン,リスチャン,リスチャン,カスタム名詞";

            string input = "クリスチャンは寿司が大好きです。";

            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                List<Token> tokens = tokenizer.Tokenize(input);

                TestUtils.AssertTokenSurfacesEquals(
                        new string[] { "ク", "リスチャン", "は", "寿司", "が", "大好き", "です", "。" },
                        tokens.ToArray());
            }
        }

        [TestMethod]
        public void TestShorterEntryMatchWhenUserEntriesOverlap()
        {
            string userDictionary = "関西国際空港,関西国際空港,かんさいこくさいくうこう,カスタム施設\n" +
                    "関西,関西,かんさい,カスタム地名";

            string input = "関西国際医療センター";

            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                List<Token> tokens = tokenizer.Tokenize(input);

                Assert.AreEqual("関西", tokens[0].Surface);
                Assert.AreEqual("カスタム地名", tokens[0].GetPartOfSpeechLevel1());
            }
        }

        [TestMethod]
        public void TestInsertedFail()
        {
            string userDictionary = "引,引,引,カスタム品詞\n";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                TestUtils.AssertTokenSurfacesEquals(
                        new string[] { "引", "く", "。" },
                        tokenizer.Tokenize("引く。").ToArray());
            }
        }

        [TestMethod]
        public void TestFullUserDictionary()
        {
            string userDictionary = "" +
                    "日本経済新聞,日本 経済 新聞,ニホン ケイザイ シンブン,カスタム名詞\n" +
                    "渡部,1290,1290,5900,カスタム名詞,固有名詞,人名,姓,*,*,渡部,ワタナベ,ワタナベ\n";

            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                String input = "渡部さんは日本経済新聞社に勤めている。";
                String[]
                surfaces = { "渡部", "さん", "は", "日本", "経済", "新聞", "社", "に", "勤め", "て", "いる", "。" };
                String[]
                features = {
        "カスタム名詞,固有名詞,人名,姓,*,*,渡部,ワタナベ,ワタナベ",
            "名詞,接尾,人名,*,*,*,さん,サン,サン",
            "助詞,係助詞,*,*,*,*,は,ハ,ワ",
            "カスタム名詞,*,*,*,*,*,*,ニホン,*",
            "カスタム名詞,*,*,*,*,*,*,ケイザイ,*",
            "カスタム名詞,*,*,*,*,*,*,シンブン,*",
            "名詞,一般,*,*,*,*,社,シャ,シャ",
            "助詞,格助詞,一般,*,*,*,に,ニ,ニ",
            "動詞,自立,*,*,一段,連用形,勤める,ツトメ,ツトメ",
            "助詞,接続助詞,*,*,*,*,て,テ,テ",
            "動詞,非自立,*,*,一段,基本形,いる,イル,イル",
            "記号,句点,*,*,*,*,。,。,。"
        };
                List<Token> tokens = tokenizer.Tokenize(input);

                for (int i = 0; i < tokens.Count; i++)
                {
                    Assert.AreEqual(surfaces[i], tokens[i].Surface);
                    Assert.AreEqual(features[i], tokens[i].GetAllFeatures());
                }
            }
        }

        [TestMethod]
        public void TestChauUserDictionaryEntries()
        {            
            string userDictionary = "ちゃい,1000,1000,1000,動詞,*,*,*,五段・ワ行促音便,連用形,ちゃう,チャイ,チャイ";

            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {
                string input = "また太っちゃいますよ。";               
                var tokens = tokenizer.Tokenize(input);
                Assert.AreEqual(6, tokens.Count);
                Assert.AreEqual("ちゃい", tokens[2].Surface);
            }
        }

        [TestMethod]
        public void TestNaraUserDictionaryEntries()
        {
            string userDictionary = "なら,200,200,200,補助,*,*,*,*,*,なら,ナラ,ナラ";

            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {
                string input = "ホームセンターならあるけど。";
                var tokens = tokenizer.Tokenize(input);
                Assert.AreEqual(5, tokens.Count);
                Assert.AreEqual("なら", tokens[1].Surface);
                Assert.AreEqual("なら", tokens[1].BaseForm);
            }
        }

        //Java: @Ignore("Doesn't segment properly - Viterbi lattice looks funny")
        [TestMethod]
        public void TestTsunk()
        {
            string userDictionary = "" +
                    "シャ乱Q つんく♂,シャ乱Q つんく ♂,シャランキュー ツンク ボーイ,カスタムアーティスト名";
            using (Tokenizer tokenizer = MakeTokenizer(userDictionary))
            {

                using (var output = File.Open("./tsunk.gv", FileMode.Create))
                {
                    tokenizer.DebugTokenize(output, "シャQ");
                }
            }
        }

        private Tokenizer MakeTokenizer(string userDictionaryEntry)
        {
            using (var stream = MakeUserDictionaryStream(userDictionaryEntry))
            {
                var builder = new Tokenizer.Builder(TestUtils.AbsoluteIpadicResourcePath);
                builder.LoadUserDictionary(stream);
                return new Tokenizer(builder);
            }
        }

        private MemoryStream MakeUserDictionaryStream(string userDictionary)
        {
            var bytes = Encoding.UTF8.GetBytes(userDictionary);
            return new MemoryStream(bytes);
        }
    }
}
