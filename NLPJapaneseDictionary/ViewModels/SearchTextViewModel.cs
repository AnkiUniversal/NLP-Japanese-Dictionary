using NLPJDict.Models;
using NLPJDict.NLPJDictCore.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.ViewModels
{
    public class SearchTextViewModel
    {
        public ObservableCollectionAutoResize<SearchTextModel> SearchedTexts { get; set; }

        public SearchTextViewModel(int maxSize)
        {
            SearchedTexts = new ObservableCollectionAutoResize<SearchTextModel>(maxSize);
        }

        public void AddFirstNonDuplicate(SearchTextModel model)
        {
            if (SearchedTexts.Count == 0 || !SearchedTexts[0].Text.Equals(model.Text, StringComparison.OrdinalIgnoreCase))
            {
                SearchedTexts.Insert(0, model);
            }
        }

        public void Remove(SearchTextModel model)
        {
            SearchedTexts.Remove(model);
        }
    }
}
