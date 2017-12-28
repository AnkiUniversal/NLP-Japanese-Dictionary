using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public class ViterbiFormatter : IDisposable
    {

        private const string BOS_LABEL = "BOS";
        private const string EOS_LABEL = "EOS";
        private const string FONT_NAME = "Helvetica";

        private ConnectionCosts costs;
        private Dictionary<string, ViterbiNode> nodeMap;
        private Dictionary<string, String> bestPathMap;

        private bool foundBOS;

        public void Dispose()
        {
            if(costs != null)
                costs.Dispose();
        }

        public ViterbiFormatter(ConnectionCosts costs)
        {
            this.costs = costs;
            this.nodeMap = new Dictionary<string, ViterbiNode>();
            this.bestPathMap = new Dictionary<string, string>();
        }

        public string Format(ViterbiLattice lattice)
        {
            return Format(lattice, null);
        }

        public string Format(ViterbiLattice lattice, List<ViterbiNode> bestPath)
        {

            InitBestPathMap(bestPath);

            StringBuilder builder = new StringBuilder();
            builder.Append(FormatHeader());
            builder.Append(FormatNodes(lattice));
            builder.Append(FormatTrailer());
            return builder.ToString();

        }

        private void InitBestPathMap(List<ViterbiNode> bestPath)
        {
            this.bestPathMap.Clear();

            if (bestPath == null)
            {
                return;
            }
            for (int i = 0; i < bestPath.Count - 1; i++)
            {
                ViterbiNode from = bestPath[i];
                ViterbiNode to = bestPath[i + 1];

                string fromId = GetNodeId(from);
                string toId = GetNodeId(to);

                Debug.Assert(this.bestPathMap.ContainsKey(fromId) == false);
                Debug.Assert(this.bestPathMap.ContainsValue(toId) == false);
                this.bestPathMap[fromId] = toId;
            }
        }

        private string FormatNodes(ViterbiLattice lattice)
        {
            ViterbiNode[][] startsArray = lattice.StartIndexArr;
            ViterbiNode[][] endsArray = lattice.EndIndexArr;
            this.nodeMap.Clear();
            this.foundBOS = false;

            StringBuilder builder = new StringBuilder();
            for (int i = 1; i < endsArray.Length; i++)
            {
                if (endsArray[i] == null || startsArray[i] == null)
                {
                    continue;
                }
                for (int j = 0; j < endsArray[i].Length; j++)
                {
                    ViterbiNode from = endsArray[i][j];
                    if (from == null)
                    {
                        continue;
                    }
                    builder.Append(FormatNodeIfNew(from));
                    for (int k = 0; k < startsArray[i].Length; k++)
                    {
                        ViterbiNode to = startsArray[i][k];
                        if (to == null)
                        {
                            break;
                        }
                        builder.Append(FormatNodeIfNew(to));
                        builder.Append(FormatEdge(from, to));
                    }
                }
            }
            return builder.ToString();
        }

        private String FormatNodeIfNew(ViterbiNode node)
        {
            String nodeId = GetNodeId(node);
            if (!this.nodeMap.ContainsKey(nodeId))
            {
                this.nodeMap[nodeId] = node;
                return FormatNode(node);
            }
            else
            {
                return "";
            }
        }

        private string FormatHeader()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("digraph viterbi {\n");
            builder.Append("graph [ fontsize=30 labelloc=\"t\" label=\"\" splines=true overlap=false rankdir = \"LR\" ];\n");
            builder.Append("# A2 paper size\n");
            builder.Append("size = \"34.4,16.5\";\n");
            builder.Append("# try to fill paper\n");
            builder.Append("ratio = fill;\n");
            builder.Append("edge [ fontname=\"" + FONT_NAME + "\" fontcolor=\"red\" color=\"#606060\" ]\n");
            builder.Append("node [ style=\"filled\" fillcolor=\"#e8e8f0\" shape=\"Mrecord\" fontname=\"" + FONT_NAME + "\" ]\n");

            return builder.ToString();
        }

        private string FormatTrailer()
        {
            return "}";
        }

        private string FormatEdge(ViterbiNode from, ViterbiNode to)
        {
            if (this.bestPathMap.ContainsKey(GetNodeId(from)) &&
                this.bestPathMap[GetNodeId(from)].Equals(GetNodeId(to)))
            {
                return FormatEdge(from, to, "color=\"#40e050\" fontcolor=\"#40a050\" penwidth=3 fontsize=20 ");

            }
            else
            {
                return FormatEdge(from, to, "");
            }
        }

        private string FormatEdge(ViterbiNode from, ViterbiNode to, String attributes)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetNodeId(from));
            builder.Append(" -> ");
            builder.Append(GetNodeId(to));
            builder.Append(" [ ");
            builder.Append("label=\"");
            builder.Append(GetCost(from, to));
            builder.Append("\"");
            builder.Append(" ");
            builder.Append(attributes);
            builder.Append(" ");
            builder.Append(" ]");
            builder.Append("\n");
            return builder.ToString();
        }

        private string FormatNode(ViterbiNode node)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("\"");
            builder.Append(GetNodeId(node));
            builder.Append("\"");
            builder.Append(" [ ");
            builder.Append("label=");
            builder.Append(FormatNodeLabel(node));
            if (node.Type == ViterbiNode.NodeType.USER)
            {
                builder.Append(" fillcolor=\"#e8f8e8\"");
            }
            else if (node.Type == ViterbiNode.NodeType.UNKNOWN)
            {
                builder.Append(" fillcolor=\"#f8e8f8\"");
            }
            else if (node.Type == ViterbiNode.NodeType.INSERTED)
            {
                builder.Append(" fillcolor=\"#ffe8e8\"");
            }
            builder.Append(" ]");
            return builder.ToString();
        }

        private string FormatNodeLabel(ViterbiNode node)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<<table border=\"0\" cellborder=\"0\">");
            builder.Append("<tr><td>");
            builder.Append(GetNodeLabel(node));
            builder.Append("</td></tr>");
            builder.Append("<tr><td>");
            builder.Append("<font color=\"blue\">");
            builder.Append(node.WordCost);
            builder.Append("</font>");
            builder.Append("</td></tr>");
            builder.Append("</table>>");
            return builder.ToString();
        }

        private string GetNodeId(ViterbiNode node)
        {
            return node.GetHashCode().ToString();
        }

        private string GetNodeLabel(ViterbiNode node)
        {
            if (node.Type == ViterbiNode.NodeType.KNOWN && node.WordId == 0)
            {
                if (this.foundBOS)
                {
                    return EOS_LABEL;
                }
                else
                {
                    this.foundBOS = true;
                    return BOS_LABEL;
                }
            }
            else
            {
                return node.Surface;
            }
        }

        private int GetCost(ViterbiNode from, ViterbiNode to)
        {
            return this.costs.Get(from.LeftId, to.RightId);
        }
    }
}
