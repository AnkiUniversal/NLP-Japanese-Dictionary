using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Models
{
    public class InkToWordList
    {
        public ObservableCollection<InkToWord> WordList { get; set; }

        public InkToWordList(IEnumerable<InkToWord> list)
        {
            WordList = new ObservableCollection<InkToWord>(list);
        }
    }
}
