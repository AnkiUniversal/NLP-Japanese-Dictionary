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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.HelperClasses
{
    public static class StringHelper
    {
        public static readonly string[] SPACE_STRING_ARRAY = new string[] { " ", "　", "\t" };
        private static readonly Regex notKatakanaRegex = new Regex(@"[^\p{IsKatakana}]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex notHiraganaRegex = new Regex(@"[^\p{IsHiragana}]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex hiraganaRegex = new Regex(@"[\p{IsHiragana}+]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex notKanjiRegex = new Regex(@"[^\p{IsCJKUnifiedIdeographs}]", RegexOptions.Compiled | RegexOptions.Singleline);        
        private static readonly Regex kanjiRegex = new Regex(@"[\p{IsCJKUnifiedIdeographs}]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex notBasicLatin = new Regex(@"[^\p{IsBasicLatin}]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex isBasicLatinOrNumber = new Regex("[A-Za-z0-9]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex notSkipCodeRegex = new Regex(@"[^0-9-]", RegexOptions.Compiled | RegexOptions.Singleline);

        public static readonly Regex KatakanaRegex = new Regex(@"(\p{IsKatakana}+)", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly Regex NewLineRegex = new Regex(@"[\n\r]", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly Regex DashRegex = new Regex(@"(-)", RegexOptions.Compiled | RegexOptions.Singleline);

        public static bool EqualsOrdinalIgnore(this string compared, string comparer)
        {
            return compared.Equals(comparer, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHaveBasicLatinOrNumber(string str)
        {
            return isBasicLatinOrNumber.IsMatch(str);
        }

        public static bool IsHaveKanji(string str)
        {
            return kanjiRegex.IsMatch(str);
        }

        public static bool IsHaveHiragana(string str)
        {
            return hiraganaRegex.IsMatch(str);
        }

        public static bool IsHaveKatakana(string str)
        {
            return KatakanaRegex.IsMatch(str);
        }

        public static bool IsSkipCode(string str)
        {
            return !notSkipCodeRegex.IsMatch(str);
        }

        /**
         * Predicate denoting if input is all katakana characters
         *
         * @param string  input string
         * @return true if input is all katakana characters
         */
        public static bool IsKatakanaOnly(string str)
        {
            //WARNING: Different with the original java code
            //we use regex here instead of checking for each char
            return !notKatakanaRegex.IsMatch(str);
        }

        public static bool IsHiraganaOnly(string str)
        {
            //WARNING: Different with the original java code
            //we use regex here instead of checking for each char
            return !notHiraganaRegex.IsMatch(str);
        }

        public static bool IsKanjiOnly(string str)
        {
            return !notKanjiRegex.IsMatch(str);
        }

        public static bool IBasicLatinOnly(string str)
        {
            return !notBasicLatin.IsMatch(str);
        }

        public static string[] SplitString(this string str, string split)
        {
            return str.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitString(this string str, string[] split)
        {
            return str.Split(split, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitSpace(this string str)
        {
            return str.Split(SPACE_STRING_ARRAY, StringSplitOptions.RemoveEmptyEntries);
        }

        public static int Int32Decode(this string str)
        {
            if(str.StartsWith("0x"))
                 return Convert.ToInt32(str, 16);
            else
                return Convert.ToInt32(str);
        }

        public static string Array2String<T>(this IEnumerable<T> list)
        {
            return "[" + string.Join(", ", list) + "]";
        }

        public static int CodePointAt(this string str, int index)
        {
            if (!Char.IsSurrogatePair(str, index))
                return str[index];
            else
                return Char.ConvertToUtf32(str, index);
        }

        public static int SortLexicographically(string x, string y)
        {
            var length = x.Length < y.Length ? x.Length : y.Length;
            for (int i = 0; i < length; i++)
            {
                var xCode = (int)x[i];
                var yCode = (int)y[i];
                if (xCode != yCode)
                    return xCode - yCode;
            }
            return x.Length - y.Length;            
        }

        private static readonly char[] RemapCharList = new char[] { '￡' , '∥', '～', '－', '―' };

        /// <summary>
        /// This method should be used to correct mismatch euc-jp conversion into C#
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemapCharIfNeeded(this string str)
        {
            foreach(var c in RemapCharList)
            {
                if (str.Contains(c))
                    str = str.Replace(c, GetCharacterRemap(c));
            }
            return str;
        }

        public static char GetCharacterRemap(char c)
        {
            switch(c)
            {
                case '￡':
                    return '£';
                case '∥':
                    return '‖';
                case '～':
                    return '〜';
                case '－':
                    return '−';
                case '―':
                    return '—';
                default:
                    throw new Exception("Invalid remap char");
            }
        }

        public static List<string> GetSymbolStrings(string[] arrays)
        {
            List<string> found = new List<string>();
            for (int i = 0; i < arrays.Length; i++)
            {
                string str = arrays[i];
                foreach (var c in str)
                {
                    if (Char.IsSymbol(c))
                    {
                        found.Add(str + " : " + i.ToString());
                        break;
                    }
                }
            }
            return found;
        }

        public static bool ContainsExtend(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
