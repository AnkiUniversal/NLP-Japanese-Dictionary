using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class ViterbiLattice
    {

        static readonly string BOS = "BOS";
        static readonly string EOS = "EOS";

        private readonly int dimension;
        private readonly ViterbiNode[][] startIndexArr;
        private readonly ViterbiNode[][] endIndexArr;
        private readonly int[] startSizeArr;
        private readonly int[] endSizeArr;

        public ViterbiNode[][] StartIndexArr { get { return startIndexArr; } }
        public ViterbiNode[][] EndIndexArr { get { return endIndexArr; } }
        public int[] StartSizeArr { get { return startSizeArr; } }
        public int[] EndSizeArr { get { return endSizeArr; } }

        public ViterbiLattice(int dimension)
        {
            this.dimension = dimension;
            startIndexArr = new ViterbiNode[dimension][];
            endIndexArr = new ViterbiNode[dimension][];
            startSizeArr = new int[dimension];
            endSizeArr = new int[dimension];
        }

        public void AddBos()
        {
            ViterbiNode bosNode = new ViterbiNode(-1, BOS, 0, 0, 0, -1, ViterbiNode.NodeType.KNOWN);
            AddNode(bosNode, 0, 1);
        }

        public void AddEos()
        {
            ViterbiNode eosNode = new ViterbiNode(-1, EOS, 0, 0, 0, dimension - 1, ViterbiNode.NodeType.KNOWN);
            AddNode(eosNode, dimension - 1, 0);
        }

        public void AddNode(ViterbiNode node, int start, int end)
        {
            AddNodeToArray(node, start, StartIndexArr, StartSizeArr);
            AddNodeToArray(node, end, EndIndexArr, EndSizeArr);
        }

        private void AddNodeToArray(ViterbiNode node, int index, ViterbiNode[][] arr, int[] sizes)
        {
            int count = sizes[index];

            ExpandIfNeeded(index, arr, count);

            arr[index][count] = node;
            sizes[index] = count + 1;
        }

        private void ExpandIfNeeded(int index, ViterbiNode[][] arr, int count)
        {
            if (count == 0)
            {
                arr[index] = new ViterbiNode[10];
            }

            if (arr[index].Length <= count)
            {
                arr[index] = ExtendArray(arr[index]);
            }
        }

        private ViterbiNode[] ExtendArray(ViterbiNode[] array)
        {
            ViterbiNode[] newArray = new ViterbiNode[array.Length * 2];
            Array.Copy(array, 0, newArray, 0, array.Length);
            return newArray;
        }

        public bool TokenEndsWhereCurrentTokenStarts(int startIndex)
        {
            return EndSizeArr[startIndex + 1] != 0;
        }
    }
}
