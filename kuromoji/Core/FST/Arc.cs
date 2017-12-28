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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.FST
{
    public class Arc
    {
        private char label;

        private int output = 0;

        private State destination;

        public Arc(int output, State destination, char label)
        {
            this.output = output;
            this.destination = destination;
            this.label = label;
        }

        public Arc(State Destination)
        {
            this.destination = Destination;
        }

        public State GetDestination()
        {
            return this.destination;
        }

        public int GetOutput()
        {
            return this.output;
        }

        public char GetLabel()
        {
            return this.label;
        }

        public void SetOutput(int output)
        {
            this.output = output;
        }

        public void SetLabel(char label)
        {
            this.label = label;
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType())
                return false;

            Arc arc = (Arc)o;

            if (label != arc.label)
                return false;
            if (output != arc.output)
                return false;
            if (destination != null)
            {
                if (!destination.Equals(arc.destination))
                    return false;
            }
            else
            {
                if (arc.destination != null)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = (int)label;
            result = 31 * result + output;
            result = 31 * result + (destination != null ? destination.GetHashCode() : 0);
            return result;
        }
    }
}