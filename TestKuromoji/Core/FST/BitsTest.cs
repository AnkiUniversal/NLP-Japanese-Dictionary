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
    public class BitsTest
    {
        [TestMethod]
        public void TestBits()
        {
            byte[] bytes = new byte[] { 90, unchecked((byte)-1), 0, 0, 0, unchecked((byte)-112) , 0, 0, 0, 6, 0, 5, 1 };
            Assert.AreEqual(1, Bits.GetByte(bytes, bytes.Length - 1));
            Assert.AreEqual(5, Bits.GetShort(bytes, bytes.Length - (1 + 1)));
            Assert.AreEqual(6, Bits.GetInt(bytes, bytes.Length - (1 + 1 + 2)));
            Assert.AreEqual(144, Bits.GetInt(bytes, bytes.Length - (1 + 1 + 2 + 4)));
        }
    }
}
