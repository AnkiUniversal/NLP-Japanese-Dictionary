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

using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.FST;
using NLPJDict.Kuromoji.Core.Viterbi;
using NLPJDict.KuromojiIpadic.Compile;
using NLPJDict.Kuromoji.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.KuromojiIpadic.Ipadic
{
    public class Tokenizer : TokenizerBase<Token>, ITokenizer<Token>
    {
        public Tokenizer(string absoluteFolderPath) : this(new Builder(absoluteFolderPath))
        { }

        public Tokenizer(Builder builder)
        {
            Configure<Builder, Tokenizer>(builder);
        }

        /**
         * Tokenizes the provided text and returns a list of tokens with various feature information
         * <p>
         * This method is thread safe
         *
         * @param text  text to tokenize
         * @return list of Token, not null
         */
        public override List<Token> Tokenize(string text)
        {
            return CreateTokenList(text);
        }

        public class IpadicTokenFactory : TokenFactory<Token>
        {
            public Token CreateToken(int wordId, string surface, ViterbiNode.NodeType type,
                                    int position, IDictionary dictionary)
            {
                return new Token(wordId, surface, type, position, dictionary);
            }
        }

        /**
         * Builder class for creating a customized tokenizer instance
         */
        public class Builder : Builder<Tokenizer>
        {

            private const int DEFAULT_KANJI_LENGTH_THRESHOLD = 2;
            private const int DEFAULT_OTHER_LENGTH_THRESHOLD = 7;
            private const int DEFAULT_KANJI_PENALTY = 3000;
            private const int DEFAULT_OTHER_PENALTY = 1700;

            private int kanjiPenaltyLengthTreshold = DEFAULT_KANJI_LENGTH_THRESHOLD;
            private int kanjiPenalty = DEFAULT_KANJI_PENALTY;
            private int otherPenaltyLengthThreshold = DEFAULT_OTHER_LENGTH_THRESHOLD;
            private int otherPenalty = DEFAULT_OTHER_PENALTY;

            /**
             * Creates a default builder
             */
            public Builder(string absoluteFolderPath)
            {
                AbsoluteFolderPath = absoluteFolderPath;
                totalFeatures = DictionaryEntry.TOTAL_FEATURES;
                readingFeature = DictionaryEntry.READING_FEATURE;
                partOfSpeechFeature = DictionaryEntry.PART_OF_SPEECH_FEATURE;
                TokenFactory = new IpadicTokenFactory();
            }

            /**
             * Sets a custom kanji penalty
             * <p>
             * This is an expert feature used with {@link com.atilika.kuromoji.TokenizerBase.Mode#SEARCH} and {@link com.atilika.kuromoji.TokenizerBase.Mode#EXTENDED} modes that sets a length threshold and an additional costs used when running the Viterbi search.
             * The additional cost is applicable for kanji candidate tokens longer than the length threshold specified.
             * <p>
             * This is an expert feature and you usually would not need to change this.
             *
             * @param lengthThreshold  length threshold applicable for this penalty
             * @param penalty  cost added to Viterbi nodes for long kanji candidate tokens
             * @return this builder, not null
             */
            public Builder SetKanjiPenalty(int lengthThreshold, int penalty)
            {
                this.kanjiPenaltyLengthTreshold = lengthThreshold;
                this.kanjiPenalty = penalty;
                return this;
            }

            /**
             * Sets a custom non-kanji penalty
             * <p>
             * This is an expert feature used with {@link com.atilika.kuromoji.TokenizerBase.Mode#SEARCH} and {@link com.atilika.kuromoji.TokenizerBase.Mode#EXTENDED} modes that sets a length threshold and an additional costs used when running the Viterbi search.
             * The additional cost is applicable for non-kanji candidate tokens longer than the length threshold specified.
             * <p>
             * This is an expert feature and you usually would not need to change this.
             *
             * @param lengthThreshold  length threshold applicable for this penalty
             * @param penalty  cost added to Viterbi nodes for long non-kanji candidate tokens
             * @return this builder, not null
             */
            public Builder SetOtherPenalty(int lengthThreshold, int penalty)
            {
                this.otherPenaltyLengthThreshold = lengthThreshold;
                this.otherPenalty = penalty;
                return this;
            }

            /**
             * Predictate that splits unknown words on the middle dot character (U+30FB KATAKANA MIDDLE DOT)
             * <p>
             * This feature is off by default.
             * This is an expert feature sometimes used with {@link com.atilika.kuromoji.TokenizerBase.Mode#SEARCH} and {@link com.atilika.kuromoji.TokenizerBase.Mode#EXTENDED} mode.
             *
             * @param split  predicate to indicate split on middle dot
             * @return this builder, not null
             */
            public bool IsSplitOnNakaguro { get; set; } = false;         

            /**
             * Creates the custom tokenizer instance
             *
             * @return tokenizer instance, not null
             */
            public override Tokenizer Build()
            {
                return new Tokenizer(this);
            }

            public override void LoadDictionaries()
            {
                Penalties = new List<int>();
                Penalties.Add(kanjiPenaltyLengthTreshold);
                Penalties.Add(kanjiPenalty);
                Penalties.Add(otherPenaltyLengthThreshold);
                Penalties.Add(otherPenalty);

                try
                {
                    Fst = FST.NewInstance(AbsoluteFolderPath);
                    ConnectionCosts = ConnectionCosts.NewInstance(AbsoluteFolderPath);
                    TokenInfoDictionary = TokenInfoDictionary.NewInstance(AbsoluteFolderPath);
                    CharacterDefinitions = CharacterDefinitions.NewInstance(AbsoluteFolderPath);

                    if (IsSplitOnNakaguro)
                    {
                        CharacterDefinitions.SetCategories('・', new string[] { "SYMBOL" });
                    }

                    UnknownDictionary = UnknownDictionary.NewInstance(AbsoluteFolderPath, CharacterDefinitions, totalFeatures);
                    InsertedDictionary = new InsertedDictionary(totalFeatures);
                }
                catch (Exception ouch)
                {
                    throw new Exception("Could not load dictionaries: " + ouch.Message);
                }
            }
        }
    }
}
