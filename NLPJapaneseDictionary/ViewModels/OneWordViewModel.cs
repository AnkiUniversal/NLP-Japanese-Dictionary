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
