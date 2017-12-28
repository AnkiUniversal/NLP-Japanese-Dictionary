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
using NLPJDict.Kuromoji.Core.HelperClasses;

namespace NLPJDictTest.kuromoji.Core.Compile
{
    [TestClass]
    public class ConnectionCostsCompilerTest
    {
        private static ConnectionCosts connectionCosts;
        private static string costFile = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "./kuromoji-connectioncosts-.bin";

        [TestCleanup]
        public void Clean()
        {
            connectionCosts.Dispose();
        }

        [TestInitialize]
        public void SetUp()
        {            
            string costs = "" +
            "3 3\n" +
            "0 0 1\n" +
            "0 1 2\n" +
            "0 2 3\n" +
            "1 0 4\n" +
            "1 1 5\n" +
            "1 2 6\n" +
            "2 0 7\n" +
            "2 1 8\n" +
            "2 2 9\n";

            using (ConnectionCostsCompiler compiler = new ConnectionCostsCompiler())
            using (var outputStream = File.Create(costFile))
            {
                var bytes = Encoding.UTF8.GetBytes(costs);
                Stream inputStream = new MemoryStream(bytes);
                compiler.ReadCosts(inputStream);
                compiler.Compile(outputStream);
            }

            using (var readStream = File.OpenRead(costFile))
            using (var reader = new BinaryReader(readStream))
            {
                int size = reader.ReadRawInt32();
                var costsBuffer = new MemoryStreamWrapper(ByteBufferIO.Read(readStream));
                connectionCosts = new ConnectionCosts(size, costsBuffer);
            }
        }

        [TestMethod]
        public void TestCosts()
        {
            int cost = 1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.AreEqual(cost++, connectionCosts.Get(i, j));
                }
            }
        }
    }
}
