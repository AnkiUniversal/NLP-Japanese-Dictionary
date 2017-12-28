using NLPJDict.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace NLPJDict.Models
{
    public class SenseElementRichTextModel
    {      
        private Paragraph sense;
        public Paragraph Sense
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
   
        public SenseElementRichTextModel(int order, string onlyForKanji, string onlyForRead, string crossReference, string antonym,
                                string partOfSpeech, string field, string misc, string dialect, string gloss)
        {
            sense = new Paragraph();            

            if (!String.IsNullOrWhiteSpace(partOfSpeech))
            {
                Run partOfSpeechRun = new Run();
                if(order == 1)
                    partOfSpeechRun.Text = partOfSpeech;
                else
                    partOfSpeechRun.Text = "\n" + partOfSpeech;

                partOfSpeechRun.FontSize = 18;
                partOfSpeechRun.FontStyle = FontStyles.Italic;
                sense.Inlines.Add(partOfSpeechRun);
            }

            Run orderRun = new Run();                        
            orderRun.Text = "\n" + order.ToString() + " ";
            orderRun.FontSize = 20;
            orderRun.FontWeight = FontWeights.SemiBold;
            sense.Inlines.Add(orderRun);

            Run glossRun = new Run();            
            glossRun.Text = gloss;
            glossRun.FontSize = 20;            
            sense.Inlines.Add(glossRun);

            if (onlyForKanji != null && !String.IsNullOrWhiteSpace(onlyForKanji))
            {
                Run onlyForKanjiRun = new Run();
                onlyForKanjiRun.Text = "\nOnly apply to: " + onlyForKanji;
                onlyForKanjiRun.FontSize = 16;
                onlyForKanjiRun.FontWeight = FontWeights.SemiBold;
                sense.Inlines.Add(onlyForKanjiRun);                
            }

            if (onlyForRead != null && !String.IsNullOrWhiteSpace(onlyForRead))
            {
                Run onlyForReadRun = new Run();
                onlyForReadRun.Text = "\nOnly apply to: " + onlyForRead;
                onlyForReadRun.FontSize = 16;
                onlyForReadRun.FontWeight = FontWeights.SemiBold;
                sense.Inlines.Add(onlyForReadRun);
            }

            if (crossReference != null && !String.IsNullOrWhiteSpace(crossReference))
            {
                Run crossReferenceRun = new Run();
                crossReferenceRun.Text = "\nSee also: " + crossReference;
                crossReferenceRun.FontSize = 14;
                sense.Inlines.Add(crossReferenceRun);
            }

            if (antonym != null && !String.IsNullOrWhiteSpace(antonym))
            {
                Run antonymRun = new Run();
                antonymRun.Text = "\nAntonym: " + antonym;
                antonymRun.FontSize = 14;
                sense.Inlines.Add(antonymRun);
            }

            if (field != null && !String.IsNullOrWhiteSpace(field))
            {
                Run fieldRun = new Run();
                fieldRun.Text = "\nField: " + field;
                fieldRun.FontSize = 14;
                sense.Inlines.Add(fieldRun);
            }

            if (misc != null && !String.IsNullOrWhiteSpace(misc))
            {
                Run miscRun = new Run();
                miscRun.Text = "\nNote(s): " + misc;
                miscRun.FontSize = 14;
                sense.Inlines.Add(miscRun);
            }

            if (dialect != null && !String.IsNullOrWhiteSpace(dialect))
            {
                Run dialgectRun = new Run();
                dialgectRun.Text = "\nDialect: " + dialect;
                dialgectRun.FontSize = 14;
                sense.Inlines.Add(dialgectRun);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
