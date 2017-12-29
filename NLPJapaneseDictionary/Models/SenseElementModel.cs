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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NLPJapaneseDictionary.Models
{
    public class SenseElementModel : INotifyPropertyChanged
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

        private string onlyForKanji;
        public string OnlyForKanji
        {
            get
            {
                return onlyForKanji;
            }
            set
            {
                onlyForKanji = value;
                RaisePropertyChanged("OnlyForKanji");
            }
        }

        private string onlyForRead;
        public string OnlyForRead
        {
            get
            {
                return onlyForRead;
            }
            set
            {
                onlyForRead = value;
                RaisePropertyChanged("OnlyForRead");
            }
        }

        private string crossReference;
        public string CrossReference
        {
            get
            {
                return crossReference;
            }
            set
            {
                crossReference = value;
                RaisePropertyChanged("CrossReference");
            }
        }

        private string antonym;
        public string Antonym
        {
            get
            {
                return antonym;
            }
            set
            {
                antonym = value;
                RaisePropertyChanged("Antonym");
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
                RaisePropertyChanged("PartOfSpeech");
            }
        }

        private string field;
        public string Field
        {
            get
            {
                return field;
            }
            set
            {
                field = value;
                RaisePropertyChanged("Field");
            }
        }

        private string misc;
        public string Misc
        {
            get
            {
                return misc;
            }
            set
            {
                misc = value;
                RaisePropertyChanged("Misc");
            }
        }

        private string dialgect;
        public string Dialgect
        {
            get
            {
                return dialgect;
            }
            set
            {
                dialgect = value;
                RaisePropertyChanged("Dialgect");
            }
        }

        private string gloss;
        public string Gloss
        {
            get
            {
                return gloss;
            }
            set
            {
                gloss = value;
                RaisePropertyChanged("Gloss");
            }
        }

        public Visibility isDialgectVisible;
        public Visibility IsDialgectVisible
        {
            get
            {
                return isDialgectVisible;
            }
            set
            {
                isDialgectVisible = value;
                RaisePropertyChanged("IsDialgectVisible");
            }
        }

        public Visibility isAntonymVisible;
        public Visibility IsAntonymVisible
        {
            get
            {
                return isAntonymVisible;
            }
            set
            {
                isAntonymVisible = value;
                RaisePropertyChanged("IsAntonymVisible");
            }
        }

        public Visibility isFieldVisible;
        public Visibility IsFieldVisible
        {
            get
            {
                return isFieldVisible;
            }
            set
            {
                isFieldVisible = value;
                RaisePropertyChanged("IsFieldVisible");
            }
        }

        public Visibility isMiscVisible;
        public Visibility IsMiscVisible
        {
            get
            {
                return isMiscVisible;
            }
            set
            {
                isMiscVisible = value;
                RaisePropertyChanged("IsMiscVisible");
            }
        }

        public Visibility isCrossReferenceVisible;
        public Visibility IsCrossReferenceVisible
        {
            get
            {
                return isCrossReferenceVisible;
            }
            set
            {
                isCrossReferenceVisible = value;
                RaisePropertyChanged("IsCrossReferenceVisible");
            }
        }

        public Visibility isOnlyForReadVisible;
        public Visibility IsOnlyForReadVisible
        {
            get
            {
                return isOnlyForReadVisible;
            }
            set
            {
                isOnlyForReadVisible = value;
                RaisePropertyChanged("IsOnlyForReadVisible");
            }
        }

        public Visibility isOnlyForKanjiVisible;
        public Visibility IsOnlyForKanjiVisible
        {
            get
            {
                return isOnlyForKanjiVisible;
            }
            set
            {
                isOnlyForKanjiVisible = value;
                RaisePropertyChanged("IsOnlyForKanjiVisible");
            }
        }

        public SenseElementModel(int order, string onlyForKanji, string onlyForRead, string crossReference, string antonym,
                                string partOfSpeech, string field, string misc, string dialgect, string gloss)
        {
            this.order = order;
            this.onlyForKanji = onlyForKanji;
            this.onlyForRead = onlyForRead;
            this.crossReference = crossReference;
            this.antonym = antonym;
            this.partOfSpeech = partOfSpeech;
            this.field = field;
            this.misc = misc;
            this.dialgect = dialgect;
            this.gloss = gloss;

            isDialgectVisible = dialgect != null ? Visibility.Visible : Visibility.Collapsed;
            isAntonymVisible = antonym != null ? Visibility.Visible : Visibility.Collapsed;
            isFieldVisible = field != null ? Visibility.Visible : Visibility.Collapsed;
            isMiscVisible = misc != null ? Visibility.Visible : Visibility.Collapsed;
            isCrossReferenceVisible = crossReference != null ? Visibility.Visible : Visibility.Collapsed;
            isOnlyForReadVisible = onlyForRead != null ? Visibility.Visible : Visibility.Collapsed;
            isOnlyForKanjiVisible = onlyForKanji != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
