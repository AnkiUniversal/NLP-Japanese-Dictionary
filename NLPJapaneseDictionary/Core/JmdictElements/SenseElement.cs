using Newtonsoft.Json.Linq;
using NLPJDict.DatabaseTable.NLPJDictCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.JmdictElements
{
    public class SenseElement
    {
        public int Order { get; set; }
        public string[] OnlyForKanji { get; set; }
        public string[] OnlyForRead { get; set; }
        public string CrossReference { get; set; }
        public string Antonym { get; set; }
        public string PartOfSpeech { get; set; }
        public string Field { get; set; }
        public string Misc { get; set; }
        public string Dialect { get; set; }
        public string Gloss { get; set; }

        public SenseElement(JObject senseElement, int order)
        {            
            Order = order;
            JToken value;

            if (senseElement.TryGetValue("stagK", out value))
            {
                var stagK = value.ToString();
                OnlyForKanji = stagK.Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
            }

            if (senseElement.TryGetValue("stagR", out value))
            {
                var stagr = value.ToString();
                OnlyForRead = stagr.Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
            }

            if (senseElement.TryGetValue("xref", out value))
                CrossReference = value.ToString();

            if (senseElement.TryGetValue("ant", out value))
                Antonym = value.ToString();

            if (senseElement.TryGetValue("field", out value))
                Field = value.ToString();

            if (senseElement.TryGetValue("misc", out value))
                Misc = value.ToString();

            if (senseElement.TryGetValue("dialect", out value))
                Dialect = value.ToString();

            if (senseElement.TryGetValue("gloss", out value))
                Gloss = value.ToString();

            if (senseElement.TryGetValue("pos", out value))
                PartOfSpeech = value.ToString();
        }
    }
}