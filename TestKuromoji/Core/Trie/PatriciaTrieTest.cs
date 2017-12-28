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

namespace NLPJDictTest.kuromoji.Core.Trie
{
    [TestClass]
    public class PatriciaTrieTest
    {

        [TestMethod]
        public void TestRomaji()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            trie["a"] = "a";
            trie["b"] = "b";
            trie["ab"] = "ab";
            trie["bac"] = "bac";
            Assert.AreEqual("a", trie["a"]);
            Assert.AreEqual("bac", trie["bac"]);
            Assert.AreEqual("b", trie["b"]);
            Assert.AreEqual("ab", trie["ab"]);
            Assert.IsNull(trie["nonexistant"]);
        }

        [TestMethod]
        public void TestJapanese()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            trie["寿司"] = "sushi";
            trie["刺身"] = "sashimi";
            Assert.AreEqual("sushi", trie["寿司"]);
            Assert.AreEqual("sashimi", trie["刺身"]);
        }

        [TestMethod]
        public void TestNull()
        {
            try
            {
                PatriciaTrie<String> trie = new PatriciaTrie<string>();
                trie["null"] = null;
                Assert.AreEqual(null, trie["null"]);
                trie[null] = "null"; // Throws NullPointerException

                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestRandom()
        {
            // Generate random strings
            List<string> randoms = new List<string>();
            for (int i = 0; i < 10000; i++)
            {
                randoms.Add(System.Guid.NewGuid().ToString());
            }
            // Insert them
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            foreach (string random in randoms)
            {
                trie[random] = random;
            }
            // Get and test them
            foreach (string random in randoms)
            {
                Assert.AreEqual(random, trie[random]);
                Assert.IsTrue(trie.ContainsKey(random));
            }
        }

        [TestMethod]
        public void TestPutAll()
        {
            // Generate random strings
            Dictionary<string, string> randoms = new Dictionary<string, string>();
            for (int i = 0; i < 1000; i++)
            {
                string random = System.Guid.NewGuid().ToString();
                randoms[random] = random;
            }
            // Insert them
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            trie.PutAll(randoms);

            // Get and test them
            foreach (KeyValuePair<string, string> random in randoms)
            {
                Assert.AreEqual(random.Value, trie[random.Key]);
                Assert.IsTrue(trie.ContainsKey(random.Key));
            }
        }

        [TestMethod]
        public void TestLongString()
        {
            String longMovieTitle = "マルキ・ド・サドの演出のもとにシャラントン精神病院患者たちによって演じられたジャン＝ポール・マラーの迫害と暗殺";

            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            trie[longMovieTitle] = "found it";

            Assert.AreEqual("found it", trie[longMovieTitle]);
        }

        [TestMethod]
        public void TestEmpty()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            Assert.IsTrue(trie.IsEmpty());
            trie["hello"] = "world";
            Assert.IsFalse(trie.IsEmpty());
        }

        [TestMethod]
        public void TestEmptyInsert()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            Assert.IsTrue(trie.IsEmpty());
            trie[""] = "i am empty bottle of beer!";
            Assert.IsFalse(trie.IsEmpty());
            Assert.AreEqual("i am empty bottle of beer!", trie[""]);
            trie[""] = "...and i'm an empty bottle of sake";
            Assert.AreEqual("...and i'm an empty bottle of sake", trie[""]);
        }

        [TestMethod]
        public void TestClear()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            Assert.IsTrue(trie.IsEmpty());
            Assert.AreEqual(0, trie.Count);
            trie["hello"] = "world";
            trie["world"] = "hello";
            Assert.IsFalse(trie.IsEmpty());
            trie.Clear();
            Assert.IsTrue(trie.IsEmpty());
            Assert.AreEqual(0, trie.Count);
        }

        [TestMethod]
        public void TestNaiveCollections()
        {
            PatriciaTrie<String> trie = new PatriciaTrie<string>();
            trie["寿司"] = "sushi";
            trie["刺身"] = "sashimi";
            trie["そば"] = "soba";
            trie["ラーメン"] = "ramen";
            // Test keys
            Assert.AreEqual(4, trie.Keys.Count);
            Assert.IsTrue(trie.Keys.ContainsAll(new string[] { "寿司", "そば", "ラーメン", "刺身" }));
            // Test values
            Assert.AreEqual(4, trie.Values.Count);
            Assert.IsTrue(trie.Values.ContainsAll(new string[] { "sushi", "soba", "ramen", "sashimi" }));
        }

        [TestMethod]
        public void TestEscapeChars()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            trie["new"] = "no error";
            Assert.IsFalse(trie.ContainsKeyPrefix("new\na"));
            Assert.IsFalse(trie.ContainsKeyPrefix("\n"));
            Assert.IsFalse(trie.ContainsKeyPrefix("\t"));
        }

        [TestMethod]
        public void TestPrefix()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            string[] tokyoPlaces = new string[]{
            "Hachiōji",
            "Tachikawa",
            "Musashino",
            "Mitaka",
            "Ōme",
            "Fuchū",
            "Akishima",
            "Chōfu",
            "Machida",
            "Koganei",
            "Kodaira",
            "Hino",
            "Higashimurayama",
            "Kokubunji",
            "Kunitachi",
            "Fussa",
            "Komae",
            "Higashiyamato",
            "Kiyose",
            "Higashikurume",
            "Musashimurayama",
            "Tama",
            "Inagi",
            "Hamura",
            "Akiruno",
            "Nishitōkyō"
        };
            for (int i = 0; i < tokyoPlaces.Length; i++)
            {
                trie[tokyoPlaces[i]] = tokyoPlaces[i];
            }

            // Prefixes of Kodaira
            Assert.IsTrue(trie.ContainsKeyPrefix("K"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Ko"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Kod"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Koda"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Kodai"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Kodair"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Kodaira"));
            Assert.IsFalse(trie.ContainsKeyPrefix("Kodaira "));
            Assert.IsFalse(trie.ContainsKeyPrefix("Kodaira  "));
            Assert.IsTrue(trie["Kodaira"] != null);

            // Prefixes of Fussa
            Assert.IsFalse(trie.ContainsKeyPrefix("fu"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Fu"));
            Assert.IsTrue(trie.ContainsKeyPrefix("Fus"));
        }

        [TestMethod]
        public void TestTextScan()
        {
            PatriciaTrie<string> trie = new PatriciaTrie<string>();
            String[] terms = new String[]{
            "お寿司", "sushi",
            "美味しい", "tasty",
            "日本", "japan",
            "だと思います", "i think",
            "料理", "food",
            "日本料理", "japanese food",
            "一番", "first and foremost",
        };
            for (int i = 0; i < terms.Length; i += 2)
            {
                trie[terms[i]] = terms[i + 1];
            }

            string text = "日本料理の中で、一番美味しいのはお寿司だと思います。すぐ日本に帰りたいです。";
            StringBuilder builder = new StringBuilder();

            int startIndex = 0;
            while (startIndex < text.Length)
            {
                int matchLength = 0;
                while (trie.ContainsKeyPrefix(text.Substring(startIndex, matchLength + 1)))
                {
                    matchLength++;
                }
                if (matchLength > 0)
                {
                    String match = text.Substring(startIndex, matchLength);
                    builder.Append("[");
                    builder.Append(match);
                    builder.Append("|");
                    builder.Append(trie[match]);
                    builder.Append("]");
                    startIndex += matchLength;
                }
                else
                {
                    builder.Append(text[startIndex]);
                    startIndex++;
                }
            }
            Assert.AreEqual("[日本料理|japanese food]の中で、[一番|first and foremost][美味しい|tasty]のは[お寿司|sushi][だと思います|i think]。すぐ[日本|japan]に帰りたいです。", builder.ToString());
        }

        [TestMethod]
        public void TestMultiThreadedTrie()
        {
            int numThreads = 10;
            int perThreadRuns = 50000;
            int keySetSize = 1000;

            List<Task> tasks = new List<Task>();
            List<string> randoms = new List<string>();

            PatriciaTrie<int?> trie = new PatriciaTrie<int?>();

            for (int i = 0; i < keySetSize; i++)
            {
                String random = System.Guid.NewGuid().ToString();
                randoms.Add(random);
                trie[random] = i;
            }

            for (int i = 0; i < numThreads; i++)
            {
                var task = new Task(() =>
                {
                    for (int run = 0; run < perThreadRuns; run++)
                    {
                        var rand = new Random();
                        int randomIndex = rand.Next(randoms.Count - 1);
                        string random = randoms[randomIndex];

                        // Test retrieve
                        Assert.AreEqual(randomIndex, (int)trie[random]);

                        int randomPrefixLength = rand.Next(random.Length - 1);

                        // Test random prefix length prefix match
                        Assert.IsTrue(trie.ContainsKeyPrefix(random.Substring(0, randomPrefixLength)));
                    }
                });
                tasks.Add(task);
                task.Start();
            }

            foreach (var task in tasks)
            {
                task.Wait();
            }

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestSimpleKey()
        {
            PatriciaTrie<string>.KeyMapper<string> keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            string key = "abc";

            // a = U+0061 = 0000 0000 0110 0001
            Assert.IsFalse(keyMapper.IsSet(0, key));
            Assert.IsFalse(keyMapper.IsSet(1, key));
            Assert.IsFalse(keyMapper.IsSet(2, key));
            Assert.IsFalse(keyMapper.IsSet(3, key));

            Assert.IsFalse(keyMapper.IsSet(4, key));
            Assert.IsFalse(keyMapper.IsSet(5, key));
            Assert.IsFalse(keyMapper.IsSet(6, key));
            Assert.IsFalse(keyMapper.IsSet(7, key));

            Assert.IsFalse(keyMapper.IsSet(8, key));
            Assert.IsTrue(keyMapper.IsSet(9, key));
            Assert.IsTrue(keyMapper.IsSet(10, key));
            Assert.IsFalse(keyMapper.IsSet(11, key));

            Assert.IsFalse(keyMapper.IsSet(12, key));
            Assert.IsFalse(keyMapper.IsSet(13, key));
            Assert.IsFalse(keyMapper.IsSet(14, key));
            Assert.IsTrue(keyMapper.IsSet(15, key));

            // b = U+0062 = 0000 0000 0110 0010
            Assert.IsFalse(keyMapper.IsSet(16, key));
            Assert.IsFalse(keyMapper.IsSet(17, key));
            Assert.IsFalse(keyMapper.IsSet(18, key));
            Assert.IsFalse(keyMapper.IsSet(19, key));

            Assert.IsFalse(keyMapper.IsSet(20, key));
            Assert.IsFalse(keyMapper.IsSet(21, key));
            Assert.IsFalse(keyMapper.IsSet(22, key));
            Assert.IsFalse(keyMapper.IsSet(23, key));

            Assert.IsFalse(keyMapper.IsSet(24, key));
            Assert.IsTrue(keyMapper.IsSet(25, key));
            Assert.IsTrue(keyMapper.IsSet(26, key));
            Assert.IsFalse(keyMapper.IsSet(27, key));

            Assert.IsFalse(keyMapper.IsSet(28, key));
            Assert.IsFalse(keyMapper.IsSet(29, key));
            Assert.IsTrue(keyMapper.IsSet(30, key));
            Assert.IsFalse(keyMapper.IsSet(31, key));

            // c = U+0063 = 0000 0000 0110 0011
            Assert.IsFalse(keyMapper.IsSet(32, key));
            Assert.IsFalse(keyMapper.IsSet(33, key));
            Assert.IsFalse(keyMapper.IsSet(34, key));
            Assert.IsFalse(keyMapper.IsSet(35, key));

            Assert.IsFalse(keyMapper.IsSet(36, key));
            Assert.IsFalse(keyMapper.IsSet(37, key));
            Assert.IsFalse(keyMapper.IsSet(38, key));
            Assert.IsFalse(keyMapper.IsSet(39, key));

            Assert.IsFalse(keyMapper.IsSet(40, key));
            Assert.IsTrue(keyMapper.IsSet(41, key));
            Assert.IsTrue(keyMapper.IsSet(42, key));
            Assert.IsFalse(keyMapper.IsSet(43, key));

            Assert.IsFalse(keyMapper.IsSet(44, key));
            Assert.IsFalse(keyMapper.IsSet(45, key));
            Assert.IsTrue(keyMapper.IsSet(46, key));
            Assert.IsTrue(keyMapper.IsSet(47, key));
        }

        [TestMethod]
        public void TestNullKeyMap()
        {
            PatriciaTrie<string>.KeyMapper<String> keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            Assert.IsFalse(keyMapper.IsSet(0, null));
            Assert.IsFalse(keyMapper.IsSet(100, null));
            Assert.IsFalse(keyMapper.IsSet(1000, null));
        }

        [TestMethod]
        public void TestEmptyKeyMap()
        {
            PatriciaTrie<string>.KeyMapper<string> keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            // Note: this is a special case handled in PatriciaTrie
            Assert.IsTrue(keyMapper.IsSet(0, ""));
            Assert.IsTrue(keyMapper.IsSet(100, ""));
            Assert.IsTrue(keyMapper.IsSet(1000, ""));
        }

        [TestMethod]
        public void TestOverflowBit()
        {
            PatriciaTrie<string>.KeyMapper<String> keyMapper = new PatriciaTrie<string>.StringKeyMapper();
            String key = "a";

            // a = U+0061 = 0000 0000 0110 0001
            Assert.IsFalse(keyMapper.IsSet(0, key));
            Assert.IsFalse(keyMapper.IsSet(1, key));
            Assert.IsFalse(keyMapper.IsSet(2, key));
            Assert.IsFalse(keyMapper.IsSet(3, key));

            Assert.IsFalse(keyMapper.IsSet(4, key));
            Assert.IsFalse(keyMapper.IsSet(5, key));
            Assert.IsFalse(keyMapper.IsSet(6, key));
            Assert.IsFalse(keyMapper.IsSet(7, key));

            Assert.IsFalse(keyMapper.IsSet(8, key));
            Assert.IsTrue(keyMapper.IsSet(9, key));
            Assert.IsTrue(keyMapper.IsSet(10, key));
            Assert.IsFalse(keyMapper.IsSet(11, key));

            Assert.IsFalse(keyMapper.IsSet(12, key));
            Assert.IsFalse(keyMapper.IsSet(13, key));
            Assert.IsFalse(keyMapper.IsSet(14, key));
            Assert.IsTrue(keyMapper.IsSet(15, key));

            // Asking for overflow bits should return 1
            Assert.IsTrue(keyMapper.IsSet(16, key));
            Assert.IsTrue(keyMapper.IsSet(17, key));
            Assert.IsTrue(keyMapper.IsSet(100, key));
        }
    }
}
