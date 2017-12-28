using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.kuromojiIpadic
{
    [TestClass]
    public class TestCompiler
    {
        private string inputDir = @"./KuromojiIpadic/IpadicResource";
        private string outputDir = "./IpadicCompiled";

        /// <summary>
        /// Use this method to compile/create Ipadic dict databases (.bin files) for Kuromoji 
        /// </summary>
        [TestMethod, Ignore]
        public void TestDictionaryCompiler()
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
            Directory.CreateDirectory(outputDir);

            NLPJDict.KuromojiIpadic.Compile.DictionaryCompiler.StartCompile(new string[] { inputDir, outputDir, "euc-jp" }, CodePagesEncodingProvider.Instance);
            Assert.IsTrue(true);
        }
    }
}
