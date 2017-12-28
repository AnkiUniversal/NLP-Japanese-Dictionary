using NLPJDict.KuromojiIpadic.Ipadic;
using NLPJDict.NLPJDictCore;
using NLPJDict.NLPJDictCore.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NLPJDict.Models
{
    public class WordInformationModel : INotifyPropertyChanged
    {
        public int Index { get; set; }        

        private string surface;
        public string Surface
        {
            get
            {
                return surface;
            }
            set
            {
                surface = value;
                RaisePropertyChanged("surface");
            }
        }

        private string conjugation;
        public string Conjugation
        {
            get
            {
                return conjugation;
            }
            set
            {
                conjugation = value;
                RaisePropertyChanged("Conjugation");
            }
        }

        private string baseForm;
        public string BaseForm
        {
            get
            {
                return baseForm;
            }
            set
            {
                baseForm = value;
                RaisePropertyChanged("BaseForm");
            }
        }

        private string reading;
        public string Reading
        {
            get
            {
                return reading;
            }
            set
            {
                reading = value;
                RaisePropertyChanged("Reading");
            }
        }

        private string pronunciation;
        public string Pronunciation
        {
            get
            {
                return pronunciation;
            }
            set
            {
                pronunciation = value;
                RaisePropertyChanged("Pronunciation");
            }
        }

        private bool isSelectable = true;
        public bool IsSelectable
        {
            get
            {
                return isSelectable;
            }
            set
            {
                isSelectable = value;
                RaisePropertyChanged("IsSelectable");
            }
        }

        private bool? isChecked;
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                RaisePropertyChanged("IsChecked");
            }
        }

        private SolidColorBrush borderColor;
        public SolidColorBrush BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                borderColor = value;
                RaisePropertyChanged("BorderColor");
            }
        }

        public WordInformationModel(string surface, string conjugation, string baseForm, string reading, string pronunciation, bool isSelectable, bool isChecked, SolidColorBrush borderColor)
        {
            this.surface = surface;
            this.conjugation = conjugation;
            this.baseForm = baseForm;
            this.reading = reading;
            this.pronunciation = pronunciation;
            this.isSelectable = isSelectable;
            this.isChecked = isChecked;
            this.borderColor = borderColor;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
