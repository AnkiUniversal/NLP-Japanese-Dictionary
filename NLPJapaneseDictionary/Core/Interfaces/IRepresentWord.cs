using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.Interfaces
{
    public interface IRepresentWord
    {
        string Word { get; set; }        
        string Priority { get; set; }
        string Information { get; set; }
    }
}
