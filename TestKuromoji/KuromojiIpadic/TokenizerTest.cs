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
using System.Linq;
using NLPJDictTest.kuromoji.Core;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class TokenizerTest
    {

        private static Tokenizer tokenizer;

        [ClassCleanup]
        public static void Clean()
        {
            tokenizer.Dispose();
        }

        [TestInitialize]
        public void SetUpBeforeClass()
        {
            tokenizer = new Tokenizer(TestUtils.DictResourcedPath);
        }

        [TestMethod]
        public void TestSimpleSegmentation()
        {
            string input = "スペースステーションに行きます。うたがわしい。";
            string[] surfaces = { "スペース", "ステーション", "に", "行き", "ます", "。", "うたがわしい", "。" };
            TestUtils.AssertTokenSurfacesEquals(surfaces, tokenizer.Tokenize(input).ToArray());
        }

        [TestMethod]
        public void TestSimpleMultiTokenization()
        {
            string input = "スペースステーションに行きます。うたがわしい。";
            var tokenLists = tokenizer.MultiTokenize(input, 20, 100000);

            Assert.AreEqual(20, tokenLists.Count);

            foreach (var tokens in tokenLists)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Token token in tokens)
                {
                    sb.Append(token.Surface);
                }
                Assert.AreEqual(input, sb.ToString());
            }

            string[] surfaces = { "スペース", "ステーション", "に", "行き", "ます", "。", "うたがわしい", "。" };

            TestUtils.AssertTokenSurfacesEquals<Token>(surfaces, tokenLists[0].ToArray());
        }

        [TestMethod]
        public void TestMergerCornerCase()
        {
            string input = "難しい。。。";
            var tokenLists = tokenizer.MultiTokenize(input, 2, 100000);
            string[] surfaces = { "難しい", "。", "。", "。" };

            TestUtils.AssertTokenSurfacesEquals<Token>(surfaces, tokenLists[0].ToArray());
        }

        [TestMethod]
        public void TestMultiTokenizationFindsAll()
        {
            string input = "スペースステーション";
            var tokenLists = tokenizer.MultiTokenizeNBest(input, 100);
            Assert.AreEqual(9, tokenLists.Count);
        }

        [TestMethod]
        public void TestMultiNoOverflow()
        {
            string input = "バスできた。";
            var tokenLists = tokenizer.MultiTokenizeBySlack(input, int.MaxValue);
            Assert.AreNotEqual(0, tokenLists.Count);
        }

        [TestMethod]
        public void TestMultiEmptyString()
        {
            string input = "";
            var tokenLists = tokenizer.MultiTokenize(input, 10, int.MaxValue);
            Assert.AreEqual(1, tokenLists.Count);
        }

        [TestMethod]
        public void TestSimpleReadings()
        {
            var tokens = tokenizer.Tokenize("寿司が食べたいです。");
            Assert.IsTrue(tokens.Count == 6);
            Assert.AreEqual(tokens[0].GetReading(), "スシ");
            Assert.AreEqual(tokens[1].GetReading(), "ガ");
            Assert.AreEqual(tokens[2].GetReading(), "タベ");
            Assert.AreEqual(tokens[3].GetReading(), "タイ");
            Assert.AreEqual(tokens[4].GetReading(), "デス");
            Assert.AreEqual(tokens[5].GetReading(), "。");
        }

        [TestMethod]
        public void TestSimpleReading()
        {
            var tokens = tokenizer.Tokenize("郵税");
            Assert.AreEqual(tokens[0].GetReading(), "ユウゼイ");
        }

        [TestMethod]
        public void TestSimpleBaseFormKnownWord()
        {
            var tokens = tokenizer.Tokenize("お寿司が食べたい。");
            Assert.IsTrue(tokens.Count == 6);
            Assert.AreEqual("食べ", tokens[3].Surface);
            Assert.AreEqual("食べる", tokens[3].GetBaseForm());

        }

        [TestMethod]
        public void TestSimpleBaseFormUnknownWord()
        {
            var tokens = tokenizer.Tokenize("アティリカ株式会社");
            Assert.IsTrue(tokens.Count == 2);
            Assert.IsFalse(tokens[0].IsKnown);
            Assert.AreEqual("*", tokens[0].GetBaseForm());
            Assert.IsTrue(tokens[1].IsKnown);
            Assert.AreEqual("株式会社", tokens[1].GetBaseForm());
        }

        [TestMethod]
        public void TestYabottaiCornerCase()
        {
            var tokens = tokenizer.Tokenize("やぼったい");
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("やぼったい", tokens[0].Surface);
        }

        [TestMethod]
        public void TestTsukitoshaCornerCase()
        {
            var tokens = tokenizer.Tokenize("突き通しゃ");
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual("突き通しゃ", tokens[0].Surface);
        }

        [TestMethod]
        public void TestIpadicTokenAPIs()
        {
            var tokens = tokenizer.Tokenize("お寿司が食べたい！");
            string[] pronunciations = { "オ", "スシ", "ガ", "タベ", "タイ", "！" };

            Assert.AreEqual(pronunciations.Length, tokens.Count);

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(pronunciations[i], tokens[i].GetPronunciation());
            }

            string[] conjugationForms = { "*", "*", "*", "連用形", "基本形", "*" };

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(conjugationForms[i], tokens[i].GetConjugationForm());
            }

            string[] conjugationTypes = { "*", "*", "*", "一段", "特殊・タイ", "*" };

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(conjugationTypes[i], tokens[i].GetConjugationType());
            }

            string[] posLevel1 = { "接頭詞", "名詞", "助詞", "動詞", "助動詞", "記号" };

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(posLevel1[i], tokens[i].GetPartOfSpeechLevel1());
            }

            string[] posLevel2 = { "名詞接続", "一般", "格助詞", "自立", "*", "一般" };

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(posLevel2[i], tokens[i].GetPartOfSpeechLevel2());
            }

            string[] posLevel3 = { "*", "*", "一般", "*", "*", "*" };

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(posLevel3[i], tokens[i].GetPartOfSpeechLevel3());
            }

            string[] posLevel4 = { "*", "*", "*", "*", "*", "*" };

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(posLevel4[i], tokens[i].GetPartOfSpeechLevel4());
            }
        }

        [TestMethod]
        public void TestCustomPenalties()
        {
            string input = "シニアソフトウェアエンジニアを探しています";

            using (var builder = new Tokenizer.Builder(TestUtils.DictResourcedPath))
            {
                builder.Mode = Mode.SEARCH;
                builder.SetKanjiPenalty(3, 10000);
                builder.SetOtherPenalty(int.MaxValue, 0);
                using (Tokenizer customTokenizer = new Tokenizer(builder))
                {

                    string[] expected1 = { "シニアソフトウェアエンジニア", "を", "探し", "て", "い", "ます" };

                    TestUtils.AssertTokenSurfacesEquals(expected1, customTokenizer.Tokenize(input).ToArray());

                    using (var searchBuilder = new Tokenizer.Builder(TestUtils.DictResourcedPath))
                    {

                        searchBuilder.Mode = Mode.SEARCH;
                        using (Tokenizer searchTokenizer = new Tokenizer(searchBuilder))
                        {

                            string[] expected2 = { "シニア", "ソフトウェア", "エンジニア", "を", "探し", "て", "い", "ます" };

                            TestUtils.AssertTokenSurfacesEquals(expected2, searchTokenizer.Tokenize(input).ToArray());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestNakaguroSplit()
        {
            using (Tokenizer defaultTokenizer = new Tokenizer(TestUtils.DictResourcedPath))
            {

                using (var builder = new Tokenizer.Builder(TestUtils.DictResourcedPath))
                {
                    builder.IsSplitOnNakaguro = true;
                    using (Tokenizer nakakuroSplittingTokenizer = new Tokenizer(builder))
                    {

                        string input = "ラレ・プールカリムの音楽が好き。";

                        TestUtils.AssertTokenSurfacesEquals(
                        new string[] { "ラレ・プールカリム", "の", "音楽", "が", "好き", "。" },
                        defaultTokenizer.Tokenize(input).ToArray());
                        TestUtils.AssertTokenSurfacesEquals(
                        new string[] { "ラレ", "・", "プールカリム", "の", "音楽", "が", "好き", "。" },
                        nakakuroSplittingTokenizer.Tokenize(input).ToArray());
                    }
                }
            }
        }

        [TestMethod]
        public void TestAllFeatures()
        {
            using (Tokenizer tokenizer = new Tokenizer(TestUtils.DictResourcedPath))
            {
                string input = "寿司が食べたいです。";

                List<Token> tokens = tokenizer.Tokenize(input);
                Assert.AreEqual("寿司\t名詞,一般,*,*,*,*,寿司,スシ,スシ", toString(tokens[0]));
                Assert.AreEqual("が\t助詞,格助詞,一般,*,*,*,が,ガ,ガ", toString(tokens[1]));
                Assert.AreEqual("食べ\t動詞,自立,*,*,一段,連用形,食べる,タベ,タベ", toString(tokens[2]));
                Assert.AreEqual("たい\t助動詞,*,*,*,特殊・タイ,基本形,たい,タイ,タイ", toString(tokens[3]));
                Assert.AreEqual("です\t助動詞,*,*,*,特殊・デス,基本形,です,デス,デス", toString(tokens[4]));
            }
        }

        private String toString(Token token)
        {
            return token.Surface + "\t" + token.GetAllFeatures();
        }

        [TestMethod]
        public void TestCompactedTrieCrash()
        {
            string input = "＼ｍ";
            using (Tokenizer tokenizer = new Tokenizer(TestUtils.DictResourcedPath))
            {

                TestUtils.AssertTokenSurfacesEquals(
                    new string[] { "＼", "ｍ" },
                    tokenizer.Tokenize(input).ToArray());
            }
        }

        [TestMethod]
        public void TestFeatureLengths()
        {
            string userDictionary = "" +
                "gsf,gsf,ジーエスーエフ,カスタム名詞\n";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(userDictionary)))
            {
                var builder = new Tokenizer.Builder(TestUtils.DictResourcedPath);
                builder.LoadUserDictionary(stream);
                using (Tokenizer tokenizer = new Tokenizer(builder))
                    TestUtils.AssertEqualTokenFeatureLengths("ahgsfdajhgsfdこの丘はアクロポリスと呼ばれている。", tokenizer);
            }
        }

        [TestMethod]
        public void TestNewBocchan()
        {
            using (var featureStream = File.OpenRead(TestUtils.AbsoluteIpadicResourcePath + "bocchan-ipadic-features.txt"))
            using (var stream = File.OpenRead(TestUtils.AbsoluteIpadicResourcePath + "bocchan.txt"))
            {
                TestUtils.AssertTokenizedStreamEquals(featureStream, stream, tokenizer);
            }
        }

        [TestMethod]
        public void TestPunctuation()
        {
            CommonCornerCasesTest.TestPunctuation(new Tokenizer(TestUtils.DictResourcedPath));
        }
    }
}
