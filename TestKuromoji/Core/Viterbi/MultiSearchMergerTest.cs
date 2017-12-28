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
using NLPJDict.Kuromoji.Core.Viterbi;

namespace NLPJDictTest.kuromoji.Core.Viterbi
{
    [TestClass]
    public class MultiSearchMergerTest
    {
        [TestMethod]
        public void TestMerger()
        {
            int maxCount = 3;
            int costSlack = 8;
            MultiSearchMerger merger = new MultiSearchMerger(maxCount, costSlack);
            List<MultiSearchResult> results = new List<MultiSearchResult>();

            string[][] surfaces1 = { new string[] { "a", "b" },
                                     new string[] { "c", "d" },
                                     new string[] { "e", "f" } };
            int[] costs1 = { 1, 2, 3 };
            results.Add(MakeResult(surfaces1, costs1));

            string[][] surfaces2 = { new string[] { "a", "b" },
                                     new string[] { "c", "d" } };
            int[] costs2 = { 1, 2 };
            results.Add(MakeResult(surfaces2, costs2));

            MultiSearchResult mergedResult = merger.Merge(results);
            Assert.AreEqual(3, mergedResult.Count);
            Assert.AreEqual(2, mergedResult.GetCost(0));
            Assert.AreEqual(3, mergedResult.GetCost(1));
            Assert.AreEqual(3, mergedResult.GetCost(2));
            Assert.AreEqual("a b a b", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(0)));
            Assert.AreEqual("c d a b", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(1)));
            Assert.AreEqual("a b c d", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(2)));
        }

        [TestMethod]
        public void TestMergerTooFew()
        {
            int maxCount = 5;
            int costSlack = 3;
            MultiSearchMerger merger = new MultiSearchMerger(maxCount, costSlack);
            List<MultiSearchResult> results = new List<MultiSearchResult>();

            string[][] surfaces1 = { new string[] { "a", "b" },
                                     new string[] { "c", "d" },
                                     new string[] { "e", "f" } };
            int[] costs1 = { 1, 2, 5 };
            results.Add(MakeResult(surfaces1, costs1));

            string[][] surfaces2 = { new string[] { "a", "b" },
                                     new string[] { "c", "d" } };
            int[] costs2 = { 1, 2 };
            results.Add(MakeResult(surfaces2, costs2));

            string[][] surfaces3 = { new string[] { "a", "b" } };
            int[] costs3 = { 5 };
            results.Add(MakeResult(surfaces3, costs3));

            MultiSearchResult mergedResult = merger.Merge(results);
            Assert.AreEqual(4, mergedResult.Count);
            Assert.AreEqual(7, mergedResult.GetCost(0));
            Assert.AreEqual(8, mergedResult.GetCost(1));
            Assert.AreEqual(8, mergedResult.GetCost(2));
            Assert.AreEqual(9, mergedResult.GetCost(3));
            Assert.AreEqual("a b a b a b", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(0)));
            Assert.AreEqual("c d a b a b", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(1)));
            Assert.AreEqual("a b c d a b", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(2)));
            Assert.AreEqual("c d c d a b", GetSpaceSeparatedTokens(mergedResult.GetTokenizedResult(3)));
        }

        private MultiSearchResult MakeResult(string[][] surfaces, int[] cost)
        {
            MultiSearchResult ret = new MultiSearchResult();
            for (int i = 0; i < surfaces.Length; i++)
            {
                ret.Add(MakeNodes(surfaces[i]), cost[i]);
            }
            return ret;
        }

        private LinkedList<ViterbiNode> MakeNodes(string[] surfaces)
        {
            LinkedList<ViterbiNode> ret = new LinkedList<ViterbiNode>();
            foreach (string s in surfaces)
            {
                ret.AddLast(new ViterbiNode(0, s, 0, 0, 0, 0, ViterbiNode.NodeType.KNOWN));
            }
            return ret;
        }

        private string GetSpaceSeparatedTokens(LinkedList<ViterbiNode> nodes)
        {
            var list = nodes.ToList();
            if (nodes.Count == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(list[0].Surface);
            for (int i = 1; i < nodes.Count; i++)
            {
                sb.Append(" ");
                sb.Append(list[i].Surface);
            }
            return sb.ToString();
        }
    }
}
