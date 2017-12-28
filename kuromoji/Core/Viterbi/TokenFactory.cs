using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Viterbi
{
    public interface TokenFactory<T> where T : TokenBase
    {

        T CreateToken(int wordId,
                      String surface,
                      ViterbiNode.NodeType type,
                      int position,
                      IDictionary dictionary);
    }
}
