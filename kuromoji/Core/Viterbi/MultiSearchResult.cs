using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class MultiSearchResult
    {
        private List<LinkedList<ViterbiNode>> tokenizedResults;
        private List<int> costs;

        public int Count { get { return costs.Count; } }

        public MultiSearchResult()
        {
            tokenizedResults = new List<LinkedList<ViterbiNode>>();
            costs = new List<int>();
        }

        public void Add(LinkedList<ViterbiNode> tokenizedResult, int cost)
        {
            tokenizedResults.Add(tokenizedResult);
            costs.Add(cost);
        }

        public LinkedList<ViterbiNode> GetTokenizedResult(int index)
        {
            return tokenizedResults[index];
        }

        public List<LinkedList<ViterbiNode>> GetTokenizedResultsList()
        {
            return tokenizedResults;
        }

        public int GetCost(int index)
        {
            return costs[index];
        }
       
    }
}
