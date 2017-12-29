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

using NLPJDict.HelperClasses;
using NLPJDict.Core;
using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace NLPJDict.Models
{
    public class DictionaryWordModel : INotifyPropertyChanged
    {
        public int EntrySequence { get; private set; }

        private string firsWord;
        public string FirstWord
        {
            get
            {
                return firsWord;
            }
            set
            {
                firsWord = value;
                RaisePropertyChanged("FirstWord");
            }
        }

        private string firsWordInformation;
        public string FirstWordInformation
        {
            get
            {
                return firsWordInformation;
            }
            set
            {
                firsWordInformation = value;
                RaisePropertyChanged("FirstWordInformation");
            }
        }

        private bool isHaveExample;
        public bool IsHaveExample
        {
            get
            {
                return isHaveExample;
            }
            set
            {
                isHaveExample = value;
                RaisePropertyChanged("IsHaveExample");
            }
        }
        
        private string conjungation;
        public string Conjungation
        {
            get
            {
                return conjungation;
            }
            set
            {
                conjungation = value;
                RaisePropertyChanged("Conjungation");
            }
        }

        private string priority;
        public string Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
                RaisePropertyChanged("Priority");
            }
        }

        private string readingApplyToAll;
        public string ReadingApplyToAll
        {
            get
            {
                return readingApplyToAll;
            }
            set
            {
                readingApplyToAll = value;
                RaisePropertyChanged("ReadingApplyToAll");
            }
        }

        private string restrictReading;
        public string RestrictReading
        {
            get
            {
                return restrictReading;
            }
            set
            {
                restrictReading = value;
                RaisePropertyChanged("RestrictReading");
            }
        }

        private string otherKanjiForms;
        public string OtherKanjiForms
        {
            get
            {
                return otherKanjiForms;
            }
            set
            {
                otherKanjiForms = value;
                RaisePropertyChanged("OtherKanjiForms");
            }
        }        

        private SenseElementRichTextViewModel sense;
        public SenseElementRichTextViewModel Sense
        {
            get
            {
                return sense;
            }
            set
            {
                sense = value;
                RaisePropertyChanged("Sense");
            }
        }

        private OneWordViewModel kanjiList;
        public OneWordViewModel KanjiList
        {
            get
            {
                return kanjiList;
            }
            set
            {
                kanjiList = value;
                RaisePropertyChanged("KanjiList");
            }
        }


        private Visibility isRestrictReadingVisible;
        public Visibility IsRestrictReadingVisible
        {
            get
            {
                return isRestrictReadingVisible;
            }
            set
            {
                isRestrictReadingVisible = value;
                RaisePropertyChanged("IsRestrictReadingVisible");
            }
        }

        private Visibility isOtherKanjiFormsVisible;
        public Visibility IsOtherKanjiFormsVisible
        {
            get
            {
                return isOtherKanjiFormsVisible;
            }
            set
            {
                isOtherKanjiFormsVisible = value;
                RaisePropertyChanged("IsOtherKanjiFormsVisible");
            }
        }

        private Visibility isAllReadingVisible;
        public Visibility IsAllReadingVisible
        {
            get
            {
                return isAllReadingVisible;
            }
            set
            {
                isAllReadingVisible = value;
                RaisePropertyChanged("IsAllReadingVisible");
            }
        }

        private Visibility isConjungationVisible;
        public Visibility IsConjungationVisible
        {
            get
            {
                return isConjungationVisible;
            }
            set
            {
                isConjungationVisible = value;
                RaisePropertyChanged("IsConjungationVisible");
            }
        }

        private Thickness borderThickness;
        public Thickness BorderThickness
        {
            get
            {
                return borderThickness;
            }
            set
            {
                borderThickness = value;
                RaisePropertyChanged("BorderThickness");
            }
        }

        public DictionaryWordModel(JmdictWord word, bool isHaveBorder)
        {
            EntrySequence = word.EntrySequence;
            this.firsWord = word.FirstWord.Word;
            this.isHaveExample = word.IsHaveExample;
            this.conjungation = word.Conjungation;
            if (word.FirstWord.Priority != null)
                this.priority = word.FirstWord.Priority.ToUpper();
            else
                this.priority = JmdictWord.NO_PRIORITY.ToUpper();

            firsWordInformation = word.FirstWord.Information;
            kanjiList = new OneWordViewModel(word.KanjiLetters);

            readingApplyToAll = word.ReadingApplyToAll;
            restrictReading = word.RestrictReading;
            otherKanjiForms = word.OtherKanjiForms;
            sense = new SenseElementRichTextViewModel(word.SenseElements);

            isConjungationVisible = conjungation != null && !String.IsNullOrWhiteSpace(conjungation) ? Visibility.Visible : Visibility.Collapsed;
            isAllReadingVisible = readingApplyToAll != null && !String.IsNullOrWhiteSpace(readingApplyToAll) ? Visibility.Visible : Visibility.Collapsed;
            isRestrictReadingVisible = restrictReading != null ? Visibility.Visible : Visibility.Collapsed;
            isOtherKanjiFormsVisible = otherKanjiForms != null ? Visibility.Visible : Visibility.Collapsed;

            if (isHaveBorder)
                borderThickness = new Thickness(0, 2, 0, 0);
            else
                borderThickness = new Thickness(0, 0, 0, 0);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
