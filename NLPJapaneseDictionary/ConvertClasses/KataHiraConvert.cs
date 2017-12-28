using NLPJDict.HelperClasses;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NLPJDict.ConvertClasses
{
    public static class KataHiraConvert
    {
        private static readonly string DATA_FILE_PATH = Locations.ABS_DICT_CONVERT_PATH + "HiraKata.txt";
        private const int HIRAGANA = 0;
        private const int KATAKANA = 1;

        private static SortedDictionary<char, char> katakanaToHiragana = new SortedDictionary<char, char>();
        private static SortedDictionary<char, char> hiraganaToKatakana = new SortedDictionary<char, char>();

        static KataHiraConvert()
        {
            using (var file = File.OpenRead(DATA_FILE_PATH))
            using(var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('@');
                    katakanaToHiragana.Add(line[KATAKANA][0], line[HIRAGANA][0]);
                    hiraganaToKatakana.Add(line[HIRAGANA][0], line[KATAKANA][0]);
                }
            }
        }

        public static bool IsKatakana(char c)
        {
            return katakanaToHiragana.ContainsKey(c);
        }

        public static bool TryConvertableToHira(char c, out char result)
        {            
            var success = katakanaToHiragana.TryGetValue(c, out result);
            if (success)
                return true;
            return false;
        }

        public static string ConvertHiraToKata(string text)
        {
            StringBuilder builder = new StringBuilder();
            char convert;
            for (int i = 0; i < text.Length; i++)
            {
                var isSuccess = hiraganaToKatakana.TryGetValue(text[i], out convert);
                if (isSuccess)
                    builder.Append(convert);
                else
                    builder.Append(text[i]);
            }

            return builder.ToString();
        }

        public static string ConvertKataToHira(string text)
        {
            StringBuilder builder = new StringBuilder();
            char convert;
            for(int i = 0; i < text.Length; i++)
            {
                var isSuccess = katakanaToHiragana.TryGetValue(text[i], out convert);
                if (isSuccess)
                    builder.Append(convert);
                else
                {
                    if(text[i].Equals('ー') && builder.Length > 0)
                    {
                        var roma = RomaConvert.ConvertOneHiraToRoma(builder[builder.Length - 1].ToString());
                        switch(roma[roma.Length - 1])
                        {
                            case 'a':
                                builder.Append('あ');
                                break;
                            case 'i':
                                builder.Append('い');
                                break;
                            case 'u':
                                builder.Append('う');
                                break;
                            case 'e':
                                builder.Append('え');
                                break;
                            case 'o':
                                builder.Append('お');
                                break;
                        }
                    }
                    else
                        builder.Append(text[i]);
                }
            }

            return builder.ToString();
        }
    }
}
