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
using NLPJDict.NLPJDictCore.DatabaseTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class OneWordViewModel 
    {
        public ObservableCollection<OneWordModel> Words { get; set; }

        public OneWordViewModel()
        {
            Words = new ObservableCollection<OneWordModel>();
        }

        public OneWordViewModel(List<KanjiDict> kanjis)
        {            
            List<OneWordModel> entriesModel = new List<OneWordModel>();
            for(int i = 0; i < kanjis.Count; i++)
            {
                entriesModel.Add(new OneWordModel(kanjis[i].KanjiElement, i));
            }
            Words = new ObservableCollection<OneWordModel>(entriesModel);
        }

        public OneWordViewModel(string[] words)
        {
            List<OneWordModel> entriesModel = new List<OneWordModel>();
            for (int i = 0; i < words.Length; i++)
            {
                entriesModel.Add(new OneWordModel(words[i], i));
            }
            Words = new ObservableCollection<OneWordModel>(entriesModel);
        }
    }
}
