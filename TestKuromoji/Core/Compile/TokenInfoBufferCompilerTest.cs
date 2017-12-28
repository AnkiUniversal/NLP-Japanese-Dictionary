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
    public class TokenInfoBufferCompilerTest
    {

        private static  string tokenInfoFile = TestUtils.CompiledPath + Path.DirectorySeparatorChar + "kuromoji-tokeinfo-buffer-.bin";

        [TestMethod]
        public void TestReadAndWriteFromBuffer()
        {
            List<short> shorts = new List<short>();

            for (int i = 0; i < 10; i++) {
                shorts.Add((short)i);
            }

            MemoryStreamWrapper buffer = new MemoryStreamWrapper(new MemoryStream(shorts.Count * 2 + 2));

            buffer.WriteInt16((short)shorts.Count);

            foreach (short s in shorts)
            {
                buffer.WriteInt16(s);
            }

            buffer.Stream.Position = 0;

            short count = buffer.ReadInt16();

            List<short> readShorts = new List<short>();

            for (int i = 0; i < count; i++) {
                readShorts.Add(buffer.ReadInt16());
            }

            for (int i = 0; i < shorts.Count; i++) {
                Assert.AreEqual(readShorts[i], shorts[i]);
            }
        }

        [TestMethod]
        public void TestReadAndLookUpTokenInfo()
        {
            List<short> tokenInfo = new List<short>();
            List<int> features = new List<int>();

            short[] tokenInfos = new short[3];
            tokenInfos[0] = 1;
            tokenInfos[1] = 2;
            tokenInfos[2] = 3;

            int[] featureInfos = new int[2];
            featureInfos[0] = 73;
            featureInfos[1] = 99;

            tokenInfo.Add((short)1);
            tokenInfo.Add((short)2);
            tokenInfo.Add((short)3);

            features.Add(73);
            features.Add(99);

            BufferEntry entry = new BufferEntry();
            entry.TokenInfo = tokenInfo;
            entry.Features = features;

            entry.TokenInfos = tokenInfos;
            entry.FeatureInfos = featureInfos;

            List<BufferEntry> bufferEntries = new List<BufferEntry>();
            bufferEntries.Add(entry);
            
            using (TokenInfoBufferCompiler compiler = new TokenInfoBufferCompiler(bufferEntries))
            {
                using (var outputStream = File.Create(tokenInfoFile))
                {
                    compiler.Compile(outputStream);
                }
            }

            using (var inputStream = File.OpenRead(tokenInfoFile))
            {
                using (TokenInfoBuffer tokenInfoBuffer2 = new TokenInfoBuffer(inputStream))
                {
                    Assert.AreEqual(99, tokenInfoBuffer2.LookupFeature(0, 1));
                    Assert.AreEqual(73, tokenInfoBuffer2.LookupFeature(0, 0));
                }
            }
        }

        [TestMethod]
        public void TestCompleteLookUp()
        {
            Dictionary<int, string> resultMap = new Dictionary<int, string>();

            resultMap[73] = "hello";
            resultMap[42] =  "今日は";
            resultMap[99] = "素敵な世界";

            List<short> tokenInfo = new List<short>();
            List<int> features = new List<int>();

            tokenInfo.Add((short)1);
            tokenInfo.Add((short)2);
            tokenInfo.Add((short)3);

            features.Add(73);
            features.Add(99);

            BufferEntry entry = new BufferEntry();
            entry.TokenInfo = tokenInfo;
            entry.Features = features;

            List<BufferEntry> bufferEntries = new List<BufferEntry>();
            bufferEntries.Add(entry);
            
            using (var outStream = File.Create(tokenInfoFile))
            {
                using (TokenInfoBufferCompiler compiler = new TokenInfoBufferCompiler(bufferEntries))
                {
                    compiler.Compile(outStream);
                }
            }

            using (var inStream = File.OpenRead(tokenInfoFile))
            {
                using (TokenInfoBuffer tokenInfoBuffer = new TokenInfoBuffer(inStream))
                {

                    BufferEntry result = tokenInfoBuffer.LookupEntry(0);

                    Assert.AreEqual("hello", resultMap[result.FeatureInfos[0]]);
                    Assert.AreEqual("素敵な世界", resultMap[result.FeatureInfos[1]]);
                }
            }
        }
    }
}
