/**
 * Copyright © 2017-2018 Anki Universal Team.
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

using NLPJDict.HelperClasses;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJDict.ConvertClasses
{
    public static class RomaConvert
    {   
        private static readonly Regex silentVowelRegex = new Regex("[ktcshfmyrwgzdbpkj]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly string DATA_FILE_PATH = Locations.ABS_DICT_CONVERT_PATH + "RomaHiraKata.txt";
        private const int ROMAJI = 0;
        private const int HIRAGANA = 1;        
        private const int KATAKANA = 2;

        private const int NUMBER_OF_WORD = 170;

        private static readonly string[,] HiraRomaKata;
        private const int START_SILENT__INDEX = 153;
        private const int END_SILENT_INDEX = 169;

        static RomaConvert()
        {
            HiraRomaKata = new string[NUMBER_OF_WORD,3];
            int count = 0;
            using (var file = File.OpenRead(DATA_FILE_PATH))
            using (var reader = new StreamReader(file))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                    HiraRomaKata[count, ROMAJI] = line[ROMAJI].Trim();
                    HiraRomaKata[count, HIRAGANA] = line[HIRAGANA].Trim();                    
                    HiraRomaKata[count, KATAKANA] = line[KATAKANA].Trim();
                    count++;
                }
            }
        }

        public static string ConvertRomaToHiraFullLoop(string text)
        {
            StringBuilder builder = new StringBuilder(text);
            for(int i = START_SILENT__INDEX; i <= END_SILENT_INDEX; i ++)
            {
                builder.Replace(HiraRomaKata[i, ROMAJI], HiraRomaKata[i, HIRAGANA]);
            }
            for (int i = 0; i < START_SILENT__INDEX; i++)
            {
                builder.Replace(HiraRomaKata[i, ROMAJI], HiraRomaKata[i, HIRAGANA]);
            }
            return builder.ToString();        
        }

        public static string ConvertOneRomaToHira(string text)
        {
            for (int i = 0; i < NUMBER_OF_WORD; i++)
            {
                if (HiraRomaKata[i, ROMAJI].Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return HiraRomaKata[i, HIRAGANA];
                }
            }

            return text;
        }

        public static string ConvertOneHiraToRoma(string text)
        {
            for (int i = 0; i < NUMBER_OF_WORD; i++)
            {
                if(HiraRomaKata[i, HIRAGANA].Equals(text, StringComparison.OrdinalIgnoreCase))
                {
                    return HiraRomaKata[i, ROMAJI];
                }
            }

            return text;
        }

        public static string ConvertHiraToRomaFullLoop(string text)
        {
            StringBuilder builder = new StringBuilder(text);
            return ConvertHiraToRomaFullLoop(builder);
        }

        public static string ConvertHiraToRomaFullLoop(StringBuilder builder)
        {
            for (int i = 0; i < NUMBER_OF_WORD; i++)
            {
                builder.Replace(HiraRomaKata[i, HIRAGANA], HiraRomaKata[i, ROMAJI]);
            }            
            builder.Replace("っ", " ");
            return builder.ToString();
        }

        public static string ConvertKataToRomaFullLoop(string text)
        {
            StringBuilder builder = new StringBuilder(text);
            return ConvertKataToRomaFullLoop(builder);
        }

        public static string ConvertKataToRomaFullLoop(StringBuilder builder)
        {
            for (int i = 0; i < NUMBER_OF_WORD; i++)
            {
                builder.Replace(HiraRomaKata[i, KATAKANA], HiraRomaKata[i, ROMAJI]);
            }
            builder.Replace("ッ", " ");
            string results = builder.ToString();

            if (results.Contains('ー'))
            {
                return results.Replace('ー', '-');
            }
            else
                return results;
        }
    }
}
