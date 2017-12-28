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
