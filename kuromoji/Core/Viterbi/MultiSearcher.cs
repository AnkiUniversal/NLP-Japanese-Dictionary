using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class MultiSearcher : IDisposable
    {
        private readonly ConnectionCosts costs;
        private readonly Mode mode;
        private readonly ViterbiSearcher viterbiSearcher;
        private int baseCost;
        private List<int> pathCosts;
        private Dictionary<ViterbiNode, SidetrackEdge> sidetracks;

        public void Dispose()
        {
            if(costs != null)
                costs.Dispose();
        }

        public MultiSearcher(ConnectionCosts costs, Mode mode, ViterbiSearcher viterbiSearcher)
        {
            this.costs = costs;
            this.mode = mode;
            this.viterbiSearcher = viterbiSearcher;
        }

        /**
         * Get up to maxCount shortest paths with cost at most OPT + costSlack, where OPT is the optimal solution. The results are ordered in ascending order by cost.
         *
         * @param lattice  an instance of ViterbiLattice prosecced by a ViterbiSearcher
         * @param maxCount  the maximum number of results
         * @param costSlack  the maximum cost slack of a path
         * @return  the shortest paths and their costs
         */
        public MultiSearchResult GetShortestPaths(ViterbiLattice lattice, int maxCount, int costSlack)
        {
            pathCosts = new List<int>();
            sidetracks = new Dictionary<ViterbiNode, MultiSearcher.SidetrackEdge>();
            MultiSearchResult multiSearchResult = new MultiSearchResult();
            BuildSidetracks(lattice);
            ViterbiNode eos = lattice.EndIndexArr[0][0];
            baseCost = eos.PathCost;
            List<SidetrackEdge> paths = GetPaths(eos, maxCount, costSlack);
            int i = 0;
            foreach (SidetrackEdge path in paths)
            {
                LinkedList<ViterbiNode> nodes = GeneratePath(eos, path);
                multiSearchResult.Add(nodes, pathCosts[i]);
                i += 1;
            }
            return multiSearchResult;
        }

        private LinkedList<ViterbiNode> GeneratePath(ViterbiNode eos, SidetrackEdge sidetrackEdge)
        {
            LinkedList<ViterbiNode> result = new LinkedList<ViterbiNode>();
            ViterbiNode node = eos;
            result.AddLast(node);
            while (node.LeftNode != null)
            {
                ViterbiNode leftNode = node.LeftNode;
                if (sidetrackEdge != null && sidetrackEdge.Head == node)
                {
                    leftNode = sidetrackEdge.Tail;
                    sidetrackEdge = sidetrackEdge.Parent;
                }
                node = leftNode;
                result.AddFirst(node);
            }
            return result;
        }

        private List<SidetrackEdge> GetPaths(ViterbiNode eos, int maxCount, int costSlack)
        {
            List<SidetrackEdge> result = new List<MultiSearcher.SidetrackEdge>();
            result.Add(null);
            pathCosts.Add(baseCost);
            PriorityQueue<SidetrackEdge> sidetrackHeap = new PriorityQueue<SidetrackEdge>();

            SidetrackEdge sideTrackEdge = sidetracks[eos];
            while (sideTrackEdge != null)
            {
                sidetrackHeap.Add(sideTrackEdge);
                sideTrackEdge = sideTrackEdge.NextOption;
            }

            for (int i = 1; i < maxCount; i++)
            {
                if (sidetrackHeap.IsEmpty())
                {
                    break;
                }

                sideTrackEdge = sidetrackHeap.Poll();
                if (sideTrackEdge.Cost > costSlack)
                {
                    break;
                }
                result.Add(sideTrackEdge);
                pathCosts.Add(baseCost + sideTrackEdge.Cost);
                SidetrackEdge nextSidetrack = sidetracks[sideTrackEdge.Tail];

                while (nextSidetrack != null)
                {
                    SidetrackEdge next = new SidetrackEdge(nextSidetrack.Cost, nextSidetrack.Tail, nextSidetrack.Head);
                    next.Parent = sideTrackEdge;
                    sidetrackHeap.Add(next);
                    nextSidetrack = nextSidetrack.NextOption;
                }
            }
            return result;
        }

        private void BuildSidetracks(ViterbiLattice lattice)
        {
            ViterbiNode[][] startIndexArr = lattice.StartIndexArr;
            ViterbiNode[][] endIndexArr = lattice.EndIndexArr;

            for (int i = 1; i < startIndexArr.Length; i++)
            {
                if (startIndexArr[i] == null || endIndexArr[i] == null)
                {
                    continue;
                }

                foreach (ViterbiNode node in startIndexArr[i])
                {
                    if (node == null)
                    {
                        break;
                    }

                    BuildSidetracksForNode(endIndexArr[i], node);
                }
            }
        }

        private void BuildSidetracksForNode(ViterbiNode[] leftNodes, ViterbiNode node)
        {
            int backwardConnectionId = node.LeftId;
            int wordCost = node.WordCost;

            List<SidetrackEdge> sidetrackEdges = new List<SidetrackEdge>();
            SidetrackEdge nextOption;
            sidetracks.TryGetValue(node.LeftNode, out nextOption);

            foreach (ViterbiNode leftNode in leftNodes)
            {
                if (leftNode == null)
                {
                    break;
                }

                if (leftNode.Type == ViterbiNode.NodeType.KNOWN && leftNode.WordId == -1)
                { // Ignore BOS
                    continue;
                }

                int sideTrackCost = leftNode.PathCost - node.PathCost + wordCost + costs.Get(leftNode.RightId, backwardConnectionId);
                if (mode == Mode.SEARCH || mode == Mode.EXTENDED)
                {
                    sideTrackCost += viterbiSearcher.GetPenaltyCost(node);
                }

                if (leftNode != node.LeftNode)
                {
                    sidetrackEdges.Add(new SidetrackEdge(sideTrackCost, leftNode, node));
                }
            }

            if (sidetrackEdges.Count == 0)
            {
                sidetracks[node] = nextOption;
            }
            else
            {
                for (int i = 0; i < sidetrackEdges.Count - 1; i++)
                {
                    sidetrackEdges[i].NextOption = sidetrackEdges[i + 1];
                }
                sidetrackEdges[sidetrackEdges.Count - 1].NextOption = nextOption;
                sidetracks[node] = sidetrackEdges[0];
            }
        }

        public class SidetrackEdge : IComparable<SidetrackEdge>
        {
            public int Cost { get; private set; }
            public ViterbiNode Tail { get; private set; }
            public ViterbiNode Head { get; private set; }
            public SidetrackEdge NextOption { get; set; }

            private SidetrackEdge parent;
            public SidetrackEdge Parent
            {
                get { return parent; }
                set
                {
                    parent = value;
                    Cost += parent.Cost;
                }
            }

            public SidetrackEdge(int cost, ViterbiNode tail, ViterbiNode head)
            {
                this.Cost = cost;
                this.Tail = tail;
                this.Head = head;
                NextOption = null;
                parent = null;
            }

            public int CompareTo(SidetrackEdge o)
            {
                return Cost - o.Cost;
            }
        }
    }
}
