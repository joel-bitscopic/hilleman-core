using System;
using com.bitscopic.hilleman.core.dao;
using System.Data;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.sql;

namespace com.bitscopic.hilleman.core.utils
{
    public static class SqlUtils
    {

        /// <summary>
        /// Adjust a string for direct insert in to a SQL database. For example, escape single quote with an additional single quote
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static string adjustStringForSql(string s, bool wrapWithSingleQuote = false)
        {
            String result = s.Replace("'", "''");

            if (wrapWithSingleQuote)
            {
                result = String.Concat("'", result, "'");
            }

            return result;
        }

        /// <summary>
        /// Turn a DataTable (from cached SQL data) in to a ReadRangeResponse
        /// </summary>
        /// <param name="request"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ReadRangeResponse convertDataTableToReadRangeResponse(ReadRangeRequest request, DataTable table)
        {
            ReadRangeResponse response = new ReadRangeResponse();
            response.value = new List<String>();
            foreach (DataRow row in table.Rows)
            {
                String currentLine = StringUtils.joinObjectArray(row.ItemArray, "^");

                response.value.Add(currentLine);
            }
            return response;
        }

        /// <summary>
        /// Turn a DataTable (from cached SQL data) in to a ReadResponse
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static ReadResponse convertDataTableToReadResponse(ReadRequest request, DataTable table, SqlVistaFieldTranslator tx)
        {
            ReadResponse response = new ReadResponse();
            
            Dictionary<String, String> requestedFieldsAndValues = new Dictionary<string, string>();
            String fieldsString = request.getFields();
            IList<String> requestedFieldsList = StringUtils.splitToList(fieldsString, StringUtils.SEMICOLON);

            String vistaFile = request.getFile();
            String ien = request.getIens();

            SqlTableConfigMap map = tx.getConfigMap(vistaFile);
            Dictionary<String, String> fieldColDict = tx.buildVistaFieldKeySqlColumnValueDict(
                map.vistaFieldsParsed, 
                map.sqlColumnsParsed, 
                map.sqlSpecialColumnsParsed == null ? null : new List<String>(map.sqlSpecialColumnsParsed.Keys));

            response.value = new List<String>();
            response.value.Add("[DATA]"); // DDR GETS ENTRY DATA always comes back with one of these in the first line

            foreach (String field in requestedFieldsList)
            {
                String columnNameForFieldNo = fieldColDict[field];
                response.value.Add(String.Format("{0}^{1}^{2}^{3}^{4}", vistaFile, ien, field, table.Rows[0][columnNameForFieldNo], table.Rows[0][columnNameForFieldNo]));
            }

            return response;
        }

        /// <summary>
        /// Fetch all values in a column
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal static List<string> getColumnFromReader(IDataReader rdr, String columnName)
        {
            List<String> result = new List<String>();

            Int32 colPos = -1;
            while (rdr.Read())
            {
                if (colPos < 0)
                {
                    colPos = rdr.GetOrdinal(columnName);
                }

                result.Add(rdr.GetString(colPos));
            }

            return result;
        }

        internal static DateTime getDateTimeByCxn(IDataReader rdr, String columnName, ISqlConnection cxn)
        {
            Int32 colIdx = rdr.GetOrdinal(columnName);
            if (colIdx < 0 || rdr.IsDBNull(colIdx))
            {
                return new DateTime();
            }

            if (rdr.IsDBNull(colIdx))
            {
                return new DateTime();
            }

            if (String.Equals("Oracle", cxn.getProvider(), StringComparison.CurrentCultureIgnoreCase))
            {
                return (DateTime)rdr.GetValue(colIdx);
            }
            else if (String.Equals("SQLite", cxn.getProvider(), StringComparison.CurrentCultureIgnoreCase))
            {
                if (rdr[colIdx].GetType() == typeof(DateTime))
                {
                    return (DateTime)rdr.GetValue(colIdx);
                }
                else
                {
                    return DateUtils.parseDateTime((String)rdr.GetValue(colIdx), TimeZoneInfo.Utc);
                }
            }

            throw new NotImplementedException("Date fetching/parsing has for " + cxn.getProvider() + " has not been implemented");
        }

        internal static Double safeGetDouble(IDataReader rdr, String columnName, ISqlConnection cxn)
        {
            Int32 colIdx = rdr.GetOrdinal(columnName);
            if (colIdx < 0 || rdr.IsDBNull(colIdx))
            {
                return 0;
            }

            if (String.Equals("SQLite", cxn.getProvider(), StringComparison.CurrentCultureIgnoreCase))
            {
                if (rdr[colIdx].GetType() == typeof(Double))
                {
                    return (Double)rdr.GetValue(colIdx);
                }
                else if (rdr[colIdx].GetType() == typeof(String))
                {
                    return Convert.ToDouble((String)rdr.GetValue(colIdx));
                }
                else if (rdr[colIdx].GetType() == typeof(int))
                {
                    return Convert.ToDouble((Int64)rdr.GetValue(colIdx));
                }
            }
            else if (String.Equals("Oracle", cxn.getProvider(), StringComparison.CurrentCultureIgnoreCase))
            {
                return Convert.ToDouble((Decimal)rdr.GetValue(colIdx));
            }

            throw new NotImplementedException("Double fetching/parsing has for " + cxn.getProvider() + " has not been implemented");
        }

        internal static Int64 safeGetInt(IDataReader rdr, String columnName, ISqlConnection cxn)
        {
            Int32 colIdx = rdr.GetOrdinal(columnName);
            if (colIdx < 0 || rdr.IsDBNull(colIdx))
            {
                return 0;
            }

            if (String.Equals("SQLite", cxn.getProvider(), StringComparison.CurrentCultureIgnoreCase))
            {
                return Convert.ToInt64(rdr.GetValue(colIdx));
            }
            else if (String.Equals("Oracle", cxn.getProvider(), StringComparison.CurrentCultureIgnoreCase))
            {
                return Convert.ToInt64((Decimal)rdr.GetValue(colIdx));
            }

            throw new NotImplementedException("Integer fetching/parsing has for " + cxn.getProvider() + " has not been implemented");
        }



        internal static string safeGet(IDataReader rdr, string columnName)
        {
            Int32 colIdx = rdr.GetOrdinal(columnName);
            if (colIdx < 0 || rdr.IsDBNull(colIdx))
            {
                return null;
            }

            return (String)rdr.GetValue(colIdx);
        }

        internal static T safeGet<T>(IDataReader rdr, string columnName)
        {
            Int32 colIdx = rdr.GetOrdinal(columnName);
            if (colIdx < 0 || rdr.IsDBNull(colIdx))
            {
                return default(T);
            }

            return (T)rdr.GetValue(colIdx);
        }

        internal static string safeGet(IDataReader rdr, int columnIndex)
        {
            if (rdr.IsDBNull(columnIndex))
            {
                return null;
            }
            return (String)rdr.GetValue(columnIndex);
        }

        internal static bool readerHasColumn(IDataReader rdr, string columnName, bool ignoreCase = true)
        {
            for (int i = 0; i < rdr.FieldCount; i++)
            {
                if (String.Equals(rdr.GetName(i), columnName, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
                {
                    return true;
                }
            }

            return false;
        }
    }
}