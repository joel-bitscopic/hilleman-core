using System;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using com.bitscopic.hilleman.core.domain.exception;

namespace com.bitscopic.hilleman.core.utils
{
    public static class SerializerUtils
    {
        public static JObject deserializeToJObj(String s)
        {
            if (String.IsNullOrEmpty(s))
            {
                s = "{}";
            }
            return Newtonsoft.Json.Linq.JObject.Parse(s);
        }

        public static JArray deserializeToJArray(String s)
        {
            if (String.IsNullOrEmpty(s))
            {
                s = "[]";
            }
            return Newtonsoft.Json.Linq.JArray.Parse(s);
        }

        public static String serializeForPrinting(object arg, bool includeNulls = true)
        {
            if (arg is String)
            {
                return (String)arg;
            }
            if (!includeNulls)
            {
                return JsonConvert.SerializeObject(arg, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            return JsonConvert.SerializeObject(arg, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static String serialize(object arg, bool includeNulls = true)
        {
            if (arg is String)
            {
                return (String)arg;
            }
            //return JsonConvert.SerializeObject(arg, SerializerUtils.SERIALIZER_SETTINGS);
            if (!includeNulls)
            {
                return JsonConvert.SerializeObject(arg, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
            return JsonConvert.SerializeObject(arg, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static MemoryStream serializeToStream(object arg, bool includeNulls = true)
        {
            String serialized = SerializerUtils.serialize(arg, includeNulls);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serialized));
        }

        public static T deserialize<T>(String s)
        {
            if (typeof(T) == typeof(String))
            {
                return (T)(object)s;
            }
            return JsonConvert.DeserializeObject<T>(s);
        }

        public static object deserializeToObject(String s)
        {
            return JsonConvert.DeserializeObject(s);
        }

        public static T deserializeFromStream<T>(Stream s)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                string textToDeserialize = sr.ReadToEnd();
                if (typeof(T) == typeof(String))
                {
                    return (T)(object)textToDeserialize;
                }
                return JsonConvert.DeserializeObject<T>(textToDeserialize);
            }
        }

        public static string serializeExceptionSimple(Exception exc, bool includeStack = false)
        {
            JObject jobj = new JObject();
            jobj.Add("Message", exc.Message);
            jobj.Add("HResult", exc.HResult);
            jobj.Add("Inner", exc.InnerException == null ? "<null>" : exc.InnerException.Message);
            if (includeStack)
            {
                jobj.Add("Stack", exc.StackTrace);
            }

            if (exc is HillemanBaseException)
            {
                HillemanBaseException hbe = (HillemanBaseException)exc;
                jobj.Add("errorCode", hbe.errorCode);
                jobj.Add("extraInfo", SerializerUtils.serialize(hbe.extraInfo, false));
            }

            return jobj.ToString();
        }

        public static bool looksLikeJson(String s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return false;
            }

            if (s.StartsWith("{") && s.EndsWith("}") ||
                s.StartsWith("[") && s.EndsWith("]"))
            {
                return true;
            }

            return false;
        }
    }
}