using System;
using System.Collections.Generic;
using System.Text;

namespace com.bitscopic.hilleman.core.utils
{
    public static class StringUtils
    {
        public static String COMMA = ",";
        public static String CARAT = "^";
        public static String CRLF = "\r\n";
        public static String BANG = "!";
        public static String PIPE = "|";
        public static String SEMICOLON = ";";
        public static String TAB = "\t";
        public static String[] CARAT_ARY = new String[] { "^" };
        public static String[] CRLF_ARY = new String[] { "\r\n" };
        public static String[] BANG_ARY = new String[] { "!" };
        public static String[] PIPE_ARY = new String[] { "|" };
        public static String[] SEMICOLON_ARY = new String[] { ";" };
        public static String[] TAB_ARY = new String[] { "\t" };
        public static String OK = "OK";
        public static string DB_NULL_PLACEHOLDER = "<NULL>";

        /// <summary>
        /// Break 's' in to lines using '\r\n', '\r', '\n' as the line separators
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string[] getLines(string s, StringSplitOptions options = StringSplitOptions.None)
        {
            if (String.IsNullOrEmpty(s))
            {
                return null;
            }

            s = s.Replace("\r\n", "\n"); // first replace all CR/LF pairs with single \n
            s = s.Replace('\r', '\n'); // if \r was used by itself then it will still be present - replace it with \n for easy split call below

            return s.Split(new char[] { '\n' }, options);
        }

        #region Split to IList
        public static List<String> splitToList(String s, Char delimiter)
        {
            return splitToList(s, delimiter, StringSplitOptions.None);
        }

        public static List<String> splitToList(String s, Char delimiter, StringSplitOptions options, bool trimItems = false)
        {
            return splitToList(s, new char[] { delimiter }, options, trimItems);
        }

        public static List<String> splitToList(String s, Char[] delimiters, StringSplitOptions options, bool trimItems = false)
        {
            if (String.IsNullOrEmpty(s) || delimiters == null || delimiters.Length == 0)
            {
                throw new ArgumentNullException("You must supply a non-empty string and delimiter");
            }
            String[] pieces = s.Split(delimiters, options);

            if (trimItems)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    if (!String.IsNullOrEmpty(pieces[i]))
                    {
                        pieces[i] = pieces[i].Trim();
                    }
                }
            }
            return new List<String>(pieces);
        }

        public static List<String> splitToList(String s, String delimiter)
        {
            return splitToList(s, delimiter, StringSplitOptions.None);
        }

        public static List<String> splitToList(String s, String delimiter, StringSplitOptions options)
        {
            return splitToList(s, new String[] { delimiter }, options);
        }


        public static List<String> splitToList(String s, String[] delimiters, StringSplitOptions options, bool trimItems = false)
        {
            if (String.IsNullOrEmpty(s) || delimiters == null || delimiters.Length == 0)
            {
                throw new ArgumentNullException("You must supply a non-empty string and delimiter");
            }
            String[] pieces = s.Split(delimiters, options);
            if (trimItems)
            {
                for (int i = 0; i < pieces.Length; i++)
                {
                    if (!String.IsNullOrEmpty(pieces[i]))
                    {
                        pieces[i] = pieces[i].Trim();
                    }
                }
            }
            return new List<String>(pieces);
        }
        #endregion

        #region Split to Array

        public static String[] split(String s, Char delimiter)
        {
            return split(s, delimiter, StringSplitOptions.None);
        }

        public static String[] split(String s, Char delimiter, StringSplitOptions options)
        {
            return split(s, new char[] { delimiter }, options);
        }

        public static String[] split(String s, Char[] delimiters, StringSplitOptions options)
        {
            if (String.IsNullOrEmpty(s) || delimiters == null || delimiters.Length == 0)
            {
                throw new ArgumentNullException("You must supply a non-empty string and delimiter");
            }
            String[] pieces = s.Split(delimiters, options);
            return pieces;
        }

        public static String[] split(String s, String delimiter)
        {
            return split(s, delimiter, StringSplitOptions.None);
        }

        public static String[] split(String s, String delimiter, StringSplitOptions options)
        {
            return split(s, new String[] { delimiter }, options);
        }

        public static String[] split(String s, String[] delimiters, StringSplitOptions options)
        {
            if (String.IsNullOrEmpty(s) || delimiters == null || delimiters.Length == 0)
            {
                throw new ArgumentNullException("You must supply a non-empty string and delimiter");
            }
            String[] pieces = s.Split(delimiters, options);
            return pieces;
        }

        #endregion

        public static String doubleToMoney(Double d)
        {
            return d.ToString("C"); // 'C' => currency
        }

        public static Int32 firstIndexOf(IList<String> stringCollection, String target)
        {
            for (int i = 0; i < stringCollection.Count; i++)
            {
                if (String.Equals(stringCollection[i], target))
                {
                    return i;
                }
            }

            return -1;
        }

        public static Int32 lastIndexOf(IList<String> stringCollection, String target)
        {
            for (int i = stringCollection.Count; i > 0; i--)
            {
                if (String.Equals(stringCollection[i - 1], target))
                {
                    return (i - 1);
                }
            }

            return -1;
        }

        /// <summary>
        /// Extract the first occurrence of a number in a string (e.g. trash1234.12foo => 1234.12)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String extractNumeric(String input)
        {
            StringBuilder sb = new StringBuilder();
            bool inNumber = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] >= '0' && '9' >= input[i])
                {
                    sb.Append(input[i]);
                    inNumber = true;
                    continue;
                }
                if (inNumber && input[i] == '.')
                {
                    sb.Append(input[i]);
                    continue;
                }
                if (inNumber) // leaving number portion so stop examining chars
                {
                    break;
                }
                inNumber = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// input = piece 1^piece 2^piece 3^piece 4
        /// StringUtils.piece(input, "^", 1) => piece 1
        /// StringUtils.piece(input, "^", 1, true) => piece 2
        /// </summary>
        /// <param name="input"></param>
        /// <param name="delimiter"></param>
        /// <param name="index"></param>
        /// <param name="zeroBasedIndex"></param>
        /// <returns></returns>
        public static String piece(String input, String delimiter, Int32 index, bool zeroBasedIndex = false)
        {
            index = index - (zeroBasedIndex ? 0 : 1);
            String[] pieces = StringUtils.split(input, delimiter, StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length <= index)
            {
                return String.Empty;
            }
            return pieces[index];
        }

        public static Int32 count(String input, char characterToCount)
        {
            Int32 total = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == characterToCount)
                {
                    total++;
                }
            }
            return total;
        }

        /// <summary>
        /// Extract the first string piece containing no numbers
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string extractNonNumeric(string input)
        {
            StringBuilder sb = new StringBuilder();
            bool inString = false;
            bool inNumber = false;
            for (int i = 0; i < input.Length; i++)
            {
                if (inNumber && input[i] == '.')
                {
                    continue;
                }
                if (input[i] >= '0' && '9' >= input[i])
                {
                    inNumber = true;
                    if (inString)
                    {
                        break;
                    }
                    continue; // else continue
                }
                inString = true;
                sb.Append(input[i]);
            }
            return sb.ToString();
        }

        public static String join(IList<String> stringsToJoin, String delimiter = null, Int32 startIdx = 0, Int32 endIdx = 0, String wrapStringsWith = null)
        {
            if (stringsToJoin == null || stringsToJoin.Count == 0)
            {
                return String.Empty;
            }

            if (String.IsNullOrEmpty(delimiter))
            {
                delimiter = StringUtils.CRLF;
            }

            if (startIdx < 0)
            {
                startIdx = 0;
            }

            if (endIdx <= 0 || endIdx > stringsToJoin.Count)
            {
                endIdx = stringsToJoin.Count;
            }

            if (!String.IsNullOrEmpty(wrapStringsWith))
            {
                IList<String> wrappedStringsToJoin = new List<String>();
                foreach (String s in stringsToJoin)
                {
                    wrappedStringsToJoin.Add(String.Concat(wrapStringsWith, s, wrapStringsWith));
                }
                stringsToJoin = wrappedStringsToJoin;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = startIdx; i < endIdx; i++)
            {
                sb.Append(stringsToJoin[i]);
                sb.Append(delimiter);
            }
            sb.Remove(sb.Length - (delimiter.Length), delimiter.Length);
            return sb.ToString();
        }

        public static String join(String[] stringsToJoin, String delimiter = null, Int32 startIdx = 0, Int32 endIdx = 0, String wrapStringsWith = null)
        {
            return StringUtils.join(new List<String>(stringsToJoin), delimiter, startIdx, endIdx);
        }

        public static String joinObjectArray(object[] stringsToJoin, String delimiter = null, Int32 startIdx = 0, Int32 endIdx = 0)
        {
            if (stringsToJoin == null || stringsToJoin.Length == 0)
            {
                return String.Empty;
            }

            IList<String> objsAsString = new List<String>();
            foreach (object obj in stringsToJoin)
            {
                objsAsString.Add(obj.ToString());
            }
            return StringUtils.join(objsAsString, delimiter, startIdx, endIdx);
        }

        internal static bool parseBool(string s)
        {
            if (String.Equals("T", s, StringComparison.CurrentCultureIgnoreCase)
                || String.Equals("TRUE", s, StringComparison.CurrentCultureIgnoreCase)
                || String.Equals("Y", s, StringComparison.CurrentCultureIgnoreCase)
                || String.Equals("YES", s, StringComparison.CurrentCultureIgnoreCase)
                || String.Equals("1", s))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a string consists of SOLELY numbers
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static bool isInteger(string target)
        {
            if (String.IsNullOrEmpty(target))
            {
                return false;
            }

            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] > '9' || '0' > target[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check is a string is numeric. Decimal numbers are ok. Must be well-formatted number (e.g. StringUtils.isNumeric("123.") == false)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static bool isNumeric(string target)
        {
            if (String.IsNullOrEmpty(target))
            {
                return false;
            }

            for (int i = 0; i < target.Length; i++)
            {
                if (target[i] > '9' || '0' > target[i])
                {
                    if (target[i] == '.' && (i + 1 == target.Length)) // if last char is a decimal, return false
                    {
                        return false;
                    }
                    if (target[i] != '.')
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static bool startsWithNumeric(String s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return false;
            }
            String numericPart = StringUtils.extractNumeric(s);
            if (String.IsNullOrEmpty(numericPart))
            {
                return false;
            }
            return s.IndexOf(numericPart) == 0;
        }

        /// <summary>
        /// Given a string, return a list of lines where each line contains less than or equal to the max lines per char param
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IList<String> wrapText(String text, Int32 maxCharsPerLine = 80)
        {
            IList<String> result = new List<String>();

            String[] wordsInText = StringUtils.split(text, " ", StringSplitOptions.None);

            StringBuilder currentLine = new StringBuilder();

            for (int i = 0; i < wordsInText.Length; i++) // String word in wordsInText)
            {
                String word = wordsInText[i];

                if (word.Length > maxCharsPerLine)
                {
                    if (currentLine.Length > 0) // if there was a word in the current line we need to add it here so it's not lost!
                    {
                        result.Add(currentLine.ToString());
                        currentLine.Clear();
                    }

                    int chunkSize = maxCharsPerLine - 1; // subtract 1 for hyphen!
                    bool lastPiece = false;
                    for (int z = 0; z < word.Length; z += chunkSize)
                    {
                        if (z + chunkSize > word.Length)
                        {
                            chunkSize = word.Length - z;
                            lastPiece = true;
                        }

                        if (lastPiece)
                        {
                            currentLine.Clear(); // SHOULD ALREADY BE CLEARED!!! however, doing so explicitly here to make it obvious we're starting a new line with the last piece
                            currentLine.Append(word.Substring(z, chunkSize));
                        }
                        else
                        {
                            result.Add((word.Substring(z, chunkSize) + "-"));
                        }
                    }
                    continue; // go to the next word!
                }

                if (currentLine.Length + word.Length < maxCharsPerLine)
                {
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(" ");
                    }
                    currentLine.Append(word);
                }
                else
                {
                    result.Add(currentLine.ToString());
                    currentLine = currentLine.Clear();

                    if (word.Length < maxCharsPerLine) // only add the new word if < maxCharsPerLine
                    {
                        currentLine.Append(word);
                    } 
                    else // otherwise, set i back 1 so next trip through loop will catch > maxCharsPerLine and chunk it up!
                    {
                        i--;
                        continue;
                    }
                }
            }

            if (currentLine.Length > 0) // add last line
            {
                result.Add(currentLine.ToString());
            }

            return result;
        }

        internal static Dictionary<string, string> toDictFromDelimited(string delimitedString, String recordDelimiter, String keyValDelimiter)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(delimitedString))
            {
                return result;
            }

            String[] records = StringUtils.split(delimitedString, recordDelimiter);
            for (int i = 0; i < records.Length; i++)
            {
                if (String.IsNullOrEmpty(records[i]) || !records[i].Contains(keyValDelimiter))
                {
                    continue;
                }

                Int32 keyValDelimIdx = records[i].IndexOf(keyValDelimiter);
                String piece1 = records[i].Substring(0, keyValDelimIdx);
                String piece2 = records[i].Substring(keyValDelimIdx + 1);

                result.Add(piece1, piece2);
            }

            return result;
        }

        /// <summary>
        /// Given a string, pad it to the left with '0' characters up to total # of digits
        /// </summary>
        /// <param name="s">For example: 256</param>
        /// <param name="digits">10</param>
        /// <returns>0000000256</returns>
        public static String rightPack(String s, Int32 digits = 10, char packChar = '0')
        {
            StringBuilder sb = new StringBuilder();
            for (int i = s.Length; i < digits; i++)
            {
                sb.Append(packChar);
            }
            for (int i = 0; i < s.Length; i++)
            {
                sb.Append(s[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Given a string, escape it for use as a CSV fields. 
        ///     * escape all double quotes with an additional double quote
        ///     * if string contains comma or newline then wraps with double quotes
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String escapeForCsv(String s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return String.Empty;
            }

            if (s.Contains("\""))
            {
                s = s.Replace("\"", "\"\"");
            }

            if (s.Contains(",") || s.Contains("\r") || s.Contains("\r\n"))
            {
                s = String.Concat("\"", s, "\"");
            }

            return s;
        }

        internal static IList<string> extractQuotedStrings(string s)
        {
            IList<String> result = new List<String>();

            if (String.IsNullOrEmpty(s) || s.IndexOf('"') < 0)
            {
                return result;
            }

            Int32 currentPos = 0; 
            while (true)
            {
                currentPos = s.IndexOf('"', currentPos);
                if (currentPos < 0)
                {
                    break;
                }

                Int32 nextQuotePos = s.IndexOf('"', currentPos + 1);
                if (nextQuotePos < 0)
                {
                    break;
                }

                result.Add(s.Substring(currentPos + 1, (nextQuotePos - currentPos - 1)));
                currentPos = nextQuotePos + 1;
            }

            return result;
        }

        /// <summary>
        /// Extract all strings wrapped with start and end 
        /// </summary>
        /// <param name="s">e.g. this string [contains some] text wrapped [in] square brackets</param>
        /// <returns>"contains some", "in"</returns>
        internal static IList<string> extractWrappedStrings(String s, string startChar, String endChar)
        {
            IList<String> result = new List<String>();

            if (String.IsNullOrEmpty(s) || s.IndexOf(startChar) < 0 || s.IndexOf(endChar) < 0)
            {
                return result;
            }

            Int32 currentPos = 0;
            while (true)
            {
                currentPos = s.IndexOf(startChar, currentPos);
                if (currentPos < 0)
                {
                    break;
                }

                Int32 endCharPos = s.IndexOf(endChar, currentPos + 1);
                if (endCharPos < 0)
                {
                    break;
                }

                result.Add(s.Substring(currentPos + 1, (endCharPos - currentPos - 1)));
                currentPos = endCharPos + 1;
            }

            return result;
        }

        /// <summary>
        /// Extract the first instance of a string wrapped with start and end char. NOTE: this function is aware of quoted strings - if either the startChar or endChar 
        /// appear inside a quoted string then it will be ignored in favor of the next start/end char
        /// </summary>
        /// <param name="s">e.g. this string "[contains some]" text wrapped [in] square brackets</param>
        /// <returns>"in" -- note that "[contains some]" is wrapped in quotes and therefore ignored!</returns>
        internal static String extractWrappedString(String s, char startChar, char endChar)
        {
            if (String.IsNullOrEmpty(s) || s.IndexOf(startChar) < 0 || s.IndexOf(endChar) < 0)
            {
                return s;
            }

            StringBuilder sb = new StringBuilder();
            bool inQuotes = false;
            bool readingWrappedString = false;
            char previousChar = s[0];
            for (int i = 0; i < s.Length; i++)
            {
                char currentChar = s[i];
                if (inQuotes)
                {
                    if (currentChar == '"' && previousChar != '\\')
                    {
                        inQuotes = false;
                    }
                    continue;
                }

                if (readingWrappedString)
                {
                    if (currentChar == endChar && previousChar != '\\')
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(currentChar);
                        continue;
                    }
                }
                else // not reading quoted string... not in wrapped string... 
                {
                    if (currentChar == '"')
                    {
                        inQuotes = true;
                    }
                    else if (currentChar == startChar)
                    {
                        readingWrappedString = true;
                    }
                }
            }

            return s;
        }



        public static String trimLeading(String s, char charToTrim)
        {
            if (String.IsNullOrEmpty(s) || !s.StartsWith(charToTrim) || charToTrim == 0)
            {
                return s;
            }

            int startIdx = 0;
            while (s[startIdx] == charToTrim && startIdx < s.Length)
            {
                startIdx++;
            }

            return s.Substring(startIdx);
        }

        /// <summary>
        /// Parse a delimited string in to a case-insensitive dictionary. NOTE: this function is aware of quoted strings and escaped quotes inside quoted strings!!
        /// e.g. ID=AF,Number=A,Type=Float,Description="Allele Frequency"  --> ID:AF, Number:A, Type:Float, Description:Allele Frequency (without quotes!!) 
        /// </summary>
        /// <param name="delimitedString"></param>
        /// <param name="recordDelimiter"></param>
        /// <param name="keyValDelimiter"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> toCaseInsensitiveDictFromDelimited(string delimitedString, char recordDelimiter, char keyValDelimiter)
        {
            Dictionary<String, String> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (String.IsNullOrEmpty(delimitedString))
            {
                return result;
            }

            StringBuilder keySB = new StringBuilder();
            keySB.Append(delimitedString[0]);
            String currentKey = "";

            StringBuilder valueSB = new StringBuilder();

            bool inQuotes = false;
            if (delimitedString[0] == '"')
            {
                inQuotes = true;
            }
            bool readingKey = true;
            bool readingValue = false;
            char previousCharRead = delimitedString[0];

            for (int i = 1; i < delimitedString.Length; i++) // note starting at char pos 1 as we read first char into first key's value above
            {
                char currentChar = delimitedString[i];

                if (!inQuotes && currentChar == ' ') // ignore spaces if not quoted
                {
                    continue;
                }

                if (inQuotes)
                {
                    if (currentChar == '\\')
                    {
                        previousCharRead = currentChar;
                        continue;
                    }
                    if (currentChar == '"' && previousCharRead != '\\') // end quoted string!
                    {
                        inQuotes = false;
                        continue;
                    }
                    if (readingKey) keySB.Append(currentChar);
                    else if (readingValue) valueSB.Append(currentChar);
                    previousCharRead = currentChar;
                    continue;
                }

                if (currentChar == '"')
                {
                    inQuotes = true;
                    previousCharRead = currentChar;
                    continue;
                }

                if (currentChar == keyValDelimiter)
                {
                    readingKey = false;
                    readingValue = true;
                    if (keySB.Length == 0)
                    {
                        throw new ArgumentException("Malformed string - unable to parse in to key/value pairs: " + delimitedString);
                    }
                    currentKey = keySB.ToString();
                    keySB = new StringBuilder();
                    previousCharRead = currentChar;
                    continue;
                }

                if (currentChar == recordDelimiter)
                {
                    readingKey = true;
                    readingValue = false;
                    if (String.IsNullOrEmpty(currentKey))
                    {
                        throw new ArgumentException("Malformed string - unexpected record delimiter " + recordDelimiter.ToString() + " at position " + i.ToString() + " in string " + delimitedString);
                    }
                    if (result.ContainsKey(currentKey))
                    {
                        throw new ArgumentException("Unable to parse to dictionary - found multiple instances of key " + currentKey + " in string: " + delimitedString);
                    }
                    result.Add(currentKey, valueSB.ToString());
                    valueSB = new StringBuilder();
                    continue;
                }

                if (readingKey)
                {
                    keySB.Append(currentChar);
                }
                else if (readingValue)
                {
                    valueSB.Append(currentChar);
                }
            }

            // loop above misses adding last key/value pair - add last key/value if present
            if (!String.IsNullOrEmpty(currentKey) && valueSB.Length > 0 && ! result.ContainsKey(currentKey))
            {
                result.Add(currentKey, valueSB.ToString());
            }


            return result;
        }

        public static string getBase64String(string s)
        {
            return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(s));
        }

        // code taken partially from: https://www.codeproject.com/Articles/11556/Converting-Wildcards-to-Regexes
        /// <summary>
        /// Matches wildcard strings using '*' as the wildcard character - note match is case insensitive
        /// </summary>
        /// <param name="wildcardString">'For*example'</param>
        /// <param name="s">'For an example'</param>
        /// <returns>True for param example</returns>
        public static bool isWildcardMatch(String wildcardString, String s)
        {
            if (String.IsNullOrEmpty(s) || String.IsNullOrEmpty(wildcardString) || !wildcardString.Contains("*"))
            {
                return false;
            }
            String regexStr = "^" + System.Text.RegularExpressions.Regex.Escape(wildcardString).Replace("\\*", ".*") + "$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(regexStr, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return regex.IsMatch(s);
        }
    }
}