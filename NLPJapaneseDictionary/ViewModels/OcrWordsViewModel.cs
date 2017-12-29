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

using NLPJapaneseDictionary.Models;
using NLPJapaneseDictionary.Core.DatabaseTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.ViewModels
{
    class OcrWordsViewModel
    {
        public int Index { get; private set; }
        public ObservableCollection<OcrWordsModel> Words { get; set; }

        public OcrWordsViewModel()
        {
            Words = new ObservableCollection<OcrWordsModel>();
        }

        public OcrWordsViewModel(Jocr.TextBlock blocks, int index)
        {
            Index = index;
            List<OcrWordsModel> entriesModel = new List<OcrWordsModel>();
            for (int i = 0; i < blocks.Text.Count; i++)            
                entriesModel.Add(new OcrWordsModel(blocks.Text[i]));
            
            Words = new ObservableCollection<OcrWordsModel>(entriesModel);
        }

        public void AddNewList(Jocr.TextBlock blocks, int index)
        {
            Index = index;
            Words.Clear();
            for (int i = 0; i < blocks.Text.Count; i++)
                Words.Add(new OcrWordsModel(blocks.Text[i]));
        }
    }
}
