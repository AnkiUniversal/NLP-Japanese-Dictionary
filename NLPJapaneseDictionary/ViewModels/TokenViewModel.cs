using NLPJDict.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NLPJDict.KuromojiIpadic.Ipadic;

namespace NLPJDict.ViewModels
{
    public class TokenViewModel
    {
        public ObservableCollection<TokenModel> Tokens { get; set; }       
        
        public TokenViewModel()
        {
            Tokens = new ObservableCollection<TokenModel>();
        }

        public TokenViewModel(IEnumerable<Token> tokens)
        {
            List<TokenModel> tokenModels = new List<TokenModel>();
            foreach (var token in tokens)
            {
                tokenModels.Add(new TokenModel(token));
            }
            Tokens = new ObservableCollection<TokenModel>(tokenModels);
        } 

        public void AddTokens(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                Tokens.Add(new TokenModel(token));
            }
        }
    }
}
