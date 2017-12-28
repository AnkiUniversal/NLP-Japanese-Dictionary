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

namespace NLPJDictTest.kuromoji.Core.Compile
{
    [TestClass]
    public class WordIdMapCompilerTest
    {

        [TestMethod]
        public void TestGrowableArray()
        {
            WordIdMapCompiler.GrowableIntArray array = new WordIdMapCompiler.GrowableIntArray(5);
            array.Set(3, 1);
            Assert.AreEqual("[0, 0, 0, 1]", array.GetArray().Array2String());
            array.Set(0, 2);
            array.Set(10, 3);
            Assert.AreEqual("[2, 0, 0, 1, 0, 0, 0, 0, 0, 0, 3]", array.GetArray().Array2String());
        }

        [TestMethod]
        public void TestCompiler()
        {
            WordIdMapCompiler compiler = new WordIdMapCompiler();
            compiler.AddMapping(3, 1);
            compiler.AddMapping(3, 2);
            compiler.AddMapping(3, 3);
            compiler.AddMapping(10, 0);

            var fileName = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "kuromoji-wordid-.bin";

            using (var output = File.Create(fileName))
            {
                compiler.Write(output);
            }

            using (var input = File.OpenRead(fileName))
            {
                WordIdMap wordIds = new WordIdMap(input);
                Assert.AreEqual("[1, 2, 3]", wordIds.LookUp(3).Array2String());
                Assert.AreEqual("[0]", wordIds.LookUp(10).Array2String());
                Assert.AreEqual("[]", wordIds.LookUp(1).Array2String());
            }
        }
    }

}
