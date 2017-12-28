using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class ViterbiNode
    {

        public enum NodeType
        {
            KNOWN,
            UNKNOWN,
            USER,
            INSERTED
        }

        private readonly int wordId;
        public int WordId { get { return wordId; } }

        private readonly string surface;
        public string Surface { get { return surface; } }

        private readonly int leftId;
        public int LeftId { get { return leftId; } }

        private readonly int rightId;
        public int RightId { get { return rightId; } }

        /**
         * word cost for this node
         */
        private readonly int wordCost;
        public int WordCost { get { return wordCost; } }

        /**
         * minimum path cost found thus far
         */
        public int PathCost { get; set; }
        public ViterbiNode LeftNode { get; set; }

        private readonly NodeType type;
        public NodeType Type { get { return type; } }

        private readonly int startIndex;
        public int StartIndex { get { return startIndex; } }

        public ViterbiNode(int wordId, String surface, int leftId, int rightId, int wordCost, int startIndex, NodeType type)
        {
            this.wordId = wordId;
            this.surface = surface;
            this.leftId = leftId;
            this.rightId = rightId;
            this.wordCost = wordCost;
            this.startIndex = startIndex;
            this.type = type;
        }

        public ViterbiNode(int wordId, string word, IDictionary dictionary, int startIndex, NodeType type)
        : this(wordId, word, dictionary.GetLeftId(wordId), dictionary.GetRightId(wordId), dictionary.GetWordCost(wordId), startIndex, type)
        {
        }

    }
}
