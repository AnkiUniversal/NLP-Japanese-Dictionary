using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.FST;

namespace NLPJDictTest.kuromoji.Core.FST
{
    [TestClass]
    public class FSTTest
    {
        [TestMethod]
        public void TestFST()
        {
            string[] inputValues = {
                "brats", "cat", "dog", "dogs", "rat",
        };

            int[] outputValues = { 1, 3, 5, 7, 11 };

            using (Builder builder = new Builder())
            {
                builder.Build(inputValues, outputValues);

                for (int i = 0; i < inputValues.Length; i++)
                {
                    Assert.AreEqual(outputValues[i], builder.Transduce(inputValues[i]));
                }

                using (Compiler compiledFST = builder.GetCompiler())
                {
                    NLPJDict.Kuromoji.Core.FST.FST fst = new NLPJDict.Kuromoji.Core.FST.FST(compiledFST.GetBytes());

                    Assert.AreEqual(0, fst.Lookup("brat")); // Prefix match
                    Assert.AreEqual(1, fst.Lookup("brats"));
                    Assert.AreEqual(3, fst.Lookup("cat"));
                    Assert.AreEqual(5, fst.Lookup("dog"));
                    Assert.AreEqual(7, fst.Lookup("dogs"));
                    Assert.AreEqual(11, fst.Lookup("rat"));
                    Assert.AreEqual(-1, fst.Lookup("rats")); // No match
                }
            }
        }
    }
}
