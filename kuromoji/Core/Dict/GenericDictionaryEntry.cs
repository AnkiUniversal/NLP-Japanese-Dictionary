using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public class GenericDictionaryEntry : DictionaryEntryBase
    {

        private readonly string[] partOfSpeechFeatures;
        private readonly string[] otherFeatures;

        public GenericDictionaryEntry(Builder builder) : base(builder.Surface, builder.LeftId, builder.RightId, builder.WordCost)
        {
            partOfSpeechFeatures = builder.PartOfSpeechFeatures.ToArray(); ;
            otherFeatures = builder.OtherFeatures.ToArray();
        }

        public string[] GetPartOfSpeechFeatures()
        {
            return partOfSpeechFeatures;
        }

        public string[] GetOtherFeatures()
        {
            return otherFeatures;
        }

        public class Builder
        {
            public string Surface { get; set; }
            public short LeftId { get; set; }
            public short RightId { get; set; }
            public short WordCost { get; set; }
            public string[] PartOfSpeechFeatures { get; set; }
            public string[] OtherFeatures { get; set; }
        }
    }
}
