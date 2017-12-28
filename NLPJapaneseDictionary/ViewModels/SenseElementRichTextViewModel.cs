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

using NLPJDict.Models;
using NLPJDict.NLPJDictCore.JmdictElements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NLPJDict.ViewModels
{
    public class SenseElementRichTextViewModel : INotifyPropertyChanged
    {
        private FlowDocument senseDocument;
        public FlowDocument SenseDocument
        {
            get
            {
                return senseDocument;
            }
            set
            {
                senseDocument = value;
                RaisePropertyChanged("SenseDocument");
            }
        }

        /// <summary>
        /// Use this to avoid "Already child element exception" of RichTextBlock
        /// </summary>
        public void CloneSenseDocument()
        {
            senseDocument = DeepCloneDocument();
        }

        public FlowDocument DeepCloneDocument()
        {
            FlowDocument myFlowDoc = new FlowDocument();
            foreach (var block in senseDocument.Blocks)
            {
                Paragraph p = new Paragraph();
                foreach (var inline in (block as Paragraph).Inlines)
                {
                    Run run = new Run();
                    var currentRun = inline as Run;
                    run.Text = currentRun.Text;
                    run.FontFamily = currentRun.FontFamily;
                    run.FontSize = currentRun.FontSize;
                    run.FontWeight = currentRun.FontWeight;
                    run.FontStyle = currentRun.FontStyle;

                    p.Inlines.Add(run);
                }
                myFlowDoc.Blocks.Add(p);
            }
            return myFlowDoc;
        }

        public SenseElementRichTextViewModel(List<SenseElement> senseElements)
        {
            FlowDocument document = new FlowDocument();
            foreach (var sense in senseElements)
            {
                string onlyForKanji, onlyForRead;
                SenseElementViewModel.GetRestrictKanjiAndRead(sense, out onlyForKanji, out onlyForRead);

                var senseModel = new SenseElementRichTextModel(sense.Order, onlyForKanji, onlyForRead, sense.CrossReference, sense.Antonym,
                                                            sense.PartOfSpeech, sense.Field, sense.Misc, sense.Dialect, sense.Gloss);
                document.Blocks.Add(senseModel.Sense);
            }
            SenseDocument = document;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
