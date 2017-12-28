using NLPJDict.Models;
using NLPJDict.NLPJDictCore.DatabaseTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class KanjiViewModel
    {
        public ObservableCollection<KanjiModel> Kanjis { get; set; }

        public KanjiViewModel(List<KanjiDict> entries)
        {
            List<KanjiModel> entriesModel = new List<KanjiModel>();
            foreach (var entry in entries)
            {
                entriesModel.Add(new KanjiModel(entry));
            }
            Kanjis = new ObservableCollection<KanjiModel>(entriesModel);
        }
    }
}
