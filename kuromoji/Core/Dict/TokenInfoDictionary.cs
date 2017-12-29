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

using NLPJapaneseDictionary.Kuromoji.Core.Buffer;
using NLPJapaneseDictionary.Kuromoji.Core.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Dict
{
    public class TokenInfoDictionary : IDictionary, IDisposable
    {

        public const string TOKEN_INFO_DICTIONARY_FILENAME = "tokenInfoDictionary.bin";
        public const string FEATURE_MAP_FILENAME = "tokenInfoFeaturesMap.bin";
        public const string POS_MAP_FILENAME = "tokenInfoPartOfSpeechMap.bin";
        public const string TARGETMAP_FILENAME = "tokenInfoTargetMap.bin";

        private const int LEFT_ID = 0;
        private const int RIGHT_ID = 1;
        private const int WORD_COST = 2;
        private const int TOKEN_INFO_OFFSET = 3;

        private const string FEATURE_SEPARATOR = ",";

        protected TokenInfoBuffer tokenInfoBuffer;
        protected StringValueMapBuffer posValues;
        protected StringValueMapBuffer stringValues;
        protected WordIdMap wordIdMap;

        public int[] LookupWordIds(int sourceId)
        {
            return wordIdMap.LookUp(sourceId);
        }

        public int GetLeftId(int wordId)
        {
            return tokenInfoBuffer.LookupTokenInfo(wordId, LEFT_ID);
        }

        public int GetRightId(int wordId)
        {
            return tokenInfoBuffer.LookupTokenInfo(wordId, RIGHT_ID);
        }

        public int GetWordCost(int wordId)
        {
            return tokenInfoBuffer.LookupTokenInfo(wordId, WORD_COST);
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            BufferEntry bufferEntry = tokenInfoBuffer.LookupEntry(wordId);

            int posLength = bufferEntry.PosInfos.Length;
            int featureLength = bufferEntry.FeatureInfos.Length;

            bool partOfSpeechAsShorts = false;

            if (posLength == 0)
            {
                posLength = bufferEntry.TokenInfos.Length - TOKEN_INFO_OFFSET;
                partOfSpeechAsShorts = true;
            }

            String[] result = new String[posLength + featureLength];

            if (partOfSpeechAsShorts)
            {
                for (int i = 0; i < posLength; i++)
                {
                    int feature = bufferEntry.TokenInfos[i + TOKEN_INFO_OFFSET];
                    result[i] = posValues.Get(feature);
                }
            }
            else
            {
                for (int i = 0; i < posLength; i++)
                {
                    int feature = bufferEntry.PosInfos[i] & 0xff;
                    result[i] = posValues.Get(feature);
                }
            }

            for (int i = 0; i < featureLength; i++)
            {
                int feature = bufferEntry.FeatureInfos[i];
                String s = stringValues.Get(feature);
                result[i + posLength] = s;
            }

            return result;
        }

        public string GetAllFeatures(int wordId)
        {
            string[] features = GetAllFeaturesArray(wordId);

            for (int i = 0; i < features.Length; i++)
            {
                String feature = features[i];
                features[i] = DictionaryEntryLineParser.Escape(feature);
            }
            return String.Join(FEATURE_SEPARATOR, features);            
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            if (fields.Length == 1)
            {
                return ExtractSingleFeature(wordId, fields[0]);
            }

            return extractMultipleFeatures(wordId, fields);
        }

        private string ExtractSingleFeature(int wordId, int field)
        {
            int featureId;

            if (tokenInfoBuffer.IsPartOfSpeechFeature(field))
            {
                featureId = tokenInfoBuffer.LookupPartOfSpeechFeature(wordId, field);
                return posValues.Get(featureId);
            }

            featureId = tokenInfoBuffer.LookupFeature(wordId, field);
            return stringValues.Get(featureId);
        }

        private string extractMultipleFeatures(int wordId, int[] fields)
        {
            if (fields.Length == 0)
            {
                return GetAllFeatures(wordId);
            }

            if (fields.Length == 1)
            {
                return ExtractSingleFeature(wordId, fields[0]);
            }

            String[] allFeatures = GetAllFeaturesArray(wordId);
            String[] features = new String[fields.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                int featureNumber = fields[i];
                features[i] = DictionaryEntryLineParser.Escape(
                    allFeatures[featureNumber]
                );
            }
            return String.Join(FEATURE_SEPARATOR, features);
        }

        public static TokenInfoDictionary NewInstance(string absoluteResourcePath)
        {
            try
            {
                TokenInfoDictionary dictionary = new TokenInfoDictionary();
                dictionary.Setup(absoluteResourcePath);
                return dictionary;
            }
            catch (IOException ex)
            {
                throw new Exception("TokenInfoDictionary.NewInstance: " + ex.Message);
            }
        }

        private void Setup(string absoluteResourcePath)
        {
            try
            {
                using (var tokenStream = File.OpenRead(absoluteResourcePath + Path.DirectorySeparatorChar + TOKEN_INFO_DICTIONARY_FILENAME))
                { tokenInfoBuffer = new TokenInfoBuffer(tokenStream); }

                using (var stringStream = File.OpenRead(absoluteResourcePath + Path.DirectorySeparatorChar + FEATURE_MAP_FILENAME)) 
                { stringValues = new StringValueMapBuffer(stringStream); }

                using (var posStream = File.OpenRead(absoluteResourcePath + Path.DirectorySeparatorChar + POS_MAP_FILENAME))
                { posValues = new StringValueMapBuffer(posStream); }

                using (var stream = File.OpenRead(absoluteResourcePath + Path.DirectorySeparatorChar + TARGETMAP_FILENAME))
                { wordIdMap = new WordIdMap(stream); }
            }
            catch (IOException ex)
            {
                throw new Exception("TokenInfoDictionary.Setup: " + ex.Message);
            }
        }

        public void Dispose()
        {
            if(tokenInfoBuffer != null)
                tokenInfoBuffer.Dispose();
            if(stringValues != null)
                stringValues.Dispose();
            if(posValues != null)
                posValues.Dispose();
            
        }
    }
}
