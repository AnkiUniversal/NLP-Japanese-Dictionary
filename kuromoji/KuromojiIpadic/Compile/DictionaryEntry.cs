using NLPJDict.Kuromoji.Core.Dict;


namespace NLPJDict.KuromojiIpadic.Compile
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
