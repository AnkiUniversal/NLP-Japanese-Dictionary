using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Dict
{
    public class InsertedDictionary : IDictionary
    {

        private const string DEFAULT_FEATURE = "*";

        private const string FEATURE_SEPARATOR = ",";

        private readonly string[] featuresArray;

        private readonly string featuresString;

        public InsertedDictionary(int features)
        {

            featuresArray = new String[features];

            for (int i = 0; i < features; i++)
            {
                featuresArray[i] = DEFAULT_FEATURE;
            }

            featuresString = String.Join(FEATURE_SEPARATOR, featuresArray);
        }

        public int GetLeftId(int wordId)
        {
            return 0;
        }

        public int GetRightId(int wordId)
        {
            return 0;
        }

        public int GetWordCost(int wordId)
        {
            return 0;
        }

        public string GetAllFeatures(int wordId)
        {
            return featuresString;
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            return featuresArray;
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            string[] features = new string[fields.Length];

            for (int i = 0; i < features.Length; i++)
            {
                features[i] = DEFAULT_FEATURE;
            }
            return String.Join(FEATURE_SEPARATOR, features);
        }
    }
}
