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
