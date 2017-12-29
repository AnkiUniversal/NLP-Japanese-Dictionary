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
using NLPJapaneseDictionary.Kuromoji.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.ConvertClasses
{
    [TestClass]
    public class RomaConvertTest
    {
        [TestMethod]
        public void TestRomaToHiraPhrase()
        {
            string hiragana = "sonokotobanijiikuhakensonwosurukotomonaku、mata、kouteiwosurukotomonaku、odayakanabieminohyoujoudakewomisetekureta。";
            string actual = RomaConvert.ConvertRomaToHiraFullLoop(hiragana);
            string expected = "そのことばにじいくはけんそんをすることもなく、また、こうていをすることもなく、おだやかなびえみのひょうじょうだけをみせてくれた。";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestRomaToHiraSilentVowel()
        {
            string hiragana = "「kaettadesuka？」 toitteimasu。";
            string actual = RomaConvert.ConvertRomaToHiraFullLoop(hiragana);
            string expected = "「かえったですか？」 といっています。";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestRomaToHiraAllSilentVowel()
        {
            string hiragana = "wworrottoyyoppossoddoffuggohhojjokkozzokkoommo";
            string actual = RomaConvert.ConvertRomaToHiraFullLoop(hiragana);
            string expected = "っをっろっとっよっぽっそっどっふっごっほっじょっこっぞっこおっも";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestRomaToHiraFullTableFullLoop()
        {
            using (var file = File.OpenRead("./ConvertClasses/Resource/TableTest.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');                 
                    string actual = RomaConvert.ConvertRomaToHiraFullLoop(line[0]);
                    string expected = line[1];
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestHiraToRomaPhrase()
        {
            string hiragana = "そのことばにじいくはけんそんをすることもなく、また、こうていをすることもなく、おだやかなびえみのひょうじょうだけをみせてくれた。";
            string actual = RomaConvert.ConvertHiraToRomaFullLoop(hiragana);
            string expected = "sonokotobanijiikuhakensonwosurukotomonaku、mata、kouteiwosurukotomonaku、odayakanabieminohyoujoudakewomisetekureta。";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestHiraToRomaSilentVowel()
        {
            string hiragana = "「かえったですか？」 といっています。";
            string actual = RomaConvert.ConvertHiraToRomaFullLoop(hiragana);
            string expected = "「kaettadesuka？」 toitteimasu。";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }


        [TestMethod]
        public void TestHiraToRomaAllSilentVowel()
        {
            string hiragana = "っをっろっとっよっぽっそっどっふっごっほっじょっこっぞっこんおっもっ";
            string actual = RomaConvert.ConvertHiraToRomaFullLoop(hiragana);
            string expected = "wworrottoyyoppossoddoffuggohhojjokkozzokkonommo";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestFullTableFullLoop()
        {
            using (var file = File.OpenRead("./ConvertClasses/Resource/TableTest.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    string actual = RomaConvert.ConvertHiraToRomaFullLoop(line[1]);
                    string expected = line[0];
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestFullTableOneHiraToRoma()
        {
            using (var file = File.OpenRead("./ConvertClasses/Resource/TableTest.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    string actual = RomaConvert.ConvertOneHiraToRoma(line[1].Trim());
                    string expected = line[0].Trim();
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestFullTableOneRomaToHira()
        {
            using (var file = File.OpenRead("./ConvertClasses/Resource/TableTest.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    string actual = RomaConvert.ConvertOneRomaToHira(line[0].Trim());
                    string expected = line[1].Trim();
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestKataToRomaPhrase()
        {
            string kata = "ソノコトバニジークハケンソンヲスルコトモナク、マタ、コウテイヲスルコトモナク、オダヤカナビエミノヒョウジョウダケヲミセテクレタ。";
            string actual = RomaConvert.ConvertKataToRomaFullLoop(kata);
            string expected = "sonokotobaniji-kuhakensonwosurukotomonaku、mata、kouteiwosurukotomonaku、odayakanabieminohyoujoudakewomisetekureta。";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestFullTableKataToRoma()
        {
            using (var file = File.OpenRead("./ConvertClasses/Resource/TableTest.txt"))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    string actual = RomaConvert.ConvertKataToRomaFullLoop(line[2].Trim());
                    string expected = line[0].Trim();
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        public void TestKataToRomaSilentVowel()
        {
            string kata = "「カエッタデスカ？」 トイッテイマス。";            
            string actual = RomaConvert.ConvertKataToRomaFullLoop(kata);
            string expected = "「kaettadesuka？」 toitteimasu。";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestKataToRomaAllSilentVowel()
        {
            string kata = "ッヲッロットッヨッポッソッドッフッゴッホッジョッコッゾッコンオッモッ";            
            string actual = RomaConvert.ConvertKataToRomaFullLoop(kata);
            string expected = "wworrottoyyoppossoddoffuggohhojjokkozzokkonommo";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }

        [TestMethod]
        public void TestKataExtend()
        {          
            string roma = "fafifefofyuwiwevavivevotsatsitsocheshejetelitoluyeulodelidelyukwavu";
            string actual = RomaConvert.ConvertRomaToHiraFullLoop(roma);
            string expected = "ふぁふぃふぇふぉふゅうぃうぇヴぁヴぃヴぇヴぉつぁつぃつぉちぇしぇじぇてぃとぅいぇうぉでぃでゅくぁヴ";
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);

            string kata = "ファフィフェフォフュウィウェヴァヴィヴェヴォツァツィツォチェシェジェティトゥイェウォディデュクァヴ";
            actual = RomaConvert.ConvertKataToRomaFullLoop(kata);
            expected = "fafifefofyuwiwevavivevotsatsitsocheshejetituyewodidukwavu";            
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], actual[i]);
        }
    }
}
