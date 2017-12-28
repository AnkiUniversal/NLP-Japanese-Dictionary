using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDictTest.kuromoji.Core
{
    [TestClass]
    public class StringValueMapBufferTest
    {
        [TestMethod]
        public void TestInsertIntoMap()
        {
            SortedDictionary<int, string> input = new SortedDictionary<int, string>();

            input.Add(1, "hello");
            input.Add(2, "日本");
            input.Add(3, "カタカナ");
            input.Add(0, "Bye");

            using (StringValueMapBuffer values = new StringValueMapBuffer(input))
            {

                Assert.AreEqual("Bye", values.Get(0));
                Assert.AreEqual("hello", values.Get(1));
                Assert.AreEqual("日本", values.Get(2));
                Assert.AreEqual("カタカナ", values.Get(3));
            }
        }
    }
}
