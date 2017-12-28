using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.util
{
    public class UnknownDictionaryEntryParser : DictionaryEntryLineParser
    {

        // NOTE: Currently this code is the same as the IPADIC dictionary entry parser,
        // which is okay for all the dictionaries supported so far...
        public GenericDictionaryEntry parse(String entry)
        {
            string[] fields = ParseLine(entry);

            string surface = fields[0];
            short leftId = short.Parse(fields[1]);
            short rightId = short.Parse(fields[2]);
            short wordCost = short.Parse(fields[3]);

            string[] pos = new string[6];
            Array.Copy(fields, 4, pos, 0, pos.Length);

            string[] features = new string[fields.Length - 10];
            Array.Copy(fields, 10, features, 0, features.Length);

            GenericDictionaryEntry.Builder builder = new GenericDictionaryEntry.Builder()
            {
            Surface = surface,
            LeftId = leftId,
            RightId = rightId,
            WordCost = wordCost,
            PartOfSpeechFeatures = pos,
            OtherFeatures = features
            };

            GenericDictionaryEntry dictionaryEntry = new GenericDictionaryEntry(builder);
            return dictionaryEntry;
        }
    }
}
