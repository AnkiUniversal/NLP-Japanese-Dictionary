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

using NLPJapaneseDictionary.KuromojiIpadic.Ipadic;
using NLPJapaneseDictionary.Core;
using NLPJapaneseDictionary.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NLPJapaneseDictionary.Models
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
