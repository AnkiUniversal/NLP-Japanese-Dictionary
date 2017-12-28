using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Models
{
    public class ExampleSentenceModel : INotifyPropertyChanged
    {
        private string japanese;
        public string Japanese
        {
            get
            {
                return japanese;
            }
            set
            {
                japanese = value;
                RaisePropertyChanged("Japanese");
            }
        }

        private string english;
        public string English
        {
            get
            {
                return english;
            }
            set
            {
                english = value;
                RaisePropertyChanged("English");
            }
        }

        public ExampleSentenceModel(string japanese, string english)
        {
            this.japanese = japanese;
            this.english = english;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
