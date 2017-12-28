using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.util;
using System.Collections.Generic;
using System.Text;

namespace NLPJDict.KuromojiIpadic.Compile
{
    public class TokenInfoDictionaryCompiler : TokenInfoDictionaryCompilerBase<DictionaryEntry>
    {

        public TokenInfoDictionaryCompiler(string encoding, EncodingProvider provider) : base(encoding, provider)
        {
        }

        protected override DictionaryEntry Parse(string line)
        {
            string[] fields = DictionaryEntryLineParser.ParseLine(line);
            DictionaryEntry entry = new DictionaryEntry(fields);
            return entry;
        }

        protected override GenericDictionaryEntry MakeGenericDictionaryEntry(DictionaryEntry entry)
        {
            List<string> pos = MakePartOfSpeechFeatures(entry);
            List<string> features = MakeOtherFeatures(entry);

            var builder = new GenericDictionaryEntry.Builder();
            builder.Surface = entry.GetSurface();
            builder.LeftId = entry.GetLeftId();
            builder.RightId = entry.GetRightId();
            builder.WordCost = entry.GetWordCost();
            builder.PartOfSpeechFeatures = pos.ToArray();
            builder.OtherFeatures = features.ToArray();
            return new GenericDictionaryEntry(builder);
        }

        public List<string> MakePartOfSpeechFeatures(DictionaryEntry entry)
        {
            List<string> posFeatures = new List<string>();

            posFeatures.Add(entry.GetPartOfSpeechLevel1());
            posFeatures.Add(entry.GetPartOfSpeechLevel2());
            posFeatures.Add(entry.GetPartOfSpeechLevel3());
            posFeatures.Add(entry.GetPartOfSpeechLevel4());

            posFeatures.Add(entry.GetConjugationType());
            posFeatures.Add(entry.GetConjugatedForm());

            return posFeatures;
        }

        public List<string> MakeOtherFeatures(DictionaryEntry entry)
        {
            List<string> otherFeatures = new List<string>();

            otherFeatures.Add(entry.GetBaseForm());
            otherFeatures.Add(entry.GetReading());
            otherFeatures.Add(entry.GetPronunciation());

            return otherFeatures;
        }
    }
}
