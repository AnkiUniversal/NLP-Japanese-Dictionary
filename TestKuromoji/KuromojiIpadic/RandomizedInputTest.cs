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
using NLPJapaneseDictionary.Kuromoji.Core.Compile;
using NLPJapaneseDictionary.Kuromoji.Core.IO;
using NLPJapaneseDictionary.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJapaneseDictionary.Kuromoji.Core;
using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core.Buffer;
using NLPJapaneseDictionary.Kuromoji.Core.FST;
using NLPJapaneseDictionary.Kuromoji.Core.util;
using NLPJapaneseDictionary.KuromojiIpadic.Ipadic;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class RandomizedInputTest
    {
        private const int LENGTH = 512;
        private const int LOOP = 10;
        private static  Tokenizer tokenizer = new Tokenizer(TestUtils.AbsoluteIpadicResourcePath);

        [ClassCleanup]
        public static void Clean()
        {
            tokenizer.Dispose();
        }

        [TestMethod]
        public void TestRandomizedUnicodeInput()
        {
            for (int i = 0; i < LOOP; i++)
            {
                TestUtils.AssertCanTokenizeString(RandomString(5), tokenizer);
            }
        }

        //[TestMethod]
        //public void TestRandomizedRealisticUnicodeInput()
        //{
        //    for (int i = 0; i < LOOP; i++)
        //    {
        //        TestUtils.AssertCanTokenizeString(randomRealisticUnicodeOfLength(LENGTH), tokenizer);
        //    }
        //}

        //[TestMethod]
        //public void TestRandomizedAsciiInput()
        //{
        //    for (int i = 0; i < LOOP; i++)
        //    {
        //        TestUtils.AssertCanTokenizeString(randomAsciiOfLength(LENGTH), tokenizer);
        //    }
        //}

        [TestMethod]
        public void TestRandomizedUnicodeInputMultiTokenize()
        {
            for (int i = 0; i < LOOP; i++)
            {
                Random rand = new Random();
                TestUtils.AssertCanMultiTokenizeString(RandomString(LENGTH), rand.Next(998) + 2, rand.Next(100000), tokenizer);
            }
        }

        //[TestMethod]
        //public void TestRandomizedRealisticUnicodeInputMultiTokenize()
        //{
        //    for (int i = 0; i < LOOP; i++)
        //    {
        //        Random rand = new Random();
        //        TestUtils.AssertCanMultiTokenizeString(RandomString(LENGTH), rand.Next(998) + 2, rand.Next(100000), tokenizer);
        //    }
        //}

        //[TestMethod]
        //public void TestRandomizedAsciiInputMultiTokenize()
        //{
        //    for (int i = 0; i < LOOP; i++)
        //    {
        //        Random rand = new Random();
        //        TestUtils.AssertCanMultiTokenizeString(randomAsciiOfLength(LENGTH), rand.nextInt(998) + 2, rand.nextInt(100000), tokenizer);
        //    }
        //}

        private string RandomString(int size, bool lowerCase = false)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }
}
