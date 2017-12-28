using NLPJDict.Kuromoji.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public class UnknownDictionary : IDictionary
    {

        public const string UNKNOWN_DICTIONARY_FILENAME = "unknownDictionary.bin";

        private const string DEFAULT_FEATURE = "*";

        private const string FEATURE_SEPARATOR = ",";

        private readonly int[][] entries;

        private readonly int[][] costs;

        private readonly string[][] features;

        private readonly int totalFeatures;

        private readonly CharacterDefinitions characterDefinition;

        public UnknownDictionary(CharacterDefinitions characterDefinition,
                                 int[][] entries,
                                 int[][] costs,
                                 String[][] features,
                                 int totalFeatures)
        {
            this.characterDefinition = characterDefinition;
            this.entries = entries;
            this.costs = costs;
            this.features = features;
            this.totalFeatures = totalFeatures;
        }

        public UnknownDictionary(CharacterDefinitions characterDefinition, int[][] entries, int[][] costs, String[][] features)
                : this(characterDefinition, entries, costs, features, features.Length)
        {
        }


        public int[] LookupWordIds(int categoryId)
        {
            // Returns an array of word ids
            return entries[categoryId];
        }

        public int GetLeftId(int wordId)
        {
            return costs[wordId][0];
        }

        public int GetRightId(int wordId)
        {
            return costs[wordId][1];
        }

        public int GetWordCost(int wordId)
        {
            return costs[wordId][2];
        }

        public string GetAllFeatures(int wordId)
        {
            return String.Join(FEATURE_SEPARATOR, GetAllFeaturesArray(wordId));
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            if (totalFeatures == features.Length)
            {
                return features[wordId];
            }

            string[] allFeatures = new string[totalFeatures];
            string[] basicFeatures = features[wordId];

            for (int i = 0; i < basicFeatures.Length; i++)
            {
                allFeatures[i] = basicFeatures[i];
            }

            for (int i = basicFeatures.Length; i < totalFeatures; i++)
            {
                allFeatures[i] = DEFAULT_FEATURE;
            }

            return allFeatures;
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            string[] allFeatures = GetAllFeaturesArray(wordId);
            string[] features = new string[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                int featureNumber = fields[i];
                features[i] = allFeatures[featureNumber];
            }
            return String.Join(FEATURE_SEPARATOR, features);
        }

        public CharacterDefinitions GetCharacterDefinition()
        {
            return characterDefinition;
        }

        public static UnknownDictionary NewInstance(string absoluteFolderPath,
                                                    CharacterDefinitions characterDefinitions,
                                                    int totalFeatures)
        {
            string filePath = absoluteFolderPath + Path.DirectorySeparatorChar + UnknownDictionary.UNKNOWN_DICTIONARY_FILENAME;
            using (Stream unkDefInput = File.OpenRead(filePath))
            using(BinaryReader reader = new BinaryReader(unkDefInput))
            {
                int[][] costs = IntegerArrayIO.ReadArray2D(reader);
                int[][] references = IntegerArrayIO.ReadArray2D(reader);
                string[][] features = StringArrayIO.ReadArray2D(reader);

                UnknownDictionary unknownDictionary = new UnknownDictionary(
                    characterDefinitions,
                    references,
                    costs,
                    features,
                    totalFeatures
                );

                return unknownDictionary;
            }
        }
    }
}
