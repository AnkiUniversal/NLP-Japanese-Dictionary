using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.Dict;
using NLPJDict.Kuromoji.Core.Viterbi;
using NLPJDict.KuromojiIpadic.Compile;
using NLPJDict.Kuromoji.Interfaces;
using System;

namespace NLPJDict.KuromojiIpadic.Ipadic
{
    public class Token : TokenBase, IToken
    {
        public ViterbiNode.NodeType NodeType
        {
            get
            {
                return type;
            }
        }

        public string ConjugationType
        {
            get
            {
                return GetConjugationType();
            }
        }

        public string ConjugationForm
        {
            get
            {
                return GetConjugationForm();
            }
        }

        public string BaseForm
        {
            get
            {
                return GetBaseForm();
            }
        }

        public string Reading
        {
            get
            {
                return GetReading();
            }
        }

        public string Pronunciation
        {
            get
            {
                return GetPronunciation();
            }
        }

        public string PartOfSpeech
        {
            get
            {
                return GetPartOfSpeechLevel1();
            }
        }

        public Token(int wordId, string surface, ViterbiNode.NodeType type, int position, IDictionary dictionary)
                : base(wordId, surface, type, position, dictionary)
        {
        }

        /**
         * Gets the 1st level part-of-speech tag for this token (品詞細分類1)
         *
         * @return 1st level part-of-speech tag, not null
         */
        public string GetPartOfSpeechLevel1()
        {
            return GetFeature(DictionaryEntry.PART_OF_SPEECH_LEVEL_1);
        }

        /**
         * Gets the 2nd level part-of-speech tag for this token (品詞細分類2)
         *
         * @return 2nd level part-of-speech tag, not null
         */
        public string GetPartOfSpeechLevel2()
        {
            return GetFeature(DictionaryEntry.PART_OF_SPEECH_LEVEL_2);
        }

        /**
         * Gets the 3rd level part-of-speech tag for this token (品詞細分類3)
         *
         * @return 3rd level part-of-speech tag, not null
         */
        public String GetPartOfSpeechLevel3()
        {
            return GetFeature(DictionaryEntry.PART_OF_SPEECH_LEVEL_3);
        }

        /**
         * Gets the 4th level part-of-speech tag for this token (品詞細分類4)
         *
         * @return 4th level part-of-speech tag, not null
         */
        public string GetPartOfSpeechLevel4()
        {
            return GetFeature(DictionaryEntry.PART_OF_SPEECH_LEVEL_4);
        }

        /**
         * Gets the conjugation type for this token (活用型), if applicable
         * <p>
         * If this token does not have a conjugation type, return *
         *
         * @return conjugation type, not null
         */
        public string GetConjugationType()
        {
            return GetFeature(DictionaryEntry.CONJUGATION_TYPE);
        }

        /**
         * Gets the conjugation form for this token (活用形), if applicable
         * <p>
         * If this token does not have a conjugation form, return *
         *
         * @return conjugation form, not null
         */
        public string GetConjugationForm()
        {
            return GetFeature(DictionaryEntry.CONJUGATION_FORM);
        }

        /**
         * Gets the base form (also called dictionary form) for this token (基本形)
         *
         * @return base form, not null
         */
        public string GetBaseForm()
        {
            return GetFeature(DictionaryEntry.BASE_FORM);
        }

        /**
         * Gets the reading for this token (読み) in katakana script
         *
         * @return reading, not null
         */
        public string GetReading()
        {
            return GetFeature(DictionaryEntry.READING);
        }

        /**
         * Gets the pronunciation for this token (発音)
         *
         * @return pronunciation, not null
         */
        public string GetPronunciation()
        {
            return GetFeature(DictionaryEntry.PRONUNCIATION);
        }
    }
}
