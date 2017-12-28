using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.Buffer
{
    public class FeatureInfoMap
    {

        private Dictionary<string, int> featureMap = new Dictionary<string, int>();

        private int maxValue = 0;

        public List<int> MapFeatures(string[] features)
        {
            List<int> posFeatureIds = new List<int>();
            foreach (string feature in features)
            {
                if (featureMap.ContainsKey(feature))
                {
                    posFeatureIds.Add(featureMap[feature]);
                }
                else
                {
                    featureMap[feature] =  maxValue;
                    posFeatureIds.Add(maxValue);
                    maxValue++;
                }
            }
            return posFeatureIds;
        }

        public SortedDictionary<int, string> Invert()
        {
            SortedDictionary<int, string> features = new SortedDictionary<int, string>();

            foreach (string key in featureMap.Keys)
            {
                features[featureMap[key]] =  key;
            }

            return features;
        }

        public int GetEntryCount()
        {
            return maxValue;
        }


        public override string ToString()
        {
            return "FeatureInfoMap{" +
                "featureMap=" + featureMap +
                ", maxValue=" + maxValue +
                '}';
        }
    }
}
