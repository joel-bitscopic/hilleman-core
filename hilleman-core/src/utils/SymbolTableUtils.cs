using System;
using System.Collections.Generic;
using System.Text;

namespace com.bitscopic.hilleman.core.utils
{
    public static class SymbolTableUtils
    {
        public static Dictionary<String, String> deserialize(String serializedSymbolTable)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(serializedSymbolTable))
            {
                return result;
            }

            String[] pieces = serializedSymbolTable.Split(new String[] { "\x1e" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (String symbolAndValue in pieces)
            {
                String[] keyAndValue = symbolAndValue.Split(new String[] { "\x1f" }, StringSplitOptions.None);
                result.Add(keyAndValue[0], keyAndValue[1]);
            }

            return result;
        }

        public static String serialize(Dictionary<String, String> deserializedSymbolTable)
        {
            StringBuilder sb = new StringBuilder();
            foreach (String key in deserializedSymbolTable.Keys)
            {
                sb.Append(key);
                sb.Append("\x1f");
                sb.Append(deserializedSymbolTable[key]);
                sb.Append("\x1e");
            }
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - ("\x1e").Length, 1); // remove last RS character
            }
            return sb.ToString();
        }

        /// <summary>
        /// Not all values in the symbol table should be restored when connection pooling. For example, the values specifying the current
        /// process ID will be incorrect if applied to a connection attached to a process with a different ID. 
        /// </summary>
        /// <param name="serializedSymbolTable"></param>
        /// <returns></returns>
        public static String removeIgnoredSymbols(String serializedSymbolTable)
        {
            Dictionary<String, String> deserialized = SymbolTableUtils.deserialize(serializedSymbolTable);

            IList<String> ignoreExact = new List<String>() { "IO" }; // TODO - make these configurable somehow
            IList<String> ignorePatternMatch = new List<String>() { "IO(*" }; // TODO - make these configurable somehow
            IList<String> keysToRemove = new List<String>();

            // remove all exact matches
            foreach (String s in ignoreExact)
            {
                if (deserialized.ContainsKey(s))
                {
                    keysToRemove.Add(s);
                }
            }

            // remove all pattern matches if not already slated for removal
            foreach (String key in ignorePatternMatch)
            {
                String adjusted = key.Replace("*", "");
                foreach (String dictKey in deserialized.Keys)
                {
                    if (dictKey.StartsWith(adjusted) && !keysToRemove.Contains(dictKey))
                    {
                        keysToRemove.Add(dictKey);
                    }
                }
            }

            // do the remove from symbol table
            foreach (String key in keysToRemove)
            {
                deserialized.Remove(key);
            }

            // serialize before returning
            return SymbolTableUtils.serialize(deserialized);
        }
    }
}