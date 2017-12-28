using NLPJDict.Models;
using NLPJDict.NLPJDictCore.JmdictElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class SenseElementViewModel : INotifyPropertyChanged
    {
        public List<SenseElementModel> senses;
        public List<SenseElementModel> Senses
        {
            get { return senses; }
            set
            {
                senses = value;
                RaisePropertyChanged("Sense");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SenseElementViewModel(List<SenseElement> senseElements)
        {
            List<SenseElementModel> senseModels = new List<SenseElementModel>();
            foreach (var sense in senseElements)
            {
                string onlyForKanji, onlyForRead;
                GetRestrictKanjiAndRead(sense, out onlyForKanji, out onlyForRead);

                senseModels.Add(new SenseElementModel(sense.Order, onlyForKanji, onlyForRead, sense.CrossReference, sense.Antonym,
                                                      sense.PartOfSpeech, sense.Field, sense.Misc, sense.Dialect, sense.Gloss));
            }
            Senses = senseModels;
        }

        public static void GetRestrictKanjiAndRead(SenseElement sense, out string onlyForKanji, out string onlyForRead)
        {
            onlyForKanji = null;
            onlyForRead = null;
            if (sense.OnlyForKanji != null && sense.OnlyForKanji.Length > 0)
                onlyForKanji = String.Join("; ", sense.OnlyForKanji);
            if (sense.OnlyForRead != null && sense.OnlyForRead.Length > 0)
                onlyForRead = String.Join("; ", sense.OnlyForRead);
        }

    }
}
