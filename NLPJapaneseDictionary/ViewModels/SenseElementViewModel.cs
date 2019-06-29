/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using NLPJapaneseDictionary.Models;
using NLPJapaneseDictionary.Core.JmdictElements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.ViewModels
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
                                                      sense.PartOfSpeech, sense.Field, sense.Misc, sense.Dialect, sense.Gloss,sense.Gloss_vi));
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
