using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Compile
{
    public class CharacterDefinitionsCompiler : ICompiler
    {

        private SortedDictionary<string, int[]> categoryDefinitions = new SortedDictionary<string, int[]>();

        private List<SortedSet<string>> codepointCategories = new List<SortedSet<string>>();

        /// <summary>
        /// Different with the original java ver. We do not pass an output stream into the constructor
        /// to avoid complicated resource handling
        /// </summary>
        public CharacterDefinitionsCompiler(EncodingProvider provider)
        {
            Encoding.RegisterProvider(provider);
            for (int i = 0; i < 65536; i++)
            {
                codepointCategories.Add(null);
            }
        }

        public void ReadCharacterDefinition(Stream stream, string encoding)
        {
            try
            {
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(encoding)))
                {
                    while (!reader.EndOfStream)
                    {
                        // Strip comments
                        string line = Regex.Replace(reader.ReadLine(), "\\s*#.*", "").RemapCharIfNeeded();

                        // Skip empty line or comment line
                        if (String.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (IsCategoryEntry(line))
                        {
                            ParseCategory(line);
                        }
                        else
                        {
                            ParseMapping(line);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                throw new IOException("CharacterDefinitionsCompiler.ReadCharacterDefinition: " + ex.Message);
            }
        }

        private void ParseCategory(string line)
        {
            string[] values = line.SplitSpace();

            string classname = values[0];
            int invoke = Int32.Parse(values[1]);
            int group = Int32.Parse(values[2]);
            int length = Int32.Parse(values[3]);

            Debug.Assert(!categoryDefinitions.ContainsKey(classname));

            categoryDefinitions[classname] = new int[] { invoke, group, length };
        }

        private void ParseMapping(string line)
        {
            string[] values = line.SplitSpace();

            Debug.Assert(values.Length >= 2);

            string codepointString = values[0];
            List<string> categories = GetCategories(values);

            if (codepointString.ContainsExtend("..", StringComparison.OrdinalIgnoreCase))
            {
                string[] codepoints = codepointString.SplitString("..");

                int lowerCodepoint = codepoints[0].Int32Decode();
                int upperCodepoint = codepoints[1].Int32Decode();

                for (int i = lowerCodepoint; i <= upperCodepoint; i++)
                {
                    AddMapping(i, categories);
                }

            }
            else
            {
                int codepoint = codepointString.Int32Decode();

                AddMapping(codepoint, categories);
            }
        }

        private List<string> GetCategories(string[] values)
        {
            List<string> temp = new List<string>(values);
            return temp.GetRange(1, values.Length - 1);
        }

        private void AddMapping(int codepoint, List<string> categories)
        {
            foreach (string category in categories)
            {
                AddMapping(codepoint, category);
            }
        }

        private void AddMapping(int codepoint, string category)
        {
            var categories = codepointCategories[codepoint];

            if (categories == null)
            {
                categories = new SortedSet<string>();
                codepointCategories[codepoint] = categories;
            }

            categories.Add(category);
        }

        private bool IsCategoryEntry(string line)
        {
            return !line.StartsWith("0x");
        }

        public SortedDictionary<string, int> MakeCharacterCategoryMap()
        {
            var classMapping = new SortedDictionary<string, int>();
            int i = 0;

            foreach (string category in categoryDefinitions.Keys)
            {
                classMapping[category] = i++;
            }
            return classMapping;
        }

        private int[][] MakeCharacterDefinitions()
        {
            var categoryMap = MakeCharacterCategoryMap();
            int size = categoryMap.Count;
            int[][] array = new int[size][];

            foreach (string category in categoryDefinitions.Keys)
            {
                int[] values = categoryDefinitions[category];

                Debug.Assert(values.Length == 3);

                int index = categoryMap[category];
                array[index] = values;
            }

            return array;
        }

        private int[][] MakeCharacterMappings()
        {
            var categoryMap = MakeCharacterCategoryMap();

            int size = codepointCategories.Count;
            int[][] array = new int[size][];

            for (int i = 0; i < size; i++)
            {
                var categories = codepointCategories[i];

                if (categories != null)
                {
                    int innerSize = categories.Count;
                    int[] inner = new int[innerSize];

                    int j = 0;

                    foreach (string value in categories)
                    {
                        inner[j++] = categoryMap[value];
                    }
                    array[i] = inner;
                }
            }

            return array;
        }

        private string[] MakeCharacterCategorySymbols()
        {
            var categoryMap = MakeCharacterCategoryMap();
            var inverted = new SortedDictionary<int, string>();

            foreach (string key in categoryMap.Keys)
            {
                inverted[categoryMap[key]] = key;
            }

            string[] categories = new string[inverted.Count];

            foreach (int index in inverted.Keys)
            {
                categories[index] = inverted[index];
            }

            return categories;
        }

        public SortedDictionary<string, int[]> GetCategoryDefinitions()
        {
            return categoryDefinitions;
        }

        public List<SortedSet<string>> GetCodepointCategories()
        {
            return codepointCategories;
        }

        /// <summary>
        /// Different with the original java ver. We need to pass an output stream into this mehod
        /// </summary>
        public void Compile(Stream output)
        {
            try
            {
                using (BinaryWriter dataOut = new BinaryWriter(output))
                {
                    IntegerArrayIO.WriteSparseArray2D(dataOut, MakeCharacterDefinitions());
                    IntegerArrayIO.WriteSparseArray2D(dataOut, MakeCharacterMappings());
                    StringArrayIO.WriteArray(dataOut, MakeCharacterCategorySymbols());
                }
            }
            catch (IOException ex)
            {
                throw new IOException("CharacterDefinitionsCompiler.Compile: " + ex.Message);
            }
        }
    }
}
