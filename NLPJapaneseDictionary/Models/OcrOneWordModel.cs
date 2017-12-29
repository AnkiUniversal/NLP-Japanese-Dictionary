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
using System.Windows.Media;

namespace NLPJDict.Models
{
    public class OcrOneWordModel : INotifyPropertyChanged
    {
        private char word;
        public char Word
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

        public int Index { get; private set; }

        private SolidColorBrush brush;
        public SolidColorBrush Brush
        {
            get
            {
                return brush;
            }
            set
            {
                brush = value;
                RaisePropertyChanged("Brush");
            }
        }

        public OcrOneWordModel(char text, int index, SolidColorBrush brush)
        {
            word = text;
            Index = index;
            this.brush = brush;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
