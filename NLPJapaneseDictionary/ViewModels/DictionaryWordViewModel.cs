using NLPJDict.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.NLPJDictCore;
using System.Text.RegularExpressions;

namespace NLPJDict.ViewModels
{
    public class DictionaryWordViewModel
    {        
        public ObservableCollection<DictionaryWordModel> DictionaryWords { get; set; }

        public DictionaryWordViewModel()
        {
            DictionaryWords = new ObservableCollection<DictionaryWordModel>();
        }

        public void AddNewWordList(List<JmdictEntity> entries)
        {
            DictionaryWords.Clear();
            bool isHaveBorder = false;
            foreach (var entry in entries)
            {
                DictionaryWords.Add(new DictionaryWordModel(new JmdictWord(entry), isHaveBorder));
                isHaveBorder = true;
            }
        }
    }
}
