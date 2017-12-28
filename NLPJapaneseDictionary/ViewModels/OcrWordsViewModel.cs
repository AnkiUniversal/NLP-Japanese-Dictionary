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
