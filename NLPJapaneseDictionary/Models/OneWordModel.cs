using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Models
{
    public class OneWordModel : INotifyPropertyChanged
    {
        private int order;
        public int Order
        {
            get
            {
                return order;
            }
            set
            {
                order = value;
                RaisePropertyChanged("Order");
            }
        }

        private string word;
        public string Word
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

        public OneWordModel(string word, int order = 0)
        {
            this.word = word;
            this.order = order;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
