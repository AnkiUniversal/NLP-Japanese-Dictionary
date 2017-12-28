using NLPJDict.HelperClasses;
using NLPJDict.NLPJDictCore;
using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJDict.Models
{
    public class OcrWordsModel : INotifyPropertyChanged
    {
        private char word;
        public char Word
        {
            get
            {
                return word;
            }
            set
            {
                word = value;
                RaisePropertyChanged("Word");
            }
        }

        public OcrWordsModel(char text)
        {
            word = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
