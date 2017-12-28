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
