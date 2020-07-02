using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.utils
{
    public static class DictionaryUtils
    {
        /// <summary>
        /// A simple utility function for fetching a value from a dictionary. Returns a blank string if the key is not present
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public  static String safeGet(Dictionary<String, String> dict, String key)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return String.Empty;
        }
    }
}