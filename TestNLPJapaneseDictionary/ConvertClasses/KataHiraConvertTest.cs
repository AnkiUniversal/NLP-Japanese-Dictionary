using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.ConvertClasses;
using NLPJDict.HelperClasses;
using NLPJDict.Kuromoji.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.ConvertClasses
{
    [TestClass]
    public class KataHiraConvertTest
    {
        [TestMethod]
        public void TestKataToHiraSimpleConvert()
        {
            string katakana = "ソノコトバニジークハケンソンヲスルコトモナク、マタ、コウテイヲスルコトモナク、オダヤカナビエミノヒョウジョウダケヲミセテクレタ。";
            string actual = KataHiraConvert.ConvertKataToHira(katakana);
            string expected = "そのことばにじいくはけんそんをすることもなく、また、こうていをすることもなく、おだやかなびえみのひょうじょうだけをみせてくれた。";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestKataToHiraFullTable()
        {
            using (var file = File.OpenRead(Locations.ABS_DICT_CONVERT_PATH + "RomaHiraKata.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    string actual = KataHiraConvert.ConvertKataToHira(line[2]);
                    string expected = line[1];
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestHiraToKataSimpleConvert()
        {
            string sentence = "そのことばにじいくはけんそんをすることもなく、また、こうていをすることもなく、おだやかなびえみのひょうじょうだけをみせてくれた。";
            string actual = KataHiraConvert.ConvertHiraToKata(sentence);
            string expected = "ソノコトバニジイクハケンソンヲスルコトモナク、マタ、コウテイヲスルコトモナク、オダヤカナビエミノヒョウジョウダケヲミセテクレタ。";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHiraToKataFullTable()
        {
            using (var file = File.OpenRead(Locations.ABS_DICT_CONVERT_PATH + "RomaHiraKata.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    string actual = KataHiraConvert.ConvertHiraToKata(line[1]);
                    string expected = line[2];
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestKataExtend()
        {
            string sentence = "ふぁふぃふぇふぉふゅうぃうぇヴぁヴぃヴぇヴぉつぁつぃつぉちぇしぇじぇてぃとぅいぇうぉでぃでゅ";
            string actual = KataHiraConvert.ConvertHiraToKata(sentence);
            string expected = "ファフィフェフォフュウィウェヴァヴィヴェヴォツァツィツォチェシェジェティトゥイェウォディデュ";
            actual = KataHiraConvert.ConvertKataToHira(expected);
            Assert.AreEqual(sentence, actual);
        }
    }
}
