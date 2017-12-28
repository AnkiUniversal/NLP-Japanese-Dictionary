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
using System.Windows.Media;

namespace NLPJDict.Models
{
    public class OcrOneWordModel : INotifyPropertyChanged
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

        public int Index { get; private set; }

        private SolidColorBrush brush;
        public SolidColorBrush Brush
        {
            get
            {
                return brush;
            }
            set
            {
                brush = value;
                RaisePropertyChanged("Brush");
            }
        }

        public OcrOneWordModel(char text, int index, SolidColorBrush brush)
        {
            word = text;
            Index = index;
            this.brush = brush;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
