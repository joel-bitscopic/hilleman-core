using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista;
using System.Collections.Concurrent;
using com.bitscopic.hilleman.core.dao;

namespace com.bitscopic.hilleman.core.utils
{
    public static class LookupTableUtils
    {
        private static ConcurrentDictionary<String, Dictionary<String, String>> _lookupTableBySite = new ConcurrentDictionary<string, Dictionary<string, string>>();
        private static ConcurrentDictionary<String, IList<KeyValuePair<String, String>>> _sortedLookupTableBySite = new ConcurrentDictionary<string, IList<KeyValuePair<string, string>>>();

        public static void refreshLookupTable(IVistaConnection cxn, String vistaFileNumber)
        {
            String compositeKey = String.Concat(cxn.getSource().id, "_", vistaFileNumber);

            if (_lookupTableBySite.ContainsKey(compositeKey))
            {
                Dictionary<String, String> trash = new Dictionary<String, String>();
                _lookupTableBySite.TryRemove(compositeKey, out trash);
                trash = null;
            }

            getLookupTable(cxn, vistaFileNumber);
        }

        public static void refreshSortedLookupTable(IVistaConnection cxn, String vistaFileNumber)
        {
            String compositeKey = String.Concat(cxn.getSource().id, "_", vistaFileNumber);

            if (_sortedLookupTableBySite.ContainsKey(compositeKey))
            {
                IList<KeyValuePair<String, String>> trash = new List<KeyValuePair<String, String>>();
                _sortedLookupTableBySite.TryRemove(compositeKey, out trash);
                trash = null;
            }

            getSortedLookupTable(cxn, vistaFileNumber);
        }

        public static Dictionary<String, String> getLookupTable(IVistaConnection cxn, String vistaFileNumber, String screenCode = "")
        {
            String compositeKey = String.Concat(cxn.getSource().id, "_", vistaFileNumber);
            if (_lookupTableBySite.ContainsKey(compositeKey))
            {
                return LookupTableUtils._lookupTableBySite[compositeKey];
            }
            else
            {
                ReadRangeRequest request = new ReadRangeRequest(cxn.getSource());
                request.setFile(vistaFileNumber);
                request.setFields(".01");
                if (!String.IsNullOrEmpty(screenCode))
                {
                    request.setScreenParam(screenCode);
                }
                ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(cxn).readRange(request);
                if (response.value == null || response.value.Count == 0)
                {
                    Dictionary<String, String> emptyDict = new Dictionary<string, string>();
                    LookupTableUtils._lookupTableBySite.TryAdd(compositeKey, emptyDict); // empty lookup table! seems nothing in this file at this site!
                    return emptyDict;
                }
                Dictionary<String, String> parsed = new Dictionary<string, string>();
                for (int i = 0; i < response.value.Count; i++)
                {
                    String[] pieces = StringUtils.split(response.value[i], StringUtils.CARAT);
                    if (pieces != null && pieces.Length > 1)
                    {
                        parsed.Add(pieces[0], pieces[1]);
                    }
                }

                LookupTableUtils._lookupTableBySite.TryAdd(compositeKey, parsed);
                return parsed;
            }
        }

        public static IList<KeyValuePair<String, String>> getSortedLookupTable(IVistaConnection cxn, String vistaFileNumber, String screenCode = "")
        {
            // SortedList seems to be really, really slow... so caching the sorted lists in a separate collection
            String compositeKey = String.Concat(cxn.getSource().id, "_", vistaFileNumber);
            if (_sortedLookupTableBySite.ContainsKey(compositeKey))
            {
                return LookupTableUtils._sortedLookupTableBySite[compositeKey];
            }

            Dictionary<String, String> lookupTable = LookupTableUtils.getLookupTable(cxn, vistaFileNumber, screenCode);
            SortedList<String, KeyValuePair<String, String>> sorted = new SortedList<string, KeyValuePair<string, string>>();
            foreach (String key in lookupTable.Keys)
            {
                String value = lookupTable[key];
                sorted.Add(value, new KeyValuePair<string, string>(key, value));
            }

            // add to cache
            LookupTableUtils._sortedLookupTableBySite.TryAdd(compositeKey, sorted.Values);
            return sorted.Values; 
        }

        public static IList<KeyValuePair<String, String>> getNEntriesFromLookupTable(IVistaConnection cxn, String vistaFileNumber, String target, Int32 maxRex, String screenCode = "")
        {
            IList<KeyValuePair<String, String>> sorted = LookupTableUtils.getSortedLookupTable(cxn, vistaFileNumber, screenCode);

            IList<KeyValuePair<String, String>> result = new List<KeyValuePair<String, String>>();
            bool inIt = false;
            if (String.IsNullOrEmpty(target))
            {
                inIt = true;
            }
            for (int i = 0; i < sorted.Count; i++)
            {
                if (inIt)
                {
                    if (result.Count >= maxRex)
                    {
                        break;
                    }
                    result.Add(new KeyValuePair<String, String>(sorted[i].Key, sorted[i].Value));
                    continue;
                }

                if (target.CompareTo(sorted[i].Value) <= 0)
                {
                    inIt = true;
                    result.Add(new KeyValuePair<String, String>(sorted[i].Key, sorted[i].Value)); // and add the first one!
                }
            }

            return result;
        }
    }
}