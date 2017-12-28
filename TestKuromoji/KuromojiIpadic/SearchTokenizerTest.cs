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
using System.Text.RegularExpressions;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class SearchTokenizerTest
    {
        private static Tokenizer tokenizer;

        [TestCleanup]
        public void Clean()
        {
            tokenizer.Dispose();
        }

        [TestInitialize]
        public void BeforeClass()
        {
            var builder = new Tokenizer.Builder(TestUtils.AbsoluteIpadicResourcePath);
            builder.Mode = Mode.SEARCH;
            tokenizer = new Tokenizer(builder);
        }

        [TestMethod]
        public void TestCompoundSplitting()
        {
            AssertSegmentation("search-segmentation-tests.txt");
        }

        public void AssertSegmentation(string testFilename)
        {
            using (var stream = File.OpenRead(TestUtils.AbsoluteIpadicResourcePath + testFilename))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    // Remove comments
                    string line = Regex.Replace(reader.ReadLine(), "#.*$", "");
                    // Skip empty lines or comment lines
                    if (String.IsNullOrWhiteSpace(line.Trim()))
                    {
                        continue;
                    }

                    string[] fields = line.Split(new string[] { "\t" }, 2, StringSplitOptions.RemoveEmptyEntries);
                    string text = fields[0];
                    List<string> expectedSurfaces = fields[1].SplitSpace().ToList();

                    AssertSegmentation(text, expectedSurfaces);
                }
            }
        }

        public void AssertSegmentation(String text, List<String> expectedSurfaces)
        {
            List<Token> tokens = tokenizer.Tokenize(text);

            Assert.AreEqual(expectedSurfaces.Count, tokens.Count, "Input: " + text);

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedSurfaces[i], tokens[i].Surface);
            }
        }

        private Stream GetResourceAsStream(string fileName)
        {
            return File.OpenRead(TestUtils.AbsoluteIpadicResourcePath + fileName);
        }
    }
}
