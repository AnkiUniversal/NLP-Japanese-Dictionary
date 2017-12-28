

namespace NLPJDict.Kuromoji.Interfaces
{
    public interface IToken
    {
        NLPJDict.Kuromoji.Core.Viterbi.ViterbiNode.NodeType NodeType { get; }
        string Surface { get;  }
        string ConjugationType { get; }
        string ConjugationForm { get; }
        string BaseForm { get;  }
        string Reading { get;  }
        string Pronunciation { get; }
        string PartOfSpeech { get;  }
    }
}
