using NLPJDict.Kuromoji.Core.HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.FST
{
    public class State
    {
        private List<Arc> arcs;
        public List<Arc> Arcs { get { return arcs; } }
        private bool isFinal = false;
        public bool IsFinal { get { return isFinal; } }

        public bool Visited { get; set; } //for visualization purpose

        private int targetJumpAddress = -1;

        public State()
        {
            this.arcs = new List<Arc>();
        } // INITIAL_CAPACITY not set

        /**
         * Copy constructor
         *
         * @param source  state to copy
         */
        public State(State source)
        {
            this.arcs = source.arcs;
            this.isFinal = source.isFinal;
        }

        public int GetTargetJumpAddress()
        {
            return targetJumpAddress;
        }

        public void SetTargetJumpAddress(int targetJumpAddress)
        {
            this.targetJumpAddress = targetJumpAddress;
        }

        public Arc SetArc(char transition, int output, State toState)
        {
            // Assuming no duplicate transition character
            Arc newArc = new Arc(output, toState, transition);
            arcs.Add(newArc);
            return newArc;
        }

        public void SetArc(char transition, State toState)
        {
            // Assuming no duplicate transition character
            Arc newArc = new Arc(toState);
            newArc.SetLabel(transition);
            arcs.Add(newArc);
        }

        public List<char> GetAllTransitionStrings()
        {
            List<char> retList = new List<char>();

            foreach (Arc arc in arcs)
            {
                retList.Add(arc.GetLabel());
            }
            retList.Sort();
            return retList;
        }

        public void SetFinal()
        {
            this.isFinal = true;
        }

        public Arc FindArc(char transition)
        {
            return BinarySearchArc(transition, 0, this.arcs.Count);
        }

        public Arc BinarySearchArc(char transition, int beginIndice, int endIndice)
        {
            if (beginIndice >= endIndice)
            {
                return null;
            }

            int indice = beginIndice + (endIndice - beginIndice) / 2; // round down

            if (arcs[indice].GetLabel() == transition)
            {
                return arcs[indice];
            }
            else if (arcs[indice].GetLabel() > transition)
            {
                // transition char is placed at the left part of the array
                return BinarySearchArc(transition, beginIndice, indice);
            }
            else if (arcs[indice].GetLabel() < transition)
            {
                // transition char is placed at the right part of the array
                return BinarySearchArc(transition, indice + 1, endIndice);
            }

            return null;
        }

        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType())
                return false;

            State state = (State)o;

            if (isFinal != state.isFinal)
                return false;
            if (arcs != null && state.arcs != null)
            {
                if (!ArrayHelper.AreEqual(arcs, state.arcs))
                    return false;
            }
            else if((arcs != null && state.arcs == null)
                    || (arcs == null && state.arcs != null))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int result = arcs != null ? arcs.GetJavaListHashCode() : 0;

            result = 31 * result + (isFinal ? 1 : 0);
            return result;
        }


    }
}
