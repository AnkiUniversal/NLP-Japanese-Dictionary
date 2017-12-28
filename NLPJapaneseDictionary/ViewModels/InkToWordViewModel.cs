using NLPJDict.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class InkToWordViewModel
    {
        public ObservableCollection<InkToWord> Words { get; set; }

        public InkToWordViewModel()
        {
            Words = new ObservableCollection<InkToWord>();
        }

        public void Clear()
        {
            Words.Clear();
        }

        public void AddNewWord(List<InkToWord> words)
        {
            Words.Clear();
            foreach(var word in words)
            {
                Words.Add(word);
            }
        }

        public void AddNewWord(List<string> words)
        {
            Words.Clear();
            foreach (var word in words)
            {
                Words.Add(new InkToWord(word));
            }
        }

        public void AddNewWord(List<char> words)
        {
            Words.Clear();
            foreach (var word in words)
            {                
                Words.Add(new InkToWord(word.ToString()));
            }
        }
    }
}
