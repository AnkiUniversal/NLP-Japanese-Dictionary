using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class UnknownDictionaryCompiler : ICompiler
    {
        protected SortedDictionary<string, int> categoryMap;

        protected List<GenericDictionaryEntry> dictionaryEntries = new List<GenericDictionaryEntry>();

        public UnknownDictionaryCompiler(SortedDictionary<string, int> categoryMap)
        {
            this.categoryMap = categoryMap;
        }

        public void ReadUnknownDefinition(Stream input, String encoding)
        {
            try
            {
                input.Position = 0;
                using (var reader = new StreamReader(input, Encoding.GetEncoding(encoding)))
                {
                    UnknownDictionaryEntryParser parser = new UnknownDictionaryEntryParser();                    

                    while (!reader.EndOfStream)
                    {
                        GenericDictionaryEntry entry = parser.parse(reader.ReadLine().RemapCharIfNeeded());

                        dictionaryEntries.Add(entry);
                    }
                }
            }
            catch (IOException ex)
            {
                throw new IOException("UnknownDictionaryCompiler.ReadUnknownDefinition: " + ex.Message);
            }
        }

        public int[][] MakeCosts()
        {
            int[][] costs = new int[dictionaryEntries.Count][];

            for (int i = 0; i < dictionaryEntries.Count; i++)
            {
                GenericDictionaryEntry entry = dictionaryEntries[i];

                costs[i] = new int[] { entry.GetLeftId(), entry.GetRightId(), entry.GetWordCost() };
            }

            return costs;
        }

        public string[][] MakeFeatures()
        {
            string[][] features = new String[dictionaryEntries.Count][];

            for (int i = 0; i < dictionaryEntries.Count; i++)
            {
                GenericDictionaryEntry entry = dictionaryEntries[i];

                List<string> tmp = new List<string>();
                tmp.AddRange(entry.GetPartOfSpeechFeatures());
                tmp.AddRange(entry.GetOtherFeatures());

                features[i] = tmp.ToArray();
            }

            return features;
        }

        public int[][] MakeCategoryReferences()
        {
            int[][] entries = new int[categoryMap.Count][];

            foreach (string category in categoryMap.Keys)
            {
                int categoryId = categoryMap[category];

                entries[categoryId] = GetEntryIndices(category);
            }

            return entries;
        }

        public int[] GetEntryIndices(String surface)
        {
            List<int> indices = new List<int>();

            for (int i = 0; i < dictionaryEntries.Count; i++)
            {
                GenericDictionaryEntry entry = dictionaryEntries[i];

                if (entry.GetSurface().Equals(surface))
                {
                    indices.Add(i);
                }
            }

            return ToArray(indices);
        }

        private int[] ToArray(List<int> list)
        {
            int[] array = new int[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                array[i] = list[i];
            }

            return array;
        }

        public List<GenericDictionaryEntry> GetDictionaryEntries()
        {
            return dictionaryEntries;
        }

        public void Compile(Stream output)
        {
            using (BinaryWriter writer = new BinaryWriter(output))
            {
                IntegerArrayIO.WriteArray2D(writer, MakeCosts());
                IntegerArrayIO.WriteArray2D(writer, MakeCategoryReferences());
                StringArrayIO.WriteArray2D(writer, MakeFeatures());
            }
        }
    }

}
