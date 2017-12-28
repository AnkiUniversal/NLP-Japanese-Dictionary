using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.FST;

namespace NLPJDictTest.kuromoji.Core.FST
{
    [TestClass]
    public class BuilderTest
    {
        [TestMethod]
        public void TestCreateDictionary()
        {
            string[] inputValues = { "cat", "cats", "dog", "dogs", "friday", "friend", "pydata" };
            int[] outputValues = { 1, 2, 3, 4, 20, 42, 43 };

            using (Builder builder = new Builder())
            {
                builder.Build(inputValues, outputValues);

                for (int i = 0; i < inputValues.Length; i++)
                {
                    Assert.AreEqual(
                        outputValues[i],
                        builder.Transduce(inputValues[i])
                    );
                }
            }
        }
    }
}
