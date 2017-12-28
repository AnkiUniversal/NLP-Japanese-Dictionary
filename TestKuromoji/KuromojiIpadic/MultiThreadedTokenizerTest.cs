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
using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.FST;
using NLPJDict.Kuromoji.Core.util;
using NLPJDict.KuromojiIpadic.Ipadic;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class MultiThreadedTokenizerTest
    {
        [TestMethod]
        public void TestMultiThreadedBocchan()
        {
            TestUtils.AssertMultiThreadedTokenizedStreamEquals(
                5,
                10,
                TestUtils.AbsoluteIpadicResourcePath + "bocchan-ipadic-features.txt",
                TestUtils.AbsoluteIpadicResourcePath + "bocchan.txt",
                new Tokenizer(TestUtils.AbsoluteIpadicResourcePath)
            );
        }

        [TestMethod]
        public void TestMultiThreadedUserDictionary()
        {
            var filePath = "./Core/Resource/userdict.txt";
            using (var stream = File.OpenRead(filePath))
            {
                using (var builder = new Tokenizer.Builder(TestUtils.AbsoluteIpadicResourcePath))
                {
                    builder.LoadUserDictionary(stream);

                    TestUtils.AssertMultiThreadedTokenizedStreamEquals(
                    5,
                    10,
                    TestUtils.AbsoluteIpadicResourcePath + "jawikisentences-ipadic-features.txt",
                    TestUtils.AbsoluteIpadicResourcePath + "jawikisentences.txt",
                    new Tokenizer(builder)
                    );
                }
            }
        }
    }
}
