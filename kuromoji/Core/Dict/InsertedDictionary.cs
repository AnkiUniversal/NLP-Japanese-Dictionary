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
