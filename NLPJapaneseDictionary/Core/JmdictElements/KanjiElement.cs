using Newtonsoft.Json.Linq;
using NLPJDict.NLPJDictCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.JmdictElements
{
    public class KanjiElement : IRepresentWord
    {
        public string Word { get; set; }
        public string Information { get; set; }
        public string Priority { get; set; }

        public KanjiElement(JObject kanjiElement)
        {
            JToken token;

            if (kanjiElement.TryGetValue("keb", out token))
                Word = token.ToString();
            if (kanjiElement.TryGetValue("ke_inf", out token))
                Information = token.ToString();
            if (kanjiElement.TryGetValue("ke_pri", out token))
                Priority = token.ToString();
        }
    }
}
