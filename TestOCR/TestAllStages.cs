using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJapaneseDictionary.OCR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.OCR
{
    [TestClass]
    public class TestOCR
    {
        private static string ImagesPath = "./Resource/";

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            JocrWrapper.InitOcrParameters();
        }

        [TestMethod]
        public void TestVerticalMultiLines()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVerticalMultiLines.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("俺の言葉を聞いた三人の男の不安そうな声。アタアがその三人を見ながら言った。", text);
        }

        [TestMethod]
        public void TestVerticalMultiLines2()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVerticalMultiLines2.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("くそハどこ行ったのよハい", text);
        }

        [TestMethod]
        public void TestVertical1()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical1.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("おっちゃん、この指輪を一つ", text);
        }

        [TestMethod]
        public void TestVertical2()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical2.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("中の手紙が破れていますが、", text);
        }

        [TestMethod]
        public void TestVertical3()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical3.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("そんな、騒がしい二人の声を聞きながら。", text);
        }

        [TestMethod]
        public void TestVertical4()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical4.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("｢野良デュラハンを捕まえに行こうかと思うんだ｣", text);
        }

        [TestMethod]
        public void TestVertical5()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical5.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("｢うむ、よろしいだろう｣", text);
        }

        [TestMethod]
        public void TestVertical6()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical6.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("｢おいバカやめろ、勝手な事すんなよｌキそんなもんより", text);
        }

        [TestMethod]
        public void TestVertical7()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVertical7.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("桂木さんのこと", text);
        }

        [TestMethod]
        public void TestVerticalHalfMerge1()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testVerticalHalfMerge1.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("どいつも", text);

        }

        [TestMethod]
        public void TestHorizontal1()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal1.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("一応、アールトネン隊長にも声を掛けておいた。", text);
        }

        [TestMethod]
        public void TestHorizontal2()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal2.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("そこで、詳しいハロウィンの話を聞くことになる。", text);
        }

        [TestMethod]
        public void TestHorizontal3()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal3.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("記憶があいまいになっているようで、かなり適当な感じがした。", text);
        }

        [TestMethod]
        public void TestHorizontal4()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal4.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("アルノーは大きくなったらいろいろと可愛い怪物の扮装をして欲しいなと思っ", text);
        }

        [TestMethod]
        public void TestHorizontal5()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal5.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("で、森で狩った獲物を引いて歩く男衆の姿も多い。", text);
        }

        [TestMethod]
        public void TestHorizontal6()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal6.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("フオークに肉団子を刺して一口。", text);
        }

        [TestMethod]
        public void TestHorizontal7()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal7.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("他に、規則正しい生活をしているので、体の調子が良くなった。", text);
        }

        [TestMethod]
        public void TestHorizontal8()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal8.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("ゆくゆくは", text);

        }

        [TestMethod]
        public void TestHorizontal9()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal9.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("辛く長い極夜を村の皆で過ごす、初めての冬の話であった。", text);

        }

        [TestMethod]
        public void TestHorizontal10()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal10.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("第３補欠生駒みなみ", text);
        }

        [TestMethod]
        public void TestHorizontal11()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontal11.PNG");
            var text = GetSentenceFirstWord(textBlocks);

            //This is quite an intersting case as test results using SoftwareBitmap in UWP will output the correct words (です)
            //while in WPF with Bitmap the results are でず
            Assert.AreEqual("シールドとはギターとアンブをつなぐコードのことでずよ。", text);
        }

        [TestMethod]
        public void TestHorizontalHalfMerge1()
        {
            var textBlocks = JocrWrapper.ExtractTextFromLocalImage(ImagesPath + "testHorizontalHalfMerge1.PNG");
            var text = GetSentenceFirstWord(textBlocks);
            Assert.AreEqual("今日は母も手伝ってくれると言う。", text);

        }

        private static string GetSentenceFirstWord(List<Jocr.TextBlock> blocks)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var block in blocks)
            {
                builder.Append(block.Text[0]);
            }
            return builder.ToString();
        }
    }
}
