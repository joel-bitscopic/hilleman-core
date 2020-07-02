using com.bitscopic.hilleman.core.domain;
using System;
using System.IO;
using System.Text;

namespace com.bitscopic.hilleman.core.utils
{
    public static class FileIOUtils
    {
        public static byte[] readBinaryFile(String fullPath)
        {
            FileInfo fi = new FileInfo(fullPath);
            using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] fullBuf = new byte[fi.Length];
                fs.Read(fullBuf, 0, Convert.ToInt32(fi.Length));
                fs.Flush();
                fs.Close();
                return fullBuf;
            }
        }

        public static String readFile(String fullPath)
        {
            return Encoding.UTF8.GetString(FileIOUtils.readBinaryFile(fullPath));
        }

        public static void writeFile(String fileContents, string fullPath)
        {
            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                byte[] temp = Encoding.UTF8.GetBytes(fileContents);
                fs.Write(temp, 0, temp.Length);
                fs.Flush();
            }
        }


        public static void writeFile(FileSystemFile file, String fullFilePath = null)
        {
            using (FileStream fs = new FileStream(String.IsNullOrEmpty(fullFilePath) ? file.fileName : fullFilePath, FileMode.Create))
            {
                fs.Write(file.data, 0, file.size);
                fs.Flush();
            }
        }


        internal static void writePDF(string path, domain.PDF result)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(result.data, 0, result.size);
                fs.Flush();
            }
        }

        public static void appendToFile(String fullFilePath, String s, bool addLineFeedFirst = true)
        {
            using (FileStream fs = new FileStream(fullFilePath, FileMode.Append))
            {
                byte[] temp = Encoding.UTF8.GetBytes((addLineFeedFirst ? StringUtils.CRLF : "") + s);
                fs.Write(temp, 0, temp.Length);
                fs.Flush();
            }
        }

        static readonly object _fileAppendLocker = new object();
        public static void appendToFileThreadSafe(String fullFilePath, String s, bool addLineFeedFirst = true)
        {
            lock (_fileAppendLocker)
            {
                using (FileStream fs = new FileStream(fullFilePath, FileMode.Append))
                {
                    byte[] temp = Encoding.UTF8.GetBytes((addLineFeedFirst ? StringUtils.CRLF : "") + s);
                    fs.Write(temp, 0, temp.Length);
                    fs.Flush();
                }
            }
        }
    }
}