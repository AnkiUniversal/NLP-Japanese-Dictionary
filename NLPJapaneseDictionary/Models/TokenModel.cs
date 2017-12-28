using NLPJDict.Kuromoji.Interfaces;
using NLPJDict.KuromojiIpadic.Ipadic;
using NLPJDict.NLPJDictCore.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Models
{
    public class TokenModel : INotifyPropertyChanged
    {
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
                RaisePropertyChanged("Surface");
            }
        }

        private string conjugationType;
        public string ConjugationType
        {
            get
            {
                return conjugationType;
            }
            set
            {
                conjugationType = value;
                RaisePropertyChanged("ConjugationType");
            }
        }

        private string conjugationForm;
        public string ConjugationForm
        {
            get
            {
                return conjugationForm;
            }
            set
            {
                conjugationForm = value;
                RaisePropertyChanged("ConjugationForm");
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

        private string partOfSpeech;
        public string PartOfSpeech
        {
            get
            {
                return partOfSpeech;
            }
            set
            {
                partOfSpeech = value;
                RaisePropertyChanged("PartOfSpeech1");
            }
        }

        public TokenModel(IToken token)
        {
            surface = token.Surface;
            conjugationType = token.ConjugationType;
            conjugationForm = token.ConjugationForm;
            baseForm = token.BaseForm;
            reading = token.Reading;
            pronunciation = token.Pronunciation;
            partOfSpeech = token.PartOfSpeech;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
