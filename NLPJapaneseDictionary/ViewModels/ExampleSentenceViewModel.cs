using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class ExampleSentenceViewModel
    {
        public ObservableCollection<ExampleSentenceModel> Sentences { get; set; }

        public ExampleSentenceViewModel()
        {
            Sentences = new ObservableCollection<ExampleSentenceModel>();
        }

        public void GetExamples(string word, Database exampleDictionary, int numberOfExamples)
        {
            List<ExampleTable> examples = ExampleTable.GetExample(word, numberOfExamples, exampleDictionary);
            AddNewWordList(examples);
        }

        private void AddNewWordList(List<ExampleTable> entries)
        {
            Sentences.Clear();
            foreach (var entry in entries)
            {
                Sentences.Add(new ExampleSentenceModel(entry.JapaneseSentence, entry.EnglishSentence));
            }
        }
    }
}
