using System;
using System.Collections.Generic;
using System.IO;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class SqlTableConfigParser
    {
        Dictionary<String, SqlTableConfigMap> _configMaps;
        IList<FileInfo> _filesInDir;

        public SqlTableConfigParser(String configDirectory)
        {
            _filesInDir = new List<FileInfo>(new DirectoryInfo(configDirectory).GetFiles("SQL_MAP_*"));
        }

        public Dictionary<String, SqlTableConfigMap> parse(bool forceRefresh = false)
        {
            if (_configMaps != null && forceRefresh == false)
            {
                return _configMaps;
            }

            _configMaps = new Dictionary<string, SqlTableConfigMap>();
            SqlTableConfigMap current = null;

            foreach (FileInfo fi in _filesInDir)
            {
                String buf = com.bitscopic.hilleman.core.utils.FileIOUtils.readFile(fi.FullName);
                String[] bufLines = StringUtils.getLines(buf); //.split(buf, StringUtils.CRLF); // buf.Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < bufLines.Length; i++)
                {
                    if (String.IsNullOrEmpty(bufLines[i]) || bufLines[i].StartsWith("//") || bufLines[i].StartsWith("--"))
                    {
                        continue; // comments - ignore them
                    }
                    if (bufLines[i].Contains("[") && bufLines[i].Contains("]"))
                    {
                        if (current != null)
                        {
                            if (_configMaps.ContainsKey(current.vistaFileNumber))
                            {
                                throw new Exception("Invalid config files - config for file " + current.vistaFileNumber + " was specified more than once. Unable to continue...");
                            }
                            _configMaps.Add(current.vistaFileNumber, current);
                        }
                        current = new SqlTableConfigMap();
                        continue;
                    }
                    if (current == null)
                    {
                        continue;
                    }
                    if (String.IsNullOrEmpty(bufLines[i]) || !bufLines[i].Contains("="))
                    {
                        continue;
                    }

                    String key = (bufLines[i].Split(new char[] { '=' })[0].Trim()).ToUpper();
                    String value = bufLines[i].Substring(bufLines[i].IndexOf('=') + 1).Trim(); // everything after the first '='

                    switch (key)
                    {
                        case "FILENUMBER":
                            current.vistaFileNumber = value;
                            break;
                        case "FILENAME":
                            current.vistaFileName = value;
                            break;
                        case "FIELDNUMBERS":
                            current.vistaFields = value;
                            current.vistaFieldsParsed = value.Split(new char[] { ';' });
                            break;
                        case "SQLTABLENAME":
                            current.sqlTableName = value;
                            break;
                        case "SQLCOLUMNS":
                            if (value.Contains(":"))
                            {
                                current.sqlSpecialColumnsParsed = getSpecialColumns(value);
                                value = getVarChar256FieldsString(value);
                            }
                            current.sqlColumns = value;
                            current.sqlColumnsParsed = value.Split(new char[] { ';' });
                            break;
                        case "SUBFILE":
                            current.subFile = Boolean.Parse(value);
                            break;
                        default:
                            break;
                    }
                }
            }

            // save the last config!
            if (current != null)
            {
                if (_configMaps.ContainsKey(current.vistaFileNumber))
                {
                    throw new Exception("Invalid config files - config for file " + current.vistaFileNumber + " was specified more than once. Unable to continue...");
                }
                _configMaps.Add(current.vistaFileNumber, current);
            }

            return _configMaps;
        }

        internal Dictionary<string, string> getSpecialColumns(string value)
        {
            Int32 firstColonIdx = value.IndexOf(':');
            if (firstColonIdx < 0)
            {
                return null;
            }
            String[] tmpPieces = value.Split(new char[] { ':' });
            Int32 previousSemicolonIdx = tmpPieces[0].LastIndexOf(';');
            String pieceWithSpecialTypes = value.Substring(previousSemicolonIdx + 1);
            String[] piecesWithSpecialTypes = pieceWithSpecialTypes.Split(new char[] { ';' });

            Dictionary<String, String> result = new Dictionary<string, string>();
            foreach (String piece in piecesWithSpecialTypes)
            {
                String[] fieldNameAndType = piece.Split(new char[] { ':' });
                result.Add(fieldNameAndType[0], fieldNameAndType[1]);
            }
            return result;
        }

        internal string getVarChar256FieldsString(string value)
        {
            Int32 firstColonIdx = value.IndexOf(':');
            if (firstColonIdx < 0)
            {
                return value;
            }
            String[] tmpPieces = value.Split(new char[] { ':' });
            Int32 previousSemicolonIdx = tmpPieces[0].LastIndexOf(';');
            return value.Substring(0, previousSemicolonIdx);
        }
    }

}