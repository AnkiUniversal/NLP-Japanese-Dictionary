/**
 * Copyright © 2017-2018 Anki Universal Team.
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
