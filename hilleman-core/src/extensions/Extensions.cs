using System;
using System.Collections.Generic;
using System.Linq;

namespace com.bitscopic.hilleman.core.extensions
{
    public static class Extensions
    {
        public static bool CaseInsensitiveContains(this IEnumerable<String> collection, String value)
        {
            foreach (String s in collection)
            {
                if (String.Equals(s, value, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CaseInsensitiveContainsKey(this Dictionary<String, Int32> collection, String key)
        {
            if (collection.Keys.ToList().CaseInsensitiveContains(key))
            {
                return true;
            }

            return false;
        }

        public static bool CaseInsensitiveContainsKey(this Dictionary<String, Object> collection, String key)
        {
            if (collection.Keys.ToList().CaseInsensitiveContains(key))
            {
                return true;
            }

            return false;
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }
}