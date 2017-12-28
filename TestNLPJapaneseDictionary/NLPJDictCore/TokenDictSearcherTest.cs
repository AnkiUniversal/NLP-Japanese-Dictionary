using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.ConvertClasses;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.HelperClasses;
using NLPJDict.Kuromoji.Core;
using NLPJDict.KuromojiIpadic.Ipadic;
using NLPJDict.NLPJDictCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.NLPJDictCore
{
    [TestClass]
    public class TokenDictSearcherTest
    {
        private static Tokenizer tokenizer;
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
        public void TestNonToken()
        {
            string word = "愛";
            var results = TokensDictSearcher.FindJapNonToken(word, dictionary);
            Assert.AreEqual(171, results.Count);

            Assert.AreEqual(word, results[0].RepresentWord);
            Assert.AreEqual(word, results[1].RepresentWord);

            bool isPerfectMatchRepresentWordRegion = true;
            bool isPartialMatchRepresentWordRegion = false;
            for(int i = 1; i < results.Count; i++)
            {
                if(results[i].HighestFrequency < results[i - 1].HighestFrequency)
                {
                    if(isPerfectMatchRepresentWordRegion)
                    {
                        Assert.IsTrue(results[i - 1].RepresentWord.Equals(word, StringComparison.OrdinalIgnoreCase));
                        Assert.IsFalse(results[i].RepresentWord.Equals(word, StringComparison.OrdinalIgnoreCase));
                        isPerfectMatchRepresentWordRegion = false;
                        isPartialMatchRepresentWordRegion = true;
                        continue;
                    }

                    if(isPartialMatchRepresentWordRegion)
                    {
                        Assert.IsTrue(results[i - 1].RepresentWord.Contains(word));
                        Assert.IsFalse(results[i].RepresentWord.Contains(word));
                        Assert.IsTrue(results[i].KanjiElement.Contains(word));
                        continue;
                    }

                    Assert.Fail();
                }
            }

            if (isPerfectMatchRepresentWordRegion)
                Assert.Fail();
        }

        [TestMethod]
        public void TestNonTokenRepresentPrority()
        {
            string word = "なる";
            var results = TokensDictSearcher.FindJapNonToken(word, dictionary);
            Assert.AreEqual(17, results.Count);

            Assert.AreEqual("成る", results[0].RepresentWord);
            Assert.AreEqual("なる", results[3].RepresentWord);          
        }

        [TestMethod]
        public void TestNonTokenWordType()
        {
            string word = "愛";

            TokensDictSearcher.DefaultGetJapMethod = JmdictEntity.GetJapMatchVerb;
            var results = TokensDictSearcher.FindJapNonToken(word, dictionary);
            Assert.AreEqual(45, results.Count);
            CheckIfSenseContain(results, "verb");

            TokensDictSearcher.DefaultGetJapMethod = JmdictEntity.GetJapMatchAdjective;            
            results = TokensDictSearcher.FindJapNonToken(word, dictionary);
            Assert.AreEqual(20, results.Count);
            CheckIfSenseContain(results, "adjective");

            TokensDictSearcher.DefaultGetJapMethod = JmdictEntity.GetJapMatchAdverb;
            results = TokensDictSearcher.FindJapNonToken(word, dictionary);
            Assert.AreEqual(1, results.Count);
            CheckIfSenseContain(results, "adverb");

            TokensDictSearcher.DefaultGetJapMethod = JmdictEntity.GetJapMatchNoun;
            results = TokensDictSearcher.FindJapNonToken(word, dictionary);
            Assert.AreEqual(132, results.Count);
            CheckIfSenseContain(results, "noun");

            TokensDictSearcher.DefaultGetJapMethod = JmdictEntity.GetJapMatchAll;
        }

        [TestMethod]
        public void TestWordGlossOrder()
        {
            string word = "cry";

            var results = TokensDictSearcher.FindByGloss(word, dictionary);
            Assert.AreEqual(63, results.Count);

            bool isFirsGlossRegion = true;            
            for (int i = 1; i < results.Count; i++)
            {
                if (results[i].HighestFrequency < results[i - 1].HighestFrequency)
                {
                    if (isFirsGlossRegion)
                    {
                        isFirsGlossRegion = false;
                        continue;
                    }

                    Assert.Fail();
                }
            }

            if(isFirsGlossRegion)
                Assert.Fail();
        }

        [TestMethod]
        public void TestWordTypeGloss()
        {
            string word = "cry";

            TokensDictSearcher.DefaultGetGlossMethod = JmdictEntity.GetByGlossAll;
            var results = TokensDictSearcher.FindByGloss(word, dictionary);
            Assert.AreEqual(63, results.Count);

            TokensDictSearcher.DefaultGetGlossMethod = JmdictEntity.GetByGlossVerb;
            results = TokensDictSearcher.FindByGloss(word, dictionary);
            Assert.AreEqual(34, results.Count);
            CheckIfSenseContain(results, "verb");

            TokensDictSearcher.DefaultGetGlossMethod = JmdictEntity.GetByGlossAdjective;
            results = TokensDictSearcher.FindByGloss(word, dictionary);
            Assert.AreEqual(3, results.Count);
            CheckIfSenseContain(results, "adjective");

            TokensDictSearcher.DefaultGetGlossMethod = JmdictEntity.GetByGlossAdverb;
            results = TokensDictSearcher.FindByGloss(word, dictionary);
            Assert.AreEqual(2, results.Count);
            CheckIfSenseContain(results, "adverb");

            TokensDictSearcher.DefaultGetGlossMethod = JmdictEntity.GetByGlossNoun;
            results = TokensDictSearcher.FindByGloss(word, dictionary);
            Assert.AreEqual(33, results.Count);
            CheckIfSenseContain(results, "noun");

            TokensDictSearcher.DefaultGetGlossMethod = JmdictEntity.GetByGlossAll;
        }

        private static void CheckIfSenseContain(List<JmdictEntity> results,string word)
        {
            foreach (var result in results)
                Assert.IsTrue(result.SenseElement.Contains(word));
        }

        [TestMethod]
        public void TestConvertTokensToWords()
        {
            var tokens = tokenizer.Tokenize("田中さんはどこへ行けますいったか");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.IsTrue(words[5].Surface.Equals("行けます"));
            Assert.IsTrue(words[5].Conjugation.Contains("polite"));
            Assert.IsTrue(words[5].Conjugation.Contains("potential"));
            Assert.IsTrue(words[5].BaseForm.Equals("行く"));
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchNoConjugation()
        {
            WordInformation word = new WordInformation(null, null, null, null);
            word.AddWordPart("お使い", "オツカイ", "オツカイ");
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(word, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("お使い", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchGodanConjugation()
        {
            var tokens = tokenizer.Tokenize("田中さんはどこへ行けますか");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[5], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("行く", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchAmbigousGodanTTConjugation()
        {
            var tokens = tokenizer.Tokenize("田中さんはどこへいった？");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[5], dictionary);
            Assert.AreEqual(7, results.Count);
            Assert.AreEqual("行く", results[0].RepresentWord);
            Assert.AreEqual("言う", results[1].RepresentWord);
            Assert.AreEqual("結う", results[2].RepresentWord);
            Assert.AreEqual("入る", results[3].RepresentWord);
            Assert.AreEqual("煎る", results[4].RepresentWord);
            Assert.AreEqual("要る", results[5].RepresentWord);
            Assert.AreEqual("沒る", results[6].RepresentWord);

            tokens = tokenizer.Tokenize("田中さんはどこへ行った？");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[5], dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("行く", results[0].RepresentWord);
            Assert.AreEqual("行う", results[1].RepresentWord);
            Assert.AreEqual("遣る", results[2].RepresentWord);

            tokens = tokenizer.Tokenize("毎年行っていたらしい");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("行う", results[0].RepresentWord);
            Assert.AreEqual("行く", results[1].RepresentWord);
            Assert.AreEqual("遣る", results[2].RepresentWord);

            tokens = tokenizer.Tokenize("おこなっていたらしい");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[0], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("行う", results[0].RepresentWord);

            tokens = tokenizer.Tokenize("毎年行かなくていたらしい");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(1, results.Count);            
            Assert.AreEqual("行く", results[0].RepresentWord);

            tokens = tokenizer.Tokenize("毎年行わなくていたらしい");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("行う", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchAmbigousGodanNDConjugation()
        {
            var tokens = tokenizer.Tokenize("なにをよんだ");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("呼ぶ", results[0].RepresentWord);
            Assert.AreEqual("読む", results[1].RepresentWord);            
        }

        [TestMethod]
        public void TestFindTokenPerfectReadingReorder()
        {
            var tokens = tokenizer.Tokenize("田中さんはどこに入らせられました？");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[5], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("入る", results[0].RepresentWord);
            Assert.AreEqual("入る", results[1].RepresentWord);
            Assert.IsTrue(results[0].ReadElement.Contains("\"はいる\""));
            Assert.IsTrue(results[1].ReadElement.Contains("\"いる\""));

            tokens = tokenizer.Tokenize("形形");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[0], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("形", results[0].RepresentWord);
            Assert.IsTrue(results[0].ReadElement.Contains(KataHiraConvert.ConvertKataToHira(words[0].Reading)));
            Assert.AreEqual("形", results[1].RepresentWord);
            Assert.IsTrue(results[1].ReadElement.Contains(KataHiraConvert.ConvertKataToHira(words[1].Reading)));
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchIchidanConjugation()
        {
            var tokens = tokenizer.Tokenize("なにをたべられるか？");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("食べる", results[0].RepresentWord);
            Assert.AreEqual("食べられる", results[1].RepresentWord);
            Assert.IsTrue(results[0].Conjugation.Contains("passive or potential"));
            Assert.IsTrue(results[1].Conjugation == null);

            tokens = tokenizer.Tokenize("友達でいてくれて");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("居る", results[0].RepresentWord);
            Assert.AreEqual("射る", results[1].RepresentWord);
            Assert.AreEqual("鋳る", results[2].RepresentWord);
            Assert.AreEqual("癒る", results[3].RepresentWord);
            Assert.AreEqual("射手", results[4].RepresentWord);
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchVerbBaseForm()
        {
            var tokens = tokenizer.Tokenize("ここにいる");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(8, results.Count);
            Assert.AreEqual("居る", results[0].RepresentWord);
            Assert.AreEqual("射る", results[1].RepresentWord);
            Assert.AreEqual("入る", results[2].RepresentWord);
            Assert.AreEqual("煎る", results[3].RepresentWord);
            Assert.AreEqual("要る", results[4].RepresentWord);
            Assert.AreEqual("鋳る", results[5].RepresentWord);
            Assert.AreEqual("沒る", results[6].RepresentWord);
            Assert.AreEqual("癒る", results[7].RepresentWord);
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchAdjectiveConjugation()
        {
            var tokens = tokenizer.Tokenize("去年はさむかったですね？");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("寒い", results[0].RepresentWord);
            Assert.IsTrue(results[0].Conjugation.Contains("past"));
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchSpecialVerbConjugation()
        {
            var tokens = tokenizer.Tokenize("します来ます");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[0], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("為る", results[0].RepresentWord);
            Assert.IsTrue(results[0].Conjugation.Contains("[polite]"));

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.IsTrue(results[0].Conjugation.Contains("[polite]"));

            tokens = tokenizer.Tokenize("したらへこいへきなさるへ来られる来られます来られない来られません");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[0], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("為る", results[0].RepresentWord);
            Assert.AreEqual("[-tara]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(9, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[imperative]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[4], dictionary);
            Assert.AreEqual(29, results.Count);

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[7], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());            
            Assert.IsTrue(results[0].ReadElement.Contains("きたる"), "きたる");
            Assert.AreEqual("来る", results[1].RepresentWord);            
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());
            Assert.IsTrue(results[1].ReadElement.Contains("くる"), "くる");

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[8], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive] [polite]", results[0].Conjugation.Trim());
            Assert.IsTrue(results[0].ReadElement.Contains("きたる"), "きたる");
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive] [polite]", results[0].Conjugation.Trim());
            Assert.IsTrue(results[1].ReadElement.Contains("くる"), "くる");

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[9], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive or potential] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive] [negative]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[10], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive] [polite] [negative]", results[0].Conjugation.Trim());
            Assert.IsTrue(results[0].ReadElement.Contains("きたる"), "きたる");
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive or potential] [polite] [negative]", results[1].Conjugation.Trim());
            Assert.IsTrue(results[1].ReadElement.Contains("くる"), "くる");
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchSuSpecialAmbigousConjugation()
        {
            var tokens = tokenizer.Tokenize("すらない しろせよ 為る為ったすったすられるすればすろうすらせられるされるすりたい");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[0], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("刷る", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("為る", results[0].RepresentWord);
            Assert.AreEqual("[imperative]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[3], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("為る", results[0].RepresentWord);
            Assert.AreEqual("[imperative]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[5], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("成る", results[0].RepresentWord);
            Assert.AreEqual("", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[6], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("成る", results[0].RepresentWord);
            Assert.AreEqual("[past]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[7], dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("吸う", results[0].RepresentWord);
            Assert.AreEqual("[past]", results[0].Conjugation.Trim());
            Assert.AreEqual("刷る", results[1].RepresentWord);
            Assert.AreEqual("[past]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[8], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("刷る", results[0].RepresentWord);
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[9], dictionary);
            Assert.AreEqual(6, results.Count);
            Assert.AreEqual("為る", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
            Assert.AreEqual("刷る", results[1].RepresentWord);
            Assert.AreEqual("[-ba]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[10], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("刷る", results[0].RepresentWord);
            Assert.AreEqual("[volitional]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[11], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("刷る", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[12], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("為る", results[0].RepresentWord);
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[13], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("刷る", results[0].RepresentWord);
            Assert.AreEqual("[-tai]", results[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchKuSpecialAmbigousConjugation()
        {
            var tokens = tokenizer.Tokenize("くるきますこられる来る来ります来られません来させられません");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[0], dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("繰る", results[0].RepresentWord);
            Assert.AreEqual("来る", results[3].RepresentWord);

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[polite]", results[0].Conjugation.Trim());
            Assert.AreEqual("着る", results[1].RepresentWord);
            Assert.AreEqual("[polite]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[2], dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("凝る", results[0].RepresentWord);
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[2].RepresentWord);
            Assert.AreEqual("[passive or potential]", results[2].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[3], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("来る", results[1].RepresentWord);

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[4], dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[polite]", results[0].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[5], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive] [polite] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive or potential] [polite] [negative]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[6], dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[1].Conjugation.Trim());
        }

        [TestMethod]
        public void TestKuruPotentialAndPassiveConjugation()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("来られる来られない来られた来させられる来させられない来させられません");
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive or potential]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive or potential] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive] [negative]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive] [past]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[passive or potential] [past]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[4], 4, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[5], 5, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("来す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("来る", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[1].Conjugation.Trim());

            words = nlpTokenizer.Tokenize("こられるこられないこられたこさせられるこさせられないこさせられません");
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("凝る", results[0].RepresentWord);
            Assert.AreEqual("[passive]", results[0].Conjugation.Trim());
            Assert.AreEqual("梱る", results[1].RepresentWord);
            Assert.AreEqual("[passive]", results[1].Conjugation.Trim());
            Assert.AreEqual("来る", results[2].RepresentWord);
            Assert.AreEqual("[passive or potential]", results[2].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("来る", results[0].RepresentWord);
            Assert.AreEqual("[passive or potential] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("凝る", results[1].RepresentWord);
            Assert.AreEqual("[passive] [negative]", results[1].Conjugation.Trim());
            Assert.AreEqual("梱る", results[2].RepresentWord);
            Assert.AreEqual("[passive] [negative]", results[2].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("凝る", results[0].RepresentWord);
            Assert.AreEqual("[passive] [past]", results[0].Conjugation.Trim());
            Assert.AreEqual("梱る", results[1].RepresentWord);
            Assert.AreEqual("[passive] [past]", results[1].Conjugation.Trim());
            Assert.AreEqual("来る", results[2].RepresentWord);
            Assert.AreEqual("[passive or potential] [past]", results[2].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("越す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[0].Conjugation.Trim());
            Assert.AreEqual("濾す", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[1].Conjugation.Trim());
            Assert.AreEqual("鼓す", results[2].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[2].Conjugation.Trim());
            Assert.AreEqual("鼓する", results[3].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[3].Conjugation.Trim());
            Assert.AreEqual("来る", results[4].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[4].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[4], 4, words, dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("越す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("濾す", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[1].Conjugation.Trim());
            Assert.AreEqual("鼓す", results[2].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[2].Conjugation.Trim());
            Assert.AreEqual("鼓する", results[3].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[3].Conjugation.Trim());
            Assert.AreEqual("来る", results[4].RepresentWord);
            Assert.AreEqual("[causative] [passive] [negative]", results[4].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[5], 5, words, dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("越す", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("濾す", results[1].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[1].Conjugation.Trim());
            Assert.AreEqual("鼓す", results[2].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[2].Conjugation.Trim());
            Assert.AreEqual("鼓する", results[3].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[3].Conjugation.Trim());
            Assert.AreEqual("来る", results[4].RepresentWord);
            Assert.AreEqual("[causative] [passive] [polite] [negative]", results[4].Conjugation.Trim());
        }

        [TestMethod]
        public void TestFindTokenPerfectMatchPartOfSpeech()
        {
            var tokens = tokenizer.Tokenize("家に帰りましょうか。");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(8, results.Count);
            Assert.AreEqual("に", results[0].RepresentWord);

            tokens = tokenizer.Tokenize("田中さんはどこへ行きますか。");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[1], dictionary);
            Assert.AreEqual(12, results.Count);
            Assert.AreEqual("さん", results[0].RepresentWord);

            tokens = tokenizer.Tokenize("あの人危ない人だなって思った。");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.FindTokenPerfectMatchInDictionary(words[4], dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("成る", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestSearchTokenComboundWord()
        {
            var tokens = tokenizer.Tokenize("うちまで辿り着けないじゃないか");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("たどり着く", results[0].RepresentWord);
            Assert.AreEqual("[potential] [negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("辿る", results[1].RepresentWord);
            Assert.AreEqual("[masu stem]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("付ける", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("着く", results[1].RepresentWord);
            Assert.AreEqual("[potential] [negative]", results[1].Conjugation.Trim());

            tokens = tokenizer.Tokenize("うちまで辿り着け");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("たどり着く", results[0].RepresentWord);
            Assert.AreEqual("[imperative]", results[0].Conjugation.Trim());
            Assert.AreEqual("辿る", results[1].RepresentWord);
            Assert.AreEqual("[masu stem]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("付ける", results[0].RepresentWord);
            Assert.AreEqual("[masu stem]", results[0].Conjugation.Trim());
            Assert.AreEqual("着く", results[1].RepresentWord);
            Assert.AreEqual("[imperative]", results[1].Conjugation.Trim());

            tokens = tokenizer.Tokenize("うちまで辿り着かない");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("たどり着く", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("辿る", results[1].RepresentWord);
            Assert.AreEqual("[masu stem]", results[1].Conjugation.Trim());

            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("着く", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());
            Assert.AreEqual("履く", results[1].RepresentWord);
            Assert.AreEqual("[negative]", results[1].Conjugation.Trim());
        }

        [TestMethod]
        public void TestSearchLongestWord()
        {
            var tokens = tokenizer.Tokenize("田中さんかそうかもしれない");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(12, results.Count);
            Assert.AreEqual("さん", results[0].RepresentWord);
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(27, results.Count);
            Assert.AreEqual("そうかもしれない", results[0].RepresentWord);
            Assert.AreEqual("そう", results[1].RepresentWord);
        }

        [TestMethod]
        public void TestSearchKatakana()
        {
            var tokens = tokenizer.Tokenize("イヤなところといえば、仕事とプライベートがかなりごっちゃになっちゃうってところだな");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(6, results.Count);
            Assert.AreEqual("嫌", results[0].RepresentWord);
            results = TokensDictSearcher.SearchTokenWord(words[8], 8, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("プライベート", results[0].RepresentWord);

            tokens = tokenizer.Tokenize("オレこれから用事あっから");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual("オーレ", results[0].RepresentWord);
            Assert.AreEqual("俺", results[1].RepresentWord);
        }

        [TestMethod]
        public void TestSearchAdjective()
        {
            var tokens = tokenizer.Tokenize("美味しい美味しく美味しくない美味しくなかった美味しかった美味しかったり美味しくて美味しかったら");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);  
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[adverb]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[negative] [past]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[4], 4, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[past]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[5], 5, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[-tari]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[6], 6, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[-te]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[7], 7, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[-tara]", results[0].Conjugation.Trim());

            tokens = tokenizer.Tokenize("静かな");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual("静か", results[0].RepresentWord);
            Assert.AreEqual(null, results[0].Conjugation);            
        }

        [TestMethod]
        public void TestSearchWrongAdjectiveConjun()
        {
            var tokens = tokenizer.Tokenize("美味しかって。美味しくないで。美味しなさい。美味しかってて。");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[6], 6, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[9], 9, words, dictionary);
            Assert.AreEqual("美味しい", results[0].RepresentWord);
            Assert.AreEqual("", results[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestDewanaiAndDearu()
        {
            var tokens = tokenizer.Tokenize("ではない。である。であった。であったら");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual("では無い", results[0].RepresentWord);
            Assert.AreEqual(null, results[0].Conjugation);
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual("である", results[0].RepresentWord);
            Assert.AreEqual("", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[4], 4, words, dictionary);
            Assert.AreEqual("である", results[0].RepresentWord);
            Assert.AreEqual("[past]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[6], 6, words, dictionary);
            Assert.AreEqual("である", results[0].RepresentWord);
            Assert.AreEqual("[-tara]", results[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestUniqueGodanPotential()
        {
            var tokens = tokenizer.Tokenize("蜂蜜が採れるまでの流れとして");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("採れる", results[0].RepresentWord);
            Assert.IsTrue(String.IsNullOrEmpty(results[0].Conjugation), "Wrong conjugation");
            Assert.AreEqual("採る", results[1].RepresentWord);
            Assert.AreEqual("[potential]", results[1].Conjugation.Trim());
        }

        [TestMethod]
        public void TestSpecialSuruVerbDetectedAsGodan()
        {
            var tokens = tokenizer.Tokenize("接して愛して反して話して");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("接する", results[0].RepresentWord);                        
            Assert.AreEqual("[-te]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("愛す", results[0].RepresentWord);
            Assert.AreEqual("[-te]", results[0].Conjugation.Trim());
            Assert.AreEqual("愛する", results[1].RepresentWord);
            Assert.AreEqual("[-te]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("反する", results[0].RepresentWord);
            Assert.AreEqual("[-te]", results[0].Conjugation.Trim());
            Assert.AreEqual("返す", results[1].RepresentWord);
            Assert.AreEqual("[-te]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("話す", results[0].RepresentWord);
            Assert.AreEqual("[-te]", results[0].Conjugation.Trim());

            tokens = tokenizer.Tokenize("接させる愛させる反させる話させる");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("接する", results[0].RepresentWord);
            Assert.AreEqual("[causative]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("愛す", results[0].RepresentWord);
            Assert.AreEqual("[causative]", results[0].Conjugation.Trim());
            Assert.AreEqual("愛する", results[1].RepresentWord);
            Assert.AreEqual("[causative]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("反する", results[0].RepresentWord);
            Assert.AreEqual("[causative]", results[0].Conjugation.Trim());
            Assert.AreEqual("返す", results[1].RepresentWord);
            Assert.AreEqual("[causative]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("話す", results[0].RepresentWord);
            Assert.AreEqual("[causative]", results[0].Conjugation.Trim());

            tokens = tokenizer.Tokenize("接します愛します反します話します");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("接する", results[0].RepresentWord);
            Assert.AreEqual("[polite]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("愛す", results[0].RepresentWord);
            Assert.AreEqual("[polite]", results[0].Conjugation.Trim());
            Assert.AreEqual("愛する", results[1].RepresentWord);
            Assert.AreEqual("[polite]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("反する", results[0].RepresentWord);
            Assert.AreEqual("[polite]", results[0].Conjugation.Trim());
            Assert.AreEqual("返す", results[1].RepresentWord);
            Assert.AreEqual("[polite]", results[1].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("話す", results[0].RepresentWord);
            Assert.AreEqual("[polite]", results[0].Conjugation.Trim());

            tokens = tokenizer.Tokenize("接すれば愛すれば反すれば話すれば");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("接する", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("愛する", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("反する", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("話", results[0].RepresentWord);            

            tokens = tokenizer.Tokenize("接せば愛せば反せば話せば");
            words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(0, results.Count);
            results = TokensDictSearcher.SearchTokenWord(words[1], 1, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("愛す", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("返す", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[3], 3, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("話す", results[0].RepresentWord);
            Assert.AreEqual("[-ba]", results[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestNullBaseReorderFirstMatch()
        {
            var tokens = tokenizer.Tokenize("とは言っても、何の着想もない。");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[5], 5, words, dictionary);
            Assert.AreEqual(3, results.Count);
        }

        [TestMethod]
        public void TestNLPTokenizer()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("というものであった");
            Assert.AreEqual(3, words.Count);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("と言うもの", results[0].RepresentWord);
            Assert.AreEqual("と言う", results[1].RepresentWord);

            words = nlpTokenizer.TokenizeReducedSentence();
            Assert.AreEqual(2, words.Count);
            results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("と言うもの", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestReorderVerbAndAdverbPOS()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("頑張らなくてもいいしね");
            Assert.AreEqual(5, words.Count);
            var results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(8, results.Count);
            Assert.AreEqual("言う", results[0].RepresentWord);
            Assert.AreEqual("結う", results[1].RepresentWord);
            Assert.AreEqual("いい", results[2].RepresentWord);            
        }

        [TestMethod]
        public void TestDoublePotentialBug()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("並べられる");
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("並べる", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestDesuConjugation()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("です。でした。ではない。");
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("です", results[0].RepresentWord);

            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("です", results[0].RepresentWord);

            results = TokensDictSearcher.SearchTokenWord(words[4], 4, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("では無い", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestSpecialKuruVerbs()
        {
            var tokens = tokenizer.Tokenize("やって来ない。やって来た。やって来させられる。");
            var words = TokensDictSearcher.ConvertTokensToWords(tokens, dictionary);
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("やって来ない", words[0].Surface);
            Assert.AreEqual("やって来る", results[0].RepresentWord);
            Assert.AreEqual("[negative]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("やって来た", words[2].Surface);
            Assert.AreEqual("やって来る", results[0].RepresentWord);
            Assert.AreEqual("[past]", results[0].Conjugation.Trim());
            results = TokensDictSearcher.SearchTokenWord(words[4], 4, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("やって来させられる", words[4].Surface);
            Assert.AreEqual("やって来る", results[0].RepresentWord);
            Assert.AreEqual("[causative] [passive]", results[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestLongestWordSearchSKipSurface()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("こういった。");            
            var results = TokensDictSearcher.SearchTokenWord(words[0], 0, words, dictionary);            
            Assert.AreEqual(47, results.Count);
            Assert.AreEqual("こう言う", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestNotFoundBasefromButHasSurface()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("お人好しじゃない");
            var results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("ない", results[0].RepresentWord);
        }

        [TestMethod]
        public void TestIkuCombound()
        {
            var nlpTokenizer = new NLPTokenizer<Token>(tokenizer, dictionary);
            var words = nlpTokenizer.Tokenize("管理が行き届いている");
            var results = TokensDictSearcher.SearchTokenWord(words[2], 2, words, dictionary);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("行き届く", results[0].RepresentWord);
        }
    }
}

