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

using NLPJDict.NLPJDictCore;
using NLPJDict.NLPJDictCore.DatabaseTable;
using NLPJDict.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Models
{
    public class KanjiModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string kanjiElement;
        public string KanjiElement
        {
            get
            {
                return kanjiElement;
            }
            set
            {
                kanjiElement = value;
                RaisePropertyChanged("KanjiElement");
            }
        }

        private string variants;
        public string Variants
        {
            get
            {
                return variants;
            }
            set
            {
                variants = value;
                RaisePropertyChanged("Variants");
            }
        }

        private string radicalList;
        public string RadicalList
        {
            get
            {
                return radicalList;
            }
            set
            {
                radicalList = value;
                RaisePropertyChanged("RadicalList");
            }
        }

        public string oldJlpt;
        public string OldJlpt
        {
            get
            {
                return oldJlpt;
            }
            set
            {
                oldJlpt = value;
                RaisePropertyChanged("OldJlpt");
            }
        }

        private int grade;
        public int Grade
        {
            get
            {
                return grade;
            }
            set
            {
                grade = value;
                RaisePropertyChanged("Grade");
            }
        }

        private int strokeCount;
        public int StrokeCount
        {
            get
            {
                return strokeCount;
            }
            set
            {
                strokeCount = value;
                RaisePropertyChanged("StrokeCount");
            }
        }

        private string radicalName;
        public string RadicalName
        {
            get
            {
                return radicalName;
            }
            set
            {
                radicalName = value;
                RaisePropertyChanged("RadicalName");
            }
        }

        private string frequencyRank;
        public string FrequencyRank
        {
            get
            {
                return frequencyRank;
            }
            set
            {
                frequencyRank = value;
                RaisePropertyChanged("FrequencyRank");
            }
        }

        private string kunyomi;
        public string Kunyomi
        {
            get
            {
                return kunyomi;
            }
            set
            {
                kunyomi = value;
                RaisePropertyChanged("Kunyomi");
            }
        }

        private string onyomi;
        public string Onyomi
        {
            get
            {
                return onyomi;
            }
            set
            {
                onyomi = value;
                RaisePropertyChanged("Onyomi");
            }
        }

        private string nanori;
        public string Nanori
        {
            get
            {
                return nanori;
            }
            set
            {
                nanori = value;
                RaisePropertyChanged("Nanori");
            }
        }

        private string engMean;
        public string EngMean
        {
            get
            {
                return engMean;
            }
            set
            {
                engMean = value;
                RaisePropertyChanged("EngMean");
            }
        }

        private string svgData;
        public string SVGData
        {
            get
            {
                return svgData;
            }
            set
            {
                svgData = value;
                RaisePropertyChanged("SVGData");
            }
        }

        public KanjiModel(KanjiDict kanji) 
               : this(kanji.KanjiElement, kanji.Variants, kanji.RadicalList, kanji.Grade, kanji.StrokeCount, kanji.RadicalName, kanji.FrequencyRank,
                     kanji.Kunyomi, kanji.Onyomi, kanji.Nanori, kanji.EngMean, kanji.SVGData)
        {
                
        }

        public KanjiModel(string kanjiElement, string variants, string radicalList, int grade, int strokeCount, string radicalName, string frequencyRank,
                          string kunyomi, string onyomi, string nanori, string eng, string svgData)
        {
            this.kanjiElement = kanjiElement;
            this.variants = variants;
            this.radicalList = radicalList;
            this.grade = grade;
            this.strokeCount = strokeCount;
            this.radicalName = radicalName;
            if (frequencyRank != null)
                this.frequencyRank = frequencyRank.ToUpper();
            else
                this.frequencyRank = "UNRANKED";
            this.kunyomi = kunyomi;
            this.onyomi = onyomi;
            this.nanori = nanori;
            this.svgData = svgData;
            this.engMean = eng;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
