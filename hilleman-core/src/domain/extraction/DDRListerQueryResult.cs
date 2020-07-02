using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.domain.exception.vista;
using com.bitscopic.hilleman.core.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.bitscopic.hilleman.core.domain.extraction
{
    public class DDRListerQueryResult
    {
        public Dictionary<String, Int32> columnIndexByName;
        public List<Object[]> parsedWithIENSiteTimestamp;
        public Dictionary<String, List<String>> subfileIENsWithDataBySubfile;
        public String lastIEN;

        public DDRListerQueryResult() { }

        public object getValue(String columnName, object[] record)
        {
            if (columnIndexByName == null || columnIndexByName.Count == 0)
            {
                throw new ArgumentException("The column name index has not been set up!");
            }
            if (!columnIndexByName.ContainsKey(columnName))
            {
                throw new ArgumentException(columnName + " was not found in the column name dictionary!");
            }

            Int32 colIdx = this.columnIndexByName[columnName];
            if (record == null || record.Length < colIdx)
            {
                throw new ArgumentException("Record does not appear to contain column " + columnName);
            }

            return record[colIdx];
        }

        public object getValue(String columnName, Int32 rowIndex)
        {
            if (this.parsedWithIENSiteTimestamp == null || this.parsedWithIENSiteTimestamp.Count < rowIndex)
            {
                throw new ArgumentException("Invalid row index!");
            }

            return this.getValue(columnName, this.parsedWithIENSiteTimestamp[rowIndex]);
        }

        public void setFieldNameIndices(IEnumerable<String> fieldNames, bool isSubfile = false)
        {
            this.columnIndexByName = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            if (isSubfile)
            {
                this.columnIndexByName.Add("P_IEN", 0);
            }
            this.columnIndexByName.Add("IEN", this.columnIndexByName.Count);
            this.columnIndexByName.Add("SITECODE", this.columnIndexByName.Count);
            this.columnIndexByName.Add("TIMESTAMP", this.columnIndexByName.Count);
            foreach (String field in fieldNames)
            {
                this.columnIndexByName.Add(field, this.columnIndexByName.Count);
            }
        }

        public static DDRListerQueryResult buildResult(String vistaFile, String fromArg, ReadRangeResponse response, List<String> fieldsList, String targetSite, DateTime dataTimestamp, bool isSubfile = false, String parentIENSString = null, bool parseIdentifierFlags = false, List<String> subfiles = null)
        {
            DDRListerQueryResult parsed = new DDRListerQueryResult();

            List<Object[]> result = new List<object[]>();

            if (response == null || response.value == null || response.value.Count == 0)
            {
                return parsed;
            }

            String enDdiolChar = "&#94;";

            String maxIEN = "0";
            Decimal maxIENAsDecimal = 0;
            foreach (String line in response.value)
            {
                String idParamResult = "";
                String adjustedLine = line;
                if (line.IndexOf(enDdiolChar) > 0)
                {
                    idParamResult = line.Substring(line.IndexOf(enDdiolChar) + 5);
                    adjustedLine = line.Substring(0, line.IndexOf(enDdiolChar));
                }

                String[] pieces = StringUtils.split(adjustedLine, '^');
                if (pieces == null || pieces.Length < fieldsList.Count + 1)
                {
                    continue;
                }
                String ien = pieces[0];
                String iensString = ien;
                String correctedParentIENSString = null;

                // VistA did not return the 'String enDdiolChar = "&#94;";' constant for the ID param result separator and instead used a carat - use this logic to get those flags
                if (parseIdentifierFlags && subfiles != null && subfiles.Count > 0 && pieces.Length > (fieldsList.Count + 1) && String.IsNullOrEmpty(idParamResult))
                {
                    idParamResult = StringUtils.join(pieces.TakeLast(subfiles.Count).ToArray(), "^");
                }

                if (isSubfile)
                {
                    correctedParentIENSString = parentIENSString.Replace(",", "_");
                    if (correctedParentIENSString.StartsWith("_"))
                    {
                        correctedParentIENSString = correctedParentIENSString.Substring(1);
                    }
                    if (correctedParentIENSString.EndsWith("_"))
                    {
                        correctedParentIENSString = correctedParentIENSString.Substring(0, correctedParentIENSString.Length - 1);
                    }

                    iensString = String.Concat(ien, "_", correctedParentIENSString);
                }

                Decimal dIEN = 0;
                if (Decimal.TryParse(ien, out dIEN) && dIEN > maxIENAsDecimal)
                {
                    maxIEN = ien;
                }

                if (parseIdentifierFlags && !String.IsNullOrEmpty(idParamResult) && subfiles != null)
                {
                    String[] idParamValuesSplit = StringUtils.split(idParamResult.Replace(enDdiolChar, "^"), '^');
                    if (idParamValuesSplit.Length != subfiles.Count)
                    {
                        throw new ArgumentException("Unable to parse DDR LISTER results - the IDENTIFIER params count does not match the number of subfiles specified");
                    }

                    if (parsed.subfileIENsWithDataBySubfile == null) // only need to do this first time through - null checks are virtually cost free so leaving inside this IF statement inside loop to avoid duplicating check for whether subfiles are defined
                    {
                        parsed.subfileIENsWithDataBySubfile = new Dictionary<string, List<string>>();
                        for (int i = 0; i < subfiles.Count; i++)
                        {
                            parsed.subfileIENsWithDataBySubfile.Add(subfiles[i], new List<string>());
                        }
                    }

                    for (int i = 0; i < subfiles.Count; i++)
                    {
                        if (!String.Equals(idParamValuesSplit[i], "0"))
                        {
                            parsed.subfileIENsWithDataBySubfile[subfiles[i]].Add(ien);
                        }
                    }
                }

                if (isSubfile)
                {
                    object[] record = new object[fieldsList.Count + 4];
                    record[0] = correctedParentIENSString;
                    record[1] = iensString;
                    record[2] = targetSite;
                    record[3] = dataTimestamp;
                    for (int i = 0; i < fieldsList.Count; i++)
                    {
                        record[i + 4] = pieces[i + 1]; // pieces[i + 1] to skip IEN
                    }
                    result.Add(record);
                }
                else
                {
                    object[] record = new object[fieldsList.Count + 3];
                    record[0] = ien;
                    record[1] = targetSite;
                    record[2] = dataTimestamp;
                    for (int i = 0; i < fieldsList.Count; i++)
                    {
                        record[i + 3] = pieces[i + 1]; // pieces[i + 1] to skip IEN
                    }
                    result.Add(record);
                }
            }

            parsed.parsedWithIENSiteTimestamp = result;

            if (!isSubfile && maxIEN == "0" && parsed.parsedWithIENSiteTimestamp != null && parsed.parsedWithIENSiteTimestamp.Count > 0)
            {
                throw new IENZeroException(vistaFile, fromArg, printRecord(parsed.parsedWithIENSiteTimestamp[parsed.parsedWithIENSiteTimestamp.Count - 1]));
            }
            parsed.lastIEN = maxIEN;

            return parsed;
        }

        internal static string printRecord(object[] v)
        {
            if (v is null || v.Length == 0)
            {
                return "";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(v[0]).ToString();
            for (int i = 1; i < v.Length; i++)
            {
                sb.Append("|");
                sb.Append(v[i].ToString());
            }
            return sb.ToString();
        }
    }
}
