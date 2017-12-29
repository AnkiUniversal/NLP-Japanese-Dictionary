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

using NLPJapaneseDictionary.Kuromoji.Core.Compile;
using NLPJapaneseDictionary.Kuromoji.Core.Dict;
using NLPJapaneseDictionary.Kuromoji.Core.util;
using System.Collections.Generic;
using System.Text;

namespace NLPJapaneseDictionary.KuromojiIpadic.Compile
{
    public class TokenInfoDictionaryCompiler : TokenInfoDictionaryCompilerBase<DictionaryEntry>
    {

        public TokenInfoDictionaryCompiler(string encoding, EncodingProvider provider) : base(encoding, provider)
        {
        }

        protected override DictionaryEntry Parse(string line)
        {
            string[] fields = DictionaryEntryLineParser.ParseLine(line);
            DictionaryEntry entry = new DictionaryEntry(fields);
            return entry;
        }

        protected override GenericDictionaryEntry MakeGenericDictionaryEntry(DictionaryEntry entry)
        {
            List<string> pos = MakePartOfSpeechFeatures(entry);
            List<string> features = MakeOtherFeatures(entry);

            var builder = new GenericDictionaryEntry.Builder();
            builder.Surface = entry.GetSurface();
            builder.LeftId = entry.GetLeftId();
            builder.RightId = entry.GetRightId();
            builder.WordCost = entry.GetWordCost();
            builder.PartOfSpeechFeatures = pos.ToArray();
            builder.OtherFeatures = features.ToArray();
            return new GenericDictionaryEntry(builder);
        }

        public List<string> MakePartOfSpeechFeatures(DictionaryEntry entry)
        {
            List<string> posFeatures = new List<string>();

            posFeatures.Add(entry.GetPartOfSpeechLevel1());
            posFeatures.Add(entry.GetPartOfSpeechLevel2());
            posFeatures.Add(entry.GetPartOfSpeechLevel3());
            posFeatures.Add(entry.GetPartOfSpeechLevel4());

            posFeatures.Add(entry.GetConjugationType());
            posFeatures.Add(entry.GetConjugatedForm());

            return posFeatures;
        }

        public List<string> MakeOtherFeatures(DictionaryEntry entry)
        {
            List<string> otherFeatures = new List<string>();

            otherFeatures.Add(entry.GetBaseForm());
            otherFeatures.Add(entry.GetReading());
            otherFeatures.Add(entry.GetPronunciation());

            return otherFeatures;
        }
    }
}
