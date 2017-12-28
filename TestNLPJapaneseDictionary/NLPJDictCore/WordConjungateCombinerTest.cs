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
    public class WordConjungateCombinerTest
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
        public void TestSimplePhrase()
        {
            var tokens = tokenizer.Tokenize("この動詞の変化を言えますか");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(7, words.Count);
            Assert.AreEqual("この", words[0].Surface);
            Assert.AreEqual("動詞", words[1].Surface);
            Assert.AreEqual("の", words[2].Surface);
            Assert.AreEqual("変化", words[3].Surface);
            Assert.AreEqual("を", words[4].Surface);
            Assert.AreEqual("言えます", words[5].Surface);
            Assert.AreEqual("か", words[6].Surface);

            Assert.AreEqual("", words[0].Conjugation);
            Assert.AreEqual("", words[1].Conjugation);
            Assert.AreEqual("", words[2].Conjugation);
            Assert.AreEqual("", words[3].Conjugation);
            Assert.AreEqual("", words[4].Conjugation);
            Assert.AreEqual("[polite] ", words[5].Conjugation);
            Assert.AreEqual("", words[6].Conjugation);

            Assert.AreEqual("この", words[0].BaseForm);
            Assert.AreEqual("動詞", words[1].BaseForm);
            Assert.AreEqual("の", words[2].BaseForm);
            Assert.AreEqual("変化", words[3].BaseForm);
            Assert.AreEqual("を", words[4].BaseForm);
            Assert.AreEqual("言える", words[5].BaseForm);
            Assert.AreEqual("か", words[6].BaseForm);
        }

        [TestMethod]
        public void TestIchidan()
        {
            string path = RESOURCE + "TestIchidanConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    Assert.AreEqual(array[2].Trim(), words[count].BaseForm.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanU()
        {
            string path = RESOURCE + "TestGodanUConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanTsu()
        {
            string path = RESOURCE + "TestGodanTsuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanRu()
        {
            string path = RESOURCE + "TestGodanRuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanNu()
        {
            string path = RESOURCE + "TestGodanNuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanMu()
        {
            string path = RESOURCE + "TestGodanMuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanBu()
        {
            string path = RESOURCE + "TestGodanBuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanGu()
        {
            string path = RESOURCE + "TestGodanGuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanKu()
        {
            string path = RESOURCE + "TestGodanKuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestGodanSu()
        {
            string path = RESOURCE + "TestGodanSuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestSpecialSu()
        {
            string path = RESOURCE + "TestSpecialSuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    while(String.IsNullOrWhiteSpace(words[count].Surface))
                        count++;

                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestSpecialKu()
        {
            string path = RESOURCE + "TestSpecialKuConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    while (String.IsNullOrWhiteSpace(words[count].Surface))
                        count++;

                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TestSpecialKuKanji()
        {
            string path = RESOURCE + "TestSpecialKuKanjiConjugation.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    while (String.IsNullOrWhiteSpace(words[count].Surface))
                        count++;

                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TesIAdjective()
        {
            string path = RESOURCE + "TestIAdjectiveConjungate.txt";
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                var line = reader.ReadLine();
                Assert.IsTrue(!String.IsNullOrWhiteSpace(line));

                var tokens = tokenizer.Tokenize(line);
                var words = WordConjungateCombiner.Combine(tokens, dictionary);
                int count = 0;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var array = line.Split(new string[] { ";", "；" }, StringSplitOptions.None);
                    Assert.AreEqual(array[0].Trim(), words[count].Surface.Trim());
                    Assert.AreEqual(array[1].Trim(), words[count].Conjugation.Trim());
                    count++;
                }
            }
        }

        [TestMethod]
        public void TesIruConjugation()
        {
            var tokens = tokenizer.Tokenize("泳いでた超えてた言ってた読んでた書いてた記してた泳いでた超えてた");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.IsTrue(words[0].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[1].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[2].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[3].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[4].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[5].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[6].Conjugation.Contains("[-te iru] [past]"));
            Assert.IsTrue(words[7].Conjugation.Contains("[-te iru] [past]"));

            tokens = tokenizer.Tokenize("泳いでて超えてて言ってて読んでて書いてて記してて泳いでて超えてて");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.IsTrue(words[0].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[1].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[2].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[3].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[4].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[5].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[6].Conjugation.Contains("[-te iru] [-te]"));
            Assert.IsTrue(words[7].Conjugation.Contains("[-te iru] [-te]"));

            tokens = tokenizer.Tokenize("泳いでる超えてる言ってる読んでる書いてる記してる泳いでる超えてる");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.AreEqual("[-te iru]", words[0].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[1].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[2].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[3].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[4].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[5].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[6].Conjugation.Trim());
            Assert.AreEqual("[-te iru]", words[7].Conjugation.Trim());
        }

        [TestMethod]
        public void TestTaiConjugation()
        {
            var tokens = tokenizer.Tokenize("泳ぎたい食べたい言いたい読みたい書きたい記したい知りたい食べたい");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.AreEqual("[-tai]", words[0].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[1].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[2].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[3].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[4].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[5].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[6].Conjugation.Trim());
            Assert.AreEqual("[-tai]", words[7].Conjugation.Trim());

            tokens = tokenizer.Tokenize("泳ぎたくない食べたくない買いたくない読みたくない書きたくない記したくない知りたくない食べたくない");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.AreEqual("[-tai] [negative]", words[0].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[1].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[2].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[3].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[4].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[5].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[6].Conjugation.Trim());
            Assert.AreEqual("[-tai] [negative]", words[7].Conjugation.Trim());

            tokens = tokenizer.Tokenize("泳ぎたかった食べたかった買いたかった読みたかった書きたかった記したかった知りたかった食べたかった");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.AreEqual("[-tai] [past]", words[0].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[1].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[2].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[3].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[4].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[5].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[6].Conjugation.Trim());
            Assert.AreEqual("[-tai] [past]", words[7].Conjugation.Trim());

            tokens = tokenizer.Tokenize("泳ぎたくて食べたくて買いたくて読みたくて書きたくて記したくて知りたくて食べたくて");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.AreEqual("[-tai] [-te]", words[0].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[1].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[2].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[3].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[4].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[5].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[6].Conjugation.Trim());
            Assert.AreEqual("[-tai] [-te]", words[7].Conjugation.Trim());
        }

        [TestMethod]
        public void TestTariConjugation()
        {
            var tokens = tokenizer.Tokenize("泳いだり食べたり言ったり読んだり書いたり記したり知ったりしたり来たりきたり美味しかったりだったり");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(12, words.Count);
            foreach(var word in words)
                Assert.AreEqual("[-tari]", word.Conjugation.Trim());            
       
        }

        [TestMethod]
        public void TestCausativeImperative()
        {
            var tokens = tokenizer.Tokenize("脱がせろ食べられろ買わせろ読ませろ書かせろ記させろ遣らせろ脱がせろ");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            Assert.AreEqual("[causative] [imperative]", words[0].Conjugation.Trim());
            Assert.AreEqual("[passive or potential] [imperative]", words[1].Conjugation.Trim());
            Assert.AreEqual("[causative] [imperative]", words[2].Conjugation.Trim());
            Assert.AreEqual("[causative] [imperative]", words[3].Conjugation.Trim());
            Assert.AreEqual("[causative] [imperative]", words[4].Conjugation.Trim());
            Assert.AreEqual("[causative] [imperative]", words[5].Conjugation.Trim());
            Assert.AreEqual("[causative] [imperative]", words[6].Conjugation.Trim());
            Assert.AreEqual("[causative] [imperative]", words[7].Conjugation.Trim());
        }

        [TestMethod]
        public void TestSimpleVerbWrongConjugation()
        {
            var tokens = tokenizer.Tokenize("読んたら食べたられる呼びない呼ばれらせる呼びませんでた");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("読ん", words[0].Surface);
            Assert.AreEqual("", words[0].Conjugation.Trim());
            Assert.AreEqual("たら", words[1].Surface);
            Assert.AreEqual("", words[1].Conjugation.Trim());

            Assert.AreEqual("食べたら", words[2].Surface);
            Assert.AreEqual("[-tara]", words[2].Conjugation.Trim());
            Assert.AreEqual("れる", words[3].Surface);
            Assert.AreEqual("", words[3].Conjugation.Trim());

            Assert.AreEqual("呼び", words[4].Surface);
            Assert.AreEqual("[masu stem]", words[4].Conjugation.Trim());
            Assert.AreEqual("ない", words[5].Surface);
            Assert.AreEqual("", words[5].Conjugation.Trim());

            Assert.AreEqual("呼ばれ", words[6].Surface);
            Assert.AreEqual("[passive]", words[6].Conjugation.Trim());
            Assert.AreEqual("ら", words[7].Surface);
            Assert.AreEqual("", words[7].Conjugation.Trim());
            Assert.AreEqual("せる", words[8].Surface);
            Assert.AreEqual("", words[8].Conjugation.Trim());

            Assert.AreEqual("呼びません", words[9].Surface);
            Assert.AreEqual("[polite] [negative]", words[9].Conjugation.Trim());
            Assert.AreEqual("で", words[10].Surface);
            Assert.AreEqual("", words[10].Conjugation.Trim());
            Assert.AreEqual("た", words[11].Surface);
            Assert.AreEqual("", words[11].Conjugation.Trim());
        }

        [TestMethod]
        public void TestAdjectiveWrongConjugation()
        {
            var tokens = tokenizer.Tokenize("美味しかって。美味しくないで。美味しなさい。美味しかってて。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("美味しかっ", words[0].Surface);
            Assert.AreEqual("", words[0].Conjugation.Trim());
            Assert.AreEqual("て", words[1].Surface);
            Assert.AreEqual("", words[1].Conjugation.Trim());

            Assert.AreEqual("美味しくない", words[3].Surface);
            Assert.AreEqual("[negative]", words[3].Conjugation.Trim());
            Assert.AreEqual("で", words[4].Surface);
            Assert.AreEqual("", words[4].Conjugation.Trim());

            Assert.AreEqual("美味し", words[6].Surface);
            Assert.AreEqual("", words[6].Conjugation.Trim());
            Assert.AreEqual("なさい", words[7].Surface);
            Assert.AreEqual("[imperative]", words[7].Conjugation.Trim());

            Assert.AreEqual("美味しかっ", words[9].Surface);
            Assert.AreEqual("", words[9].Conjugation.Trim());
        }

        [TestMethod]
        public void TestDeWaAndDeAru()
        {
            var tokens = tokenizer.Tokenize("である。であった。であります。でありません。であって。でありたい。であったら。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("である", words[0].Surface);
            Assert.AreEqual("である", words[0].BaseForm);
            Assert.AreEqual("", words[0].Conjugation.Trim());
            Assert.AreEqual("であった", words[2].Surface);
            Assert.AreEqual("である", words[2].BaseForm);
            Assert.AreEqual("[past]", words[2].Conjugation.Trim());
            Assert.AreEqual("であります", words[4].Surface);
            Assert.AreEqual("である", words[4].BaseForm);
            Assert.AreEqual("[polite]", words[4].Conjugation.Trim());
            Assert.AreEqual("でありません", words[6].Surface);
            Assert.AreEqual("である", words[6].BaseForm);
            Assert.AreEqual("[polite] [negative]", words[6].Conjugation.Trim());
            Assert.AreEqual("であって", words[8].Surface);
            Assert.AreEqual("である", words[8].BaseForm);
            Assert.AreEqual("[-te]", words[8].Conjugation.Trim());
            Assert.AreEqual("でありたい", words[10].Surface);
            Assert.AreEqual("である", words[10].BaseForm);
            Assert.AreEqual("[-tai]", words[10].Conjugation.Trim());
            Assert.AreEqual("であったら", words[12].Surface);
            Assert.AreEqual("である", words[12].BaseForm);
            Assert.AreEqual("[-tara]", words[12].Conjugation.Trim());

            tokens = tokenizer.Tokenize("ではある。ではあります。ではない。ではあったら。");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("では", words[0].Surface);
            Assert.AreEqual("ある", words[1].BaseForm);
            Assert.AreEqual("では", words[3].Surface);
            Assert.AreEqual("ある", words[4].BaseForm);
            Assert.AreEqual("では", words[6].Surface);
            Assert.AreEqual("ない", words[7].BaseForm);
            Assert.AreEqual("では", words[9].Surface);
            Assert.AreEqual("ある", words[10].BaseForm);
        }

        [TestMethod]
        public void TestMasuStem()
        {
            var tokens = tokenizer.Tokenize("脱ぎ食べ言い飲み記し知り行き呼び");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(8, words.Count);
            foreach(var word in words)
                Assert.AreEqual("[masu stem]", word.Conjugation.Trim());

            tokens = tokenizer.Tokenize("何をし。これへ来。これへきつつ。");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("[masu stem]", words[2].Conjugation.Trim());
            Assert.AreEqual("[masu stem]", words[6].Conjugation.Trim());
            Assert.AreEqual("[masu stem]", words[10].Conjugation.Trim());
        }

        [TestMethod]
        public void TestUndetectAdverbForm()
        {
            var tokens = tokenizer.Tokenize("恐ろしく硬いもので");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(4, words.Count);
            Assert.AreEqual("恐ろしく", words[0].Surface);
            Assert.AreEqual("恐ろしい", words[0].BaseForm);
            Assert.AreEqual("[adverb]", words[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestSouConjugation()
        {
            var tokens = tokenizer.Tokenize("来そう泳ぎそう食べそう言いそう読みそう記しそう知りそうしそうきそう書きそう");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);            
            foreach(var word in words)
                Assert.AreEqual("[-sou]", word.Conjugation.Trim());      
        }

        [TestMethod]
        public void TestAdverbToFollow()
        {
            var tokens = tokenizer.Tokenize("意外と美味しかったので");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.IsTrue(words[0].IsInDictionary);
            Assert.AreEqual("意外と", words[0].Surface);
            Assert.AreEqual("意外", words[0].BaseForm);
        }

        [TestMethod]
        public void TestCorrectUnknowStatusKatakana()
        {
            var tokens = tokenizer.Tokenize("母はパイを口に入れた瞬間に目がキラリと輝いた。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(true, words[11].IsInDictionary);
            Assert.AreEqual(false, words[11].IsUnknownWord);
        }

        [TestMethod]
        public void TestGaruVerbConjugation()
        {
            var tokens = tokenizer.Tokenize("覚えたがる。覚えたがらない。覚えたがります。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("覚えたがる", words[0].Surface);
            Assert.AreEqual("[-tai] [-garu]", words[0].Conjugation.Trim());
            Assert.AreEqual("覚えたがらない", words[2].Surface);
            Assert.AreEqual("[-tai] [-garu] [negative]", words[2].Conjugation.Trim());
            Assert.AreEqual("覚えたがります", words[4].Surface);
            Assert.AreEqual("[-tai] [-garu] [polite]", words[4].Conjugation.Trim());

            tokens = tokenizer.Tokenize("覚えたいがる。覚えがる。覚えるがる。");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(9, words.Count);

            tokens = tokenizer.Tokenize("熱がる。暑がらない。熱がります。暑いがる。");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(11, words.Count);
            Assert.AreEqual("熱がる", words[0].Surface);
            Assert.AreEqual("[-garu]", words[0].Conjugation.Trim());
            Assert.AreEqual("暑", words[2].Surface);
        }

        [TestMethod]
        public void TestAdjectiveVolitionalForm()
        {
            var tokens = tokenizer.Tokenize("赤かろう。なかろう。赤くなかろう。");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual("赤かろう", words[0].Surface);
            Assert.AreEqual("[volitional]", words[0].Conjugation.Trim());
            Assert.AreEqual("なかろう", words[2].Surface);
            Assert.AreEqual("[volitional]", words[2].Conjugation.Trim());
            Assert.AreEqual("赤くなかろう", words[4].Surface);
            Assert.AreEqual("[negative] [volitional]", words[4].Conjugation.Trim());
        }

        [TestMethod]
        public void TestGodanPotentialYoConjugation()
        {
            var tokens = tokenizer.Tokenize("を手伝えよ。を手伝えよか");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(7, words.Count);
            Assert.AreEqual("手伝え", words[1].Surface.Trim());
            Assert.AreEqual("テツダエ", words[1].Pronunciation.Trim());
            Assert.AreEqual("テツダエ", words[1].Reading.Trim());
            Assert.AreEqual("[imperative]", words[1].Conjugation.Trim());

            Assert.AreEqual("手伝えよ", words[5].Surface.Trim());
            Assert.AreEqual("テツダエヨ", words[5].Pronunciation.Trim());
            Assert.AreEqual("テツダエヨ", words[5].Reading.Trim());
            Assert.AreEqual("[potential] [imperative]", words[5].Conjugation.Trim());
        }

        [TestMethod]
        public void TestIchidanCombineYoConjugation()
        {
            var tokens = tokenizer.Tokenize("うちまで辿り着けよ");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(4, words.Count);
            Assert.AreEqual("着けよ", words[3].Surface.Trim());
            Assert.AreEqual("[imperative]", words[3].Conjugation.Trim());
        }

        [TestMethod]
        public void TestTrySplitWordNotInDictionary()
        {
            var tokens = tokenizer.Tokenize("アークプリースト");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(2, words.Count);
            Assert.AreEqual("アーク", words[0].Surface.Trim());
            Assert.AreEqual("プリースト", words[1].Surface.Trim());
            Assert.AreEqual("アーク", words[0].Reading.Trim());
            Assert.AreEqual("プリースト", words[1].Reading.Trim());
            Assert.AreEqual("アーク", words[0].Pronunciation.Trim());
            Assert.AreEqual("プリースト", words[1].Pronunciation.Trim());

            tokens = tokenizer.Tokenize("昨日の残りもの");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(4, words.Count);
            Assert.AreEqual("残り", words[2].Surface.Trim());
            Assert.AreEqual("ノコリ", words[2].Reading.Trim());
            Assert.AreEqual("ノコリ", words[2].Pronunciation.Trim());
            Assert.AreEqual("もの", words[3].Surface.Trim());
            Assert.AreEqual("モノ", words[3].Reading.Trim());
            Assert.AreEqual("モノ", words[3].Pronunciation.Trim());

            tokens = tokenizer.Tokenize("ミツラギ");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(1, words.Count);
            Assert.AreEqual("ミツラギ", words[0].Surface.Trim());
        }

        [TestMethod]
        public void TestTaiSou()
        {
            var tokens = tokenizer.Tokenize("言いたそうに");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(3, words.Count);
            Assert.AreEqual("言いた", words[0].Surface.Trim());
            Assert.AreEqual("言う", words[0].BaseForm.Trim());
            Assert.AreEqual("[-tai]", words[0].Conjugation.Trim());
        }

        [TestMethod]
        public void TestDeshou()
        {
            var tokens = tokenizer.Tokenize("英雄なんでしょう");
            var words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(4, words.Count);
            Assert.AreEqual("でしょう", words[3].Surface.Trim());
            Assert.AreEqual("です", words[3].BaseForm.Trim());

            tokens = tokenizer.Tokenize("言うのだろう");
            words = WordConjungateCombiner.Combine(tokens, dictionary);
            Assert.AreEqual(3, words.Count);
            Assert.AreEqual("だろう", words[2].Surface.Trim());
            Assert.AreEqual("だ", words[2].BaseForm.Trim());
        }
    }
}
