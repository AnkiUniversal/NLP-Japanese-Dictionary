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

using NLPJapaneseDictionary.Helpers;
using NLPJDict.HelperClasses;
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
using System.Windows.Media;

namespace NLPJDict.ViewModels
{
    public class OcrOneWordViewModel
    {
        public ObservableCollection<OcrOneWordModel> Words { get; set; }
        public StringBuilder Sentence { get; set; } = new StringBuilder();
        public List<Jocr.TextBlock> TextBlocks { get; private set; }

        public OcrOneWordViewModel()
        {
            Words = new ObservableCollection<OcrOneWordModel>();            
        }

        public OcrOneWordViewModel(List<Jocr.TextBlock> blocks, SolidColorBrush brush = null)
        {
            if (brush == null)
                brush = UIUtilities.Green;

            TextBlocks = blocks;
            List<OcrOneWordModel> entriesModel = new List<OcrOneWordModel>();            
            for (int i = 0; i < blocks.Count; i++)
            {
                entriesModel.Add(new OcrOneWordModel(blocks[i].Text[0], i, brush));
                Sentence.Append(blocks[i].Text[0]);
            }
            Words = new ObservableCollection<OcrOneWordModel>(entriesModel);
        }

        public void AddNewList(List<Jocr.TextBlock> blocks, SolidColorBrush brush = null)
        {
            if (brush == null)
                brush = UIUtilities.Green;

            TextBlocks = blocks;
            Sentence.Clear();
            Words.Clear();
            for (int i = 0; i < blocks.Count; i++)
            {
                Words.Add(new OcrOneWordModel(blocks[i].Text[0], i, brush));
                Sentence.Append(blocks[i].Text[0]);
            }
        }

        public void ChangeWord(char text, int index)
        {
            Words[index].Word = text;
            Sentence[index] = text;
        }
    }
}
