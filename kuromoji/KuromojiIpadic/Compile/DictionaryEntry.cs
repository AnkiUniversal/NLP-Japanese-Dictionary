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

using NLPJapaneseDictionary.Kuromoji.Core.Dict;


namespace NLPJapaneseDictionary.KuromojiIpadic.Compile
{
    public class DictionaryEntry : DictionaryEntryBase
    {
        public const int PART_OF_SPEECH_LEVEL_1 = 4;
        public const int PART_OF_SPEECH_LEVEL_2 = 5;
        public const int PART_OF_SPEECH_LEVEL_3 = 6;
        public const int PART_OF_SPEECH_LEVEL_4 = 7;
        public const int CONJUGATION_TYPE = 8;
        public const int CONJUGATION_FORM = 9;
        public const int BASE_FORM = 10;
        public const int READING = 11;
        public const int PRONUNCIATION = 12;

        public const int TOTAL_FEATURES = 9;
        public const int READING_FEATURE = 7;
        public const int PRONUN_FEATURE = 8;
        public const int PART_OF_SPEECH_FEATURE = 0;

        private readonly string posLevel1;
        private readonly string posLevel2;
        private readonly string posLevel3;
        private readonly string posLevel4;

        private readonly string conjugatedForm;
        private readonly string conjugationType;

        private readonly string baseForm;
        private readonly string reading;
        private readonly string pronunciation;

        public DictionaryEntry(string[] fields) : base(fields[DictionaryField.SURFACE], short.Parse(fields[DictionaryField.LEFT_ID]),
                                                       short.Parse(fields[DictionaryField.RIGHT_ID]), short.Parse(fields[DictionaryField.WORD_COST]))
        {
            posLevel1 = fields[PART_OF_SPEECH_LEVEL_1];
            posLevel2 = fields[PART_OF_SPEECH_LEVEL_2];
            posLevel3 = fields[PART_OF_SPEECH_LEVEL_3];
            posLevel4 = fields[PART_OF_SPEECH_LEVEL_4];

            conjugationType = fields[CONJUGATION_TYPE];
            conjugatedForm = fields[CONJUGATION_FORM];

            baseForm = fields[BASE_FORM];
            reading = fields[READING];
            pronunciation = fields[PRONUNCIATION];
        }

        public string GetPartOfSpeechLevel1()
        {
            return posLevel1;
        }

        public string GetPartOfSpeechLevel2()
        {
            return posLevel2;
        }

        public string GetPartOfSpeechLevel3()
        {
            return posLevel3;
        }

        public string GetPartOfSpeechLevel4()
        {
            return posLevel4;
        }

        public string GetConjugatedForm()
        {
            return conjugatedForm;
        }

        public string GetConjugationType()
        {
            return conjugationType;
        }

        public string GetBaseForm()
        {
            return baseForm;
        }

        public string GetReading()
        {
            return reading;
        }

        public string GetPronunciation()
        {
            return pronunciation;
        }
    }
}
