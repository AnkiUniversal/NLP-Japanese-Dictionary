
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;

namespace NLPJDictTest.kuromoji.Core
{
    public static class CommonCornerCasesTest
    {
        public static void TestPunctuation<T>(TokenizerBase<T> tokenizer) where T : TokenBase
        {
            string gerryNoHanaNoHanashi = "僕の鼻はちょっと\r\n長いよ。";
            var expecter = new string[] { "僕", "の", "鼻", "は", "ちょっと", "\r", "\n", "長い", "よ", "。" };
            var actual = tokenizer.Tokenize(gerryNoHanaNoHanashi);
            TestUtils.AssertTokenSurfacesEquals(expecter, actual.ToArray());
        }
    }
}
