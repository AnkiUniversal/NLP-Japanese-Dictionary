using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public sealed class CharacterDefinitions
    {

        public const string CHARACTER_DEFINITIONS_FILENAME = "characterDefinitions.bin";

        public const int INVOKE = 0;

        public const int GROUP = 1;

        private const string DEFAULT_CATEGORY = "DEFAULT";

        private const int LENGTH = 2; // Not used as of now

        private readonly int[][] categoryDefinitions;

        private readonly int[][] codepointMappings;

        private readonly string[] categorySymbols;

        private readonly int[] defaultCategory;

        public CharacterDefinitions(int[][] categoryDefinitions, int[][] codepointMappings, string[] categorySymbols)
        {
            this.categoryDefinitions = categoryDefinitions;
            this.codepointMappings = codepointMappings;
            this.categorySymbols = categorySymbols;
            this.defaultCategory = LookupCategories(DEFAULT_CATEGORY);
        }

        public int[] LookupCategories(char c)
        {
            int[] mappings = codepointMappings[c];

            if (mappings == null)
            {
                return defaultCategory;
            }

            return mappings;
        }

        public int[] LookupDefinition(int category)
        {
            return categoryDefinitions[category];
        }

        public static CharacterDefinitions NewInstance(string resourceAbsolutePath)
        {
            try
            {
                using (Stream charDefInput = File.OpenRead(resourceAbsolutePath + Path.DirectorySeparatorChar + CHARACTER_DEFINITIONS_FILENAME))
                using(BinaryReader reader = new BinaryReader(charDefInput))
                {

                    int[][] definitions = IntegerArrayIO.ReadSparseArray2D(reader);
                    int[][] mappings = IntegerArrayIO.ReadSparseArray2D(reader);
                    string[] symbols = StringArrayIO.ReadArray(reader);

                    CharacterDefinitions characterDefinition = new CharacterDefinitions(definitions, mappings, symbols);
                    return characterDefinition;
                }
            }
            catch (IOException ex)
            {
                throw new IOException("CharacterDefinitions.NewInstance: " + ex.Message);
            }


        }

        public void SetCategories(char c, string[] categoryNames)
        {
            codepointMappings[c] = LookupCategories(categoryNames);
        }

        private int[] LookupCategories(params string[] categoryNames)
        {
            int[] categories = new int[categoryNames.Length];

            for (int i = 0; i < categoryNames.Length; i++)
            {
                string category = categoryNames[i];
                int categoryIndex = -1;

                for (int j = 0; j < categorySymbols.Length; j++)
                {
                    if (category.Equals(categorySymbols[j], StringComparison.OrdinalIgnoreCase))
                    {
                        categoryIndex = j;
                    }
                }

                if (categoryIndex < 0)
                {
                    throw new Exception("No category '" + category + "' found");
                }

                categories[i] = categoryIndex;
            }

            return categories;
        }
    }
}
