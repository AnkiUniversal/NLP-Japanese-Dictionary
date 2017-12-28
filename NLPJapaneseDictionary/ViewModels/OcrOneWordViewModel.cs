using NLPJapaneseDictionary.Helpers;
using NLPJDict.HelperClasses;
using NLPJDict.Models;
using NLPJDict.NLPJDictCore.DatabaseTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NLPJDict.ViewModels
{
    public class OcrOneWordViewModel
    {
        public ObservableCollection<OcrOneWordModel> Words { get; set; }
        public StringBuilder Sentence { get; set; } = new StringBuilder();
        public List<Jocr.TextBlock> TextBlocks { get; private set; }

        public OcrOneWordViewModel()
        {
            Words = new ObservableCollection<OcrOneWordModel>();            
        }

        public OcrOneWordViewModel(List<Jocr.TextBlock> blocks, SolidColorBrush brush = null)
        {
            if (brush == null)
                brush = UIUtilities.Green;

            TextBlocks = blocks;
            List<OcrOneWordModel> entriesModel = new List<OcrOneWordModel>();            
            for (int i = 0; i < blocks.Count; i++)
            {
                entriesModel.Add(new OcrOneWordModel(blocks[i].Text[0], i, brush));
                Sentence.Append(blocks[i].Text[0]);
            }
            Words = new ObservableCollection<OcrOneWordModel>(entriesModel);
        }

        public void AddNewList(List<Jocr.TextBlock> blocks, SolidColorBrush brush = null)
        {
            if (brush == null)
                brush = UIUtilities.Green;

            TextBlocks = blocks;
            Sentence.Clear();
            Words.Clear();
            for (int i = 0; i < blocks.Count; i++)
            {
                Words.Add(new OcrOneWordModel(blocks[i].Text[0], i, brush));
                Sentence.Append(blocks[i].Text[0]);
            }
        }

        public void ChangeWord(char text, int index)
        {
            Words[index].Word = text;
            Sentence[index] = text;
        }
    }
}
