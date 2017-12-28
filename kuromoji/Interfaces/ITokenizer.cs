using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Interfaces
{
    public interface ITokenizer<T> where T :IToken
    {
        List<T> Tokenize(string text);
    }
}
