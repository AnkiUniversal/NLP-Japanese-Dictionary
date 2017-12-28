using NLPJDict.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class InkToWordListViewModel
    {
        public ObservableCollection<InkToWordList> InkToWordViewModel { get; set; }

        public InkToWordListViewModel(IEnumerable<InkToWordList> list)
        {
            InkToWordViewModel = new ObservableCollection<InkToWordList>(list);
        }
    }
}
