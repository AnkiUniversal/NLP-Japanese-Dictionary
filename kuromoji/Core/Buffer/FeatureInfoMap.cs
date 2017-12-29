/**
 * Copyright © 2010-2017 Atilika Inc. and contributors (see CONTRIBUTORS.md)
 * 
 * Modifications copyright (C) 2017 - 2018 Anki Universal Team <ankiuniversal@gmail.com>
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Buffer
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
