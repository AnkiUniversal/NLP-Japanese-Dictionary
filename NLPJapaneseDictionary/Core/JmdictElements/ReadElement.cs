using Newtonsoft.Json.Linq;
using NLPJDict.DatabaseTable.NLPJDictCore;
using NLPJDict.NLPJDictCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.JmdictElements
{
    public class ReadElement : IRepresentWord
    {
        public string Word { get; set; }
        public string Priority { get; set; }
        public string[] Retricts { get; set; }
        public string Information { get; set; }

        public ReadElement(JObject readElement)
        {
            JToken value;

            if(readElement.TryGetValue("reb", out value))            
                Word = value.ToString();
            
            if (readElement.TryGetValue("re_inf", out value))            
                Information = value.ToString();
            
            if (readElement.TryGetValue("re_pri", out value))            
                Priority = value.ToString();

            if (readElement.TryGetValue("re_restr", out value))
                Retricts = value.ToString().Split(JmdictEntity.SEPARATOR_ARRAY, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
