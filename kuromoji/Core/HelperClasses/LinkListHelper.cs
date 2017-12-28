using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public static class LinkListHelper
    {
        public static void AddAll<T>(this LinkedList<T> destination, LinkedList<T> source)
        {
            foreach(var node in source)
            {
                destination.AddLast(node);
            }
        }
    }
}
