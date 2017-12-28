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

using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.kuromoji.Core.Dict
{
    [TestClass]
    public class UserDictionaryTest
    {
        [TestMethod]
        public void TestLookup()
        {
            UserDictionary dictionary;
            using (var stream = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(stream, 9, 7, 0);
            }

            List<UserDictionary.UserDictionaryMatch> matches = dictionary.FindUserDictionaryMatches("関西国際空港に行った");

            // Length should be three 関西, 国際, 空港
            Assert.AreEqual(3, matches.Count);

            // Test positions
            Assert.AreEqual(0, matches[0].GetMatchStartIndex()); // index of 関西
            Assert.AreEqual(2, matches[1].GetMatchStartIndex()); // index of 国際
            Assert.AreEqual(4, matches[2].GetMatchStartIndex()); // index of 空港

            // Test lengths
            Assert.AreEqual(2, matches[0].GetMatchLength()); // length of 関西
            Assert.AreEqual(2, matches[1].GetMatchLength()); // length of 国際
            Assert.AreEqual(2, matches[2].GetMatchLength()); // length of 空港

            List<UserDictionary.UserDictionaryMatch> matches2 = dictionary.FindUserDictionaryMatches("関西国際空港と関西国際空港に行った");
            Assert.AreEqual(6, matches2.Count);
        }

        [TestMethod]
        public void TestIpadicFeatures()
        {
            UserDictionary dictionary;
            using (var stream = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(stream, 9, 7, 0);
            }

            Assert.AreEqual("カスタム名詞,*,*,*,*,*,*,ニホン,*", dictionary.GetAllFeatures(0));
        }

        [TestMethod]
        public void TestJumanDicFeatures()
        {
            UserDictionary dictionary;
            using (var stream = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(stream, 7, 5, 0);
            }
            Assert.AreEqual("カスタム名詞,*,*,*,*,ニホン,*", dictionary.GetAllFeatures(0));
        }

        [TestMethod]
        public void TestNaistJDicFeatures()
        {
            UserDictionary dictionary;
            using (var stream = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(stream, 11, 7, 0);
            }
            // This is a sample naist-jdic entry:
            //
            //   葦登,1358,1358,4975,名詞,一般,*,*,*,*,葦登,ヨシノボリ,ヨシノボリ,,
            //
            // How should we treat the last features in the user dictionary?  They seem empty, but we return * for them...
            Assert.AreEqual("カスタム名詞,*,*,*,*,*,*,ニホン,*,*,*", dictionary.GetAllFeatures(0));
        }

        [TestMethod]
        public void TestUniDicFeatures()
        {
            UserDictionary dictionary;
            using (var stream = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(stream, 13, 7, 0);
            }

            Assert.AreEqual("カスタム名詞,*,*,*,*,*,*,ニホン,*,*,*,*,*", dictionary.GetAllFeatures(0));
        }

        [TestMethod]
        public void TestUniDicExtendedFeatures()
        {
            UserDictionary dictionary;
            using (var stream = GetResource("userdict.txt"))
            {
                dictionary = new UserDictionary(stream, 22, 13, 0);
            }

            Assert.AreEqual("カスタム名詞,*,*,*,*,*,*,*,*,*,*,*,*,ニホン,*,*,*,*,*,*,*,*", dictionary.GetAllFeatures(0));
        }

        [TestMethod]
        public void TestUserDictionaryEntries()
        {
            string userDictionaryEntry = "クロ,クロ,クロ,カスタム名詞";
            var bytes = Encoding.UTF8.GetBytes(userDictionaryEntry);
            using (var stream = new MemoryStream(bytes))
            {
                UserDictionary dictionary = new UserDictionary(stream, 9, 7, 0);
                List<UserDictionary.UserDictionaryMatch> matches = dictionary.FindUserDictionaryMatches("この丘はアクロポリスと呼ばれている");
                Assert.AreEqual(1, matches.Count);
                Assert.AreEqual(5, matches[0].GetMatchStartIndex());
            }
        }

        [TestMethod]
        public void TestOverlappingUserDictionaryEntries()
        {
            string userDictionaryEntries = "" +
                    "クロ,クロ,クロ,カスタム名詞\n" +
                    "アクロ,アクロ,アクロ,カスタム名詞";
            var bytes = Encoding.UTF8.GetBytes(userDictionaryEntries);
            using (var stream = new MemoryStream(bytes))
            {
                UserDictionary dictionary = new UserDictionary(stream, 9, 7, 0);
                List<UserDictionary.UserDictionaryMatch> positions = dictionary.FindUserDictionaryMatches("この丘はアクロポリスと呼ばれている");
                Assert.AreEqual(4, positions[0].GetMatchStartIndex());
                Assert.AreEqual(2, positions.Count);
            }
        }

        private Stream GetResource(string resource)
        {
            string path = @"./Core/Resource/" + resource;            
            return File.OpenRead(path);
        }
    }
}
