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

namespace NLPJDictTest.kuromoji.Core.Dict
{
    [TestClass]
    public class InsertedDictionaryTest
    {
        [TestMethod]
        public void TestFeatureSize()
        {
            InsertedDictionary dictionary1 = new InsertedDictionary(9);
            InsertedDictionary dictionary2 = new InsertedDictionary(5);

            Assert.AreEqual("*,*,*,*,*,*,*,*,*", dictionary1.GetAllFeatures(0));
            Assert.AreEqual("*,*,*,*,*", dictionary2.GetAllFeatures(0));

            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "*", "*", "*", "*", "*", "*", "*", "*", "*" },
                dictionary1.GetAllFeaturesArray(0)
            ));
            Assert.IsTrue(NLPJDictTest.TestUtils.IsArrayEqual(
                new string[] { "*", "*", "*", "*", "*" },
                dictionary2.GetAllFeaturesArray(0)
            ));
        }
    }
}
