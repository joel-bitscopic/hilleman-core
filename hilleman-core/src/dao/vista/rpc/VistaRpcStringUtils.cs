using System;
using System.Collections.Generic;
using System.Text;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public static class VistaRpcStringUtils
    {
        public static string strPack(string s, int n)
        {
            int lth = s.Length;
            StringBuilder result = new StringBuilder(lth.ToString());
            while (result.Length < n)
            {
                result.Insert(0, "0");
            }
            return result + s;
        }

        public static string varPack(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                s = "0";
            }
            StringBuilder b = new StringBuilder();
            b.Append('|');
            b.Append(Convert.ToChar(s.Length));
            b.Append(s);
            return b.ToString();
        }

        public static string LPack(string s, int ndigits)
        {
            int lth = (String.IsNullOrEmpty(s) ? 0 : s.Length);
            string sLth = Convert.ToString(lth);
            int width = sLth.Length;
            if (ndigits < width)
            {
                throw new ArgumentException("Too few digits");
            }
            string result = "000000000" + Convert.ToString(lth);
            result = result.Substring(result.Length - ndigits) + s;
            return result;
        }

        public static string SPack(string s)
        {
            int lth = s.Length;
            if (lth > 255)
            {
                throw new ArgumentException("Parameter exceeds 255 chars");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToChar(lth));
            sb.Append(s);
            return sb.ToString();
        }

        public static String encrypt(String inString)
        {
            string[] cipherPad = System.IO.File.ReadAllLines("CIPHER_PAD.txt");
            return VistaRpcStringUtils.encrypt(inString, cipherPad);
        }

        public static string encrypt(string inString, string[] cipherPad)
        {
            const int MAXKEY = 19;
            Random r = new Random();
            int associatorIndex = r.Next(MAXKEY);
            int identifierIndex = r.Next(MAXKEY);
            while (associatorIndex == identifierIndex)
            {
                identifierIndex = r.Next(MAXKEY);
            }
            string xlatedString = "";
            for (int i = 0; i < inString.Length; i++)
            {
                char inChar = inString[i];
                int pos = cipherPad[associatorIndex].IndexOf(inChar);
                if (pos == -1)
                {
                    xlatedString += inChar;
                }
                else
                {
                    xlatedString += cipherPad[identifierIndex][pos];
                }
            }
            return (char)(associatorIndex + 32) +  xlatedString + (char)(identifierIndex + 32);
        }

        internal static String convertListToString(Dictionary<string, string> lst)
        {
            if (lst == null || lst.Count == 0)
            {
                return VistaRpcStringUtils.LPack("", 3) + 'f';
            }
            StringBuilder result = new StringBuilder();
            foreach (String key in lst.Keys) // (int i = 0; i < lst.Count; i++)
            {
                string value = lst[key];
                if (String.IsNullOrEmpty(value))
                {
                    value = "\u0001";
                }
                result.Append(VistaRpcStringUtils.LPack(key, 3));
                result.Append(VistaRpcStringUtils.LPack(value, 3));
                result.Append('t');
            }
            result = result.Remove(result.Length - 1, 1);
            result.Append('f');
            return result.ToString();
        }
    }
}