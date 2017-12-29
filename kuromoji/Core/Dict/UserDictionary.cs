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

using NLPJapaneseDictionary.Kuromoji.Core.HelperClasses;
using NLPJapaneseDictionary.Kuromoji.Core.Trie;
using NLPJapaneseDictionary.Kuromoji.Core.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Kuromoji.Core.Dict
{
    public class UserDictionary : IDictionary
    {

        private const int SIMPLE_USERDICT_FIELDS = 4;

        private const int WORD_COST_BASE = -100000;

        public const int MINIMUM_WORD_COST = Int32.MinValue / 2;

        private const int LEFT_ID = 5;

        private const int RIGHT_ID = 5;

        private const string DEFAULT_FEATURE = "*";

        private const string FEATURE_SEPARATOR = ",";
        // List of user dictionary entries
        private readonly List<UserDictionaryEntry> entries = new List<UserDictionaryEntry>();
        private readonly int readingFeature;
        private readonly int partOfSpeechFeature;
        private readonly int totalFeatures;
        // The word id below is the word id for the source string
        // surface string => [ word id, 1st token length, 2nd token length, ... , nth token length
        private PatriciaTrie<int[]> surfaces = new PatriciaTrie<int[]>();

        public UserDictionary(Stream input,
                              int totalFeatures,
                              int readingFeature,
                              int partOfSpeechFeature)
        {
            this.totalFeatures = totalFeatures;
            this.readingFeature = readingFeature;
            this.partOfSpeechFeature = partOfSpeechFeature;
            Read(input);
        }

        /**
         * Lookup words in text
         *
         * @param text text to look up user dictionary matches for
         * @return list of UserDictionaryMatch, not null
         */
        public List<UserDictionaryMatch> FindUserDictionaryMatches(String text)
        {
            List<UserDictionaryMatch> matchInfos = new List<UserDictionaryMatch>();
            int startIndex = 0;

            while (startIndex < text.Length)
            {
                int matchLength = 0;
                int endIndex = 0;

                while (CurrentInputContainsPotentialMatch(text, startIndex, endIndex))
                {
                    string matchCandidate = text.Substring(startIndex, endIndex);

                    if (surfaces.ContainsKey(matchCandidate))
                    {
                        matchLength = endIndex;
                    }

                    endIndex++;
                }

                if (matchLength > 0)
                {
                    String match = text.Substring(startIndex, matchLength);
                    int[] details = surfaces[match];

                    if (details != null)
                    {
                        matchInfos.AddRange(
                            MakeMatchDetails(startIndex, details)
                        );
                    }
                }

                startIndex++;
            }

            return matchInfos;
        }

        private bool CurrentInputContainsPotentialMatch(string text, int startIndex, int endIndex)
        {
            return startIndex + endIndex <= text.Length && surfaces.ContainsKeyPrefix(text.Substring(startIndex, endIndex));
        }

        public int GetLeftId(int wordId)
        {
            UserDictionaryEntry entry = entries[wordId];
            return entry.GetLeftId();
        }

        public int GetRightId(int wordId)
        {
            UserDictionaryEntry entry = entries[wordId];
            return entry.GetRightId();
        }

        public int GetWordCost(int wordId)
        {
            UserDictionaryEntry entry = entries[wordId];
            return entry.GetWordCost();
        }

        public string GetAllFeatures(int wordId)
        {
            UserDictionaryEntry entry = entries[wordId];
            return entry.GetAllFeatures();
        }

        public string[] GetAllFeaturesArray(int wordId)
        {
            UserDictionaryEntry entry = entries[wordId];
            return entry.GetAllFeaturesArray();
        }

        public string GetFeature(int wordId, params int[] fields)
        {
            UserDictionaryEntry entry = entries[wordId];
            return entry.GetFeature(fields);
        }

        private List<UserDictionaryMatch> MakeMatchDetails(int matchStartIndex, int[] details)
        {
            List<UserDictionaryMatch> matchDetails = new List<UserDictionaryMatch>(details.Length - 1);

            int wordId = details[0];
            int startIndex = 0;

            for (int i = 1; i < details.Length; i++)
            {
                int matchLength = details[i];

                matchDetails.Add(new UserDictionaryMatch(wordId, matchStartIndex + startIndex, matchLength));

                startIndex += matchLength;
                wordId++;
            }
            return matchDetails;
        }

        private void Read(Stream input)
        {
            input.Position = 0;
            using (StreamReader reader = new StreamReader(input, Encoding.UTF8))
            {
                string line;

                while (!reader.EndOfStream)
                {
                    // Remove comments and trim leading and trailing whitespace
                    line = Regex.Replace(reader.ReadLine(), "#.*$", "");
                    line = line.Trim();

                    // Skip empty lines or comment lines
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    AddEntry(line);
                }
            }
        }

        public void AddEntry(String entry)
        {
            string[] values = DictionaryEntryLineParser.ParseLine(entry);

            if (values.Length == SIMPLE_USERDICT_FIELDS)
            {
                AddSimpleEntry(values);
            }
            else if (values.Length == totalFeatures + 4)
            { // 4 = surface, left id, right id, word cost
                AddFullEntry(values);
            }
            else
            {
                throw new Exception("Illegal user dictionary entry " + entry);
            }
        }

        private void AddFullEntry(string[] values)
        {

            string surface = values[0];
            int[] costs = new int[]{
            Int32.Parse(values[1]),
            Int32.Parse(values[2]),
            Int32.Parse(values[3])
            };

            string[] features = new string[values.Length - 4];
            Array.Copy(values, 4, features, 0, features.Length);

            UserDictionaryEntry entry = new UserDictionaryEntry(surface, costs, features);

            int[] wordIdAndLengths = new int[1 + 1]; // Surface and a single length - the length of surface
            wordIdAndLengths[0] = entries.Count;
            wordIdAndLengths[1] = surface.Length;

            entries.Add(entry);

            surfaces[surface] = wordIdAndLengths;
        }

        private void AddSimpleEntry(string[] values)
        {
            string surface = values[0];
            string segmentationValue = values[1];
            string readingsValue = values[2];
            string partOfSpeech = values[3];

            string[] segmentation;
            string[] readings;
            string[] partOfSpeechs = partOfSpeech.SplitSpace();

            if (isCustomSegmentation(surface, segmentationValue))
            {
                segmentation = segmentationValue.SplitSpace();
                readings = readingsValue.SplitSpace();
            }
            else
            {
                segmentation = new string[] { segmentationValue };
                readings = new string[] { readingsValue };
            }

            if (segmentation.Length != readings.Length)
            {
                throw new Exception("User dictionary entry not properly formatted: " + values.Array2String());
            }

            if(partOfSpeechs.Length != 1 && partOfSpeechs.Length != segmentation.Length)
            {
                throw new Exception("User dictionary partOfSpeech not properly formatted: " + values.Array2String());
            }

            // { wordId, 1st token length, 2nd token length, ... , nth token length
            int[] wordIdAndLengths = new int[segmentation.Length + 1];

            int wordId = entries.Count;
            wordIdAndLengths[0] = wordId;

            for (int i = 0; i < segmentation.Length; i++)
            {

                wordIdAndLengths[i + 1] = segmentation[i].Length;

                String[] features;
                if (partOfSpeechs.Length == 1)
                    features = MakeSimpleFeatures(partOfSpeech, readings[i]);
                else
                    features = MakeSimpleFeatures(partOfSpeechs[i], readings[i]);

                int[] costs = MakeCosts(surface.Length);

                UserDictionaryEntry entry = new UserDictionaryEntry(
                    segmentation[i], costs, features
                );

                entries.Add(entry);
            }

            surfaces[surface] = wordIdAndLengths;
        }

        private int[] MakeCosts(int length)
        {
            int wordCost = WORD_COST_BASE * length;
            if (wordCost < MINIMUM_WORD_COST)
            {
                wordCost = MINIMUM_WORD_COST;
            }

            return new int[] { LEFT_ID, RIGHT_ID, wordCost };
        }

        private string[] MakeSimpleFeatures(String partOfSpeech, String reading)
        {
            String[] features = emptyFeatureArray();
            features[partOfSpeechFeature] = partOfSpeech;
            features[readingFeature] = reading;            
            return features;
        }

        private string[] emptyFeatureArray()
        {
            string[] features = new string[totalFeatures];

            for (int i = 0; i < features.Length; i++)
            {
                features[i] = DEFAULT_FEATURE;
            }
            return features;
        }

        private bool isCustomSegmentation(String surface, String segmentation)
        {
            return !surface.Equals(segmentation);
        }

        public class UserDictionaryMatch
        {
            private readonly int wordId;

            private readonly int matchStartIndex;

            private readonly int matchLength;

            public UserDictionaryMatch(int wordId, int matchStartIndex, int matchLength)
            {
                this.wordId = wordId;
                this.matchStartIndex = matchStartIndex;
                this.matchLength = matchLength;
            }

            public int GetWordId()
            {
                return wordId;
            }

            public int GetMatchStartIndex()
            {
                return matchStartIndex;
            }

            public int GetMatchLength()
            {
                return matchLength;
            }

            public override string ToString()
            {
                return "UserDictionaryMatch{" +
                    "wordId=" + wordId +
                    ", matchStartIndex=" + matchStartIndex +
                    ", matchLength=" + matchLength +
                    '}';
            }
        }

        public class UserDictionaryEntry
        {

            private string surface;

            private int[] costs;

            private string[] features;

            public UserDictionaryEntry(string surface, int[] costs, string[] features)
            {
                this.surface = surface;
                this.costs = costs;
                this.features = features;
            }

            public string GetSurface()
            {
                return surface;
            }

            public int GetLeftId()
            {
                return costs[0];
            }

            public int GetRightId()
            {
                return costs[1];
            }

            public int GetWordCost()
            {
                return costs[2];
            }

            public string[] GetAllFeaturesArray()
            {
                return features;
            }

            public string GetAllFeatures()
            {
                return String.Join(FEATURE_SEPARATOR, features);
            }

            public string GetFeature(params int[] fields)
            {
                string[] f = new string[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    int featureNumber = fields[i];
                    f[i] = features[featureNumber];
                }

                return String.Join(FEATURE_SEPARATOR, f);
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(surface);
                builder.Append(FEATURE_SEPARATOR);
                builder.Append(costs[0]);
                builder.Append(FEATURE_SEPARATOR);
                builder.Append(costs[1]);
                builder.Append(FEATURE_SEPARATOR);
                builder.Append(costs[2]);
                builder.Append(FEATURE_SEPARATOR);
                builder.Append(String.Join(FEATURE_SEPARATOR, features));
                return builder.ToString();
            }
        }
    }
}
