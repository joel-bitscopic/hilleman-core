using System;
using System.Security.Cryptography;
using System.IO;

namespace com.bitscopic.hilleman.core.utils
{
    public static class CryptographyUtils
    {
        public static String getNCharRandom(Int32 length)
        {
            String chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] result = new char[length];
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new String(result);
        }

        /// <summary>
        /// Create a random key, HMAC 256 hash it, base 64 encode the result
        /// </summary>
        /// <returns></returns>
        public static String createRandomHashBase64()
        {
            byte[] key = new byte[64];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
                using (HMACSHA256 hmac = new HMACSHA256())
                {
                    byte[] hmacHashed = hmac.ComputeHash(key);
                    return Convert.ToBase64String(hmacHashed);
                }
            }
        }

        /// <summary>
        /// Binary serialize an object, compute it's HMAC256 hash, base 64 encode the result
        /// </summary>
        /// <param name="key">Secret key</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static String hmac256Hash(String key, object target)
        {
            MemoryStream ms = SerializerUtils.serializeToStream(target);
            using (HMACSHA256 hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(key)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(ms));
            }
        }

        public static String sha256Hash(String target)
        {
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            byte[] crypto = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(target), 0, System.Text.Encoding.UTF8.GetByteCount(target));
            foreach (byte b in crypto)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static String sha256HashBase64Encoded(String target)
        {
            System.Security.Cryptography.SHA256Managed sha256 = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            byte[] crypto = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(target), 0, System.Text.Encoding.UTF8.GetByteCount(target));
            return Convert.ToBase64String(crypto);
        }
    }
}