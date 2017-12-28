using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.Kuromoji.Core.util
{
    public class DictionaryEntryLineParser
    {
        private const char QUOTE = '"';
        private const char COMMA = ',';
        private const string QUOTE_ESCAPED = "\"\"";

        /**
         * Parse CSV line
         *
         * @param line  line to parse
         * @return String array of parsed valued, null
         * @throws RuntimeException on malformed input
         */
        public static string[] ParseLine(string line)
        {
            bool insideQuote = false;
            List<string> result = new List<string>();
            StringBuilder builder = new StringBuilder();
            int quoteCount = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == QUOTE)
                {
                    insideQuote = !insideQuote;
                    quoteCount++;
                }

                if (c == COMMA && !insideQuote)
                {
                    String value = builder.ToString();
                    value = Unescape(value);

                    result.Add(value);
                    builder = new StringBuilder();
                    continue;
                }

                builder.Append(c);
            }

            result.Add(builder.ToString());

            if (quoteCount % 2 != 0)
            {
                throw new Exception("Unmatched quote in entry: " + line);
            }

            return result.ToArray();
        }

        /**
         * Unescape input for CSV
         *
         * @param text  text to be unescaped
         * @return unescaped value, not null
         */
        public static string Unescape(String text)
        {
            StringBuilder builder = new StringBuilder();
            bool foundQuote = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (i == 0 && c == QUOTE || i == text.Length - 1 && c == QUOTE)
                {
                    continue;
                }

                if (c == QUOTE)
                {
                    if (foundQuote)
                    {
                        builder.Append(QUOTE);
                        foundQuote = false;
                    }
                    else
                    {
                        foundQuote = true;
                    }
                }
                else
                {
                    foundQuote = false;
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        /**
         * Escape input for CSV
         *
         * @param text  text to be escaped
         * @return escaped value, not null
         */
        public static string Escape(string text)
        {
            bool hasQuote = text.IndexOf(QUOTE) >= 0;
            bool hasComma = text.IndexOf(COMMA) >= 0;

            if (!(hasQuote || hasComma))
            {
                return text;
            }

            StringBuilder builder = new StringBuilder();

            if (hasQuote)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];

                    if (c == QUOTE)
                    {
                        builder.Append(QUOTE_ESCAPED);
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }
            else
            {
                builder.Append(text);
            }

            if (hasComma)
            {
                builder.Insert(0, QUOTE);
                builder.Append(QUOTE);
            }
            return builder.ToString();
        }
    }
}
