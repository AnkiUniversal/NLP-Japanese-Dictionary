/**
 * Copyright © 2010-2017 Atilika Inc. and contributors (see CONTRIBUTORS.md)
 * 
 * Modifications copyright (C) 2017 - 2018 Anki Universal Team <ankiuniversal@gmail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.FST
{
    public class FSTFormatter
    {

        private const string FONT_NAME = "Helvetica";

        public string Format(Builder builder, string outFileAbsoultePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FormatHeader());
            sb.Append(FormatHashedNodes(builder));
            sb.Append(FormatTrailer());

            try
            {
                using (var fw = File.Open(outFileAbsoultePath, FileMode.Create))
                using (var writer = new StreamWriter(fw))
                {
                    writer.Write(sb.ToString());
                }
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
            }

            return "";
        }

        private string FormatHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("digraph fst {\n");
            sb.Append("graph [ fontsize=30 labelloc=\"t\" label=\"\" splines=true overlap=false rankdir = \"LR\" ];\n");
            sb.Append("# A2 paper size\n");
            sb.Append("size = \"34.4,16.5\";\n");
            sb.Append("# try to fill paper\n");
            sb.Append("ratio = fill;\n");
            sb.Append("edge [ fontname=\"" + FONT_NAME + "\" fontcolor=\"red\" color=\"#606060\" ]\n");
            sb.Append("node [ peripheries=2 style=\"filled\" fillcolor=\"#e8e8f0\" shape=\"Mrecord\" fontsize=40 fontname=\"" + FONT_NAME + "\" ]\n");

            return sb.ToString();
        }

        private string FormatTrailer()
        {
            return "}";
        }

        private string FormatHashedNodes(Builder builder)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FormatState(builder.GetStartState())); // format the start state

            List<State> stateArrayList = new List<State>();
            stateArrayList.Add(builder.GetStartState());

            while (!(stateArrayList.Count == 0))
            {
                State state = stateArrayList[0];
                if (state.Arcs.Count == 0 || state.Visited)
                {
                    stateArrayList.RemoveAt(0);
                    continue;
                }
                foreach (char transition in state.GetAllTransitionStrings())
                {
                    Arc arc = state.FindArc(transition);
                    State toState = arc.GetDestination();
                    stateArrayList.Add(toState);

                    if (toState.IsFinal)
                    {
                        sb.Append(FormatFinalState(toState));
                    }
                    else
                    {
                        sb.Append(FormatState(toState));
                    }
                    int arcOutput = arc.GetOutput();
                    sb.Append(FormatEdge(state, toState, transition, arcOutput.ToString(), "fontsize=40"));
                }
                state.Visited = true;
                stateArrayList.RemoveAt(0);
            }
            return sb.ToString();

        }

        private string FormatState(State state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(GetNodeId(state));
            sb.Append("\"");
            sb.Append(" [ ");
            sb.Append("label=");
            sb.Append(FormatStateLabel(state));
            sb.Append(" ]");
            return sb.ToString();
        }

        private string FormatFinalState(State state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\"");
            sb.Append(GetNodeId(state));
            sb.Append("\"");
            sb.Append(" [ ");
            sb.Append("fillcolor=pink ");
            sb.Append("label=");
            sb.Append(FormatFinalStateLabel(state));
            sb.Append(" ]");
            return sb.ToString();
        }

        private string FormatStateLabel(State state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<<table border=\"0\" cellborder=\"0\">");
            sb.Append("<tr><td>");
            sb.Append("Node");
            sb.Append("</td></tr>");
            sb.Append("<tr><td>");
            sb.Append("<font color=\"blue\">");
            sb.Append("Normal State");
            sb.Append("</font>");
            sb.Append("</td></tr>");
            sb.Append("</table>>");
            return sb.ToString();
        }

        private string FormatFinalStateLabel(State state)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<<table border=\"0\" cellborder=\"0\">");
            sb.Append("<tr><td>");
            sb.Append("Node");
            sb.Append("</td></tr>");
            sb.Append("<tr><td>");
            sb.Append("<font color=\"blue\">");
            sb.Append("Accepting State");
            sb.Append("</font>");
            sb.Append("</td></tr>");
            sb.Append("</table>>");
            return sb.ToString();
        }


        private string FormatEdge(State from, State to, char transition, String output, String attributes)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetNodeId(from));
            sb.Append(" -> ");
            sb.Append(GetNodeId(to));
            sb.Append(" [ ");
            sb.Append("label=\"");
            sb.Append(transition + "/");
            sb.Append(output);
            sb.Append("\"");
            sb.Append(" ");
            sb.Append(attributes);
            sb.Append(" ");
            sb.Append(" ]");
            sb.Append("\n");
            return sb.ToString();
        }

        private string GetNodeId(State node)
        {
            return node.GetHashCode().ToString();
        }

    }
}
