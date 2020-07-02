using System;
using System.Collections.Generic;
using System.Linq;

namespace com.bitscopic.hilleman.core.domain.to
{
    [Serializable]
    public class VistaRecord : BaseClass
    {
        public VistaFile file;
        public List<VistaField> fields;
        public String iens;
        public bool exactIens;

        public Dictionary<String, String> convertFieldsToDict(bool useExternalValues = false)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();

            if (fields == null || fields.Count == 0)
            {
                return result;
            }

            foreach (VistaField vf in fields)
            {
                if (String.IsNullOrEmpty(vf.number) || result.ContainsKey(vf.number))
                {
                    continue;
                }

                if (useExternalValues && !String.IsNullOrEmpty(vf.externalValue))
                {
                    result.Add(vf.number, vf.externalValue);
                }
                else
                {
                    result.Add(vf.number, vf.value);
                }
            }

            return result;
        }

        public String safeGetFieldValue(String fieldNo, bool externalValue = false)
        {
            if (this.fields != null && this.fields.Any(vf => vf.number == fieldNo))
            {
                if (externalValue)
                {
                    return this.fields.First(vf => vf.number == fieldNo).externalValue;
                }
                else
                {
                    return this.fields.First(vf => vf.number == fieldNo).value;
                }
            }

            return null;
        }

        /// <summary>
        /// If external value is available and not equal to the internal value, returns combined string formatted as: internal-external
        /// 
        /// If external value is not present, simply returns internal value
        /// </summary>
        /// <param name="fieldNo"></param>
        /// <returns></returns>
        public String safeGetCombinedFieldValue(String fieldNo)
        {
            if (this.fields != null && this.fields.Any(vf => vf.number == fieldNo))
            {
                String internalValue = this.fields.First(vf => vf.number == fieldNo).value;
                String externalValue = this.fields.First(vf => vf.number == fieldNo).externalValue;
                if (!String.IsNullOrEmpty(externalValue) && !String.Equals(internalValue, externalValue))
                {
                    return String.Concat(internalValue, "-", externalValue);
                }
                else
                {
                    return internalValue;
                }
            }

            return null;
        }

    }
}