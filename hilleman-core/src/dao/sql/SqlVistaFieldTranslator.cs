using System;
using System.Collections.Generic;
using System.IO;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class SqlVistaFieldTranslator
    {
        DirectoryInfo _confDirectory;
        Dictionary<String, SqlTableConfigMap> _configs;

        public SqlVistaFieldTranslator(String configDirectory)
        {
            _confDirectory = new DirectoryInfo(configDirectory);
            refreshSqlTableDefs();
        }

        public Dictionary<String, SqlTableConfigMap> refreshSqlTableDefs()
        {
            _configs = new SqlTableConfigParser(_confDirectory.FullName).parse(true);
            return _configs;
        }

        public SqlTableConfigMap getConfigMap(String vistaFile)
        {
            if (_configs == null || _configs.Count == 0)
            {
                refreshSqlTableDefs();
            }
            return _configs[vistaFile];
        }

        #region CRRUD - READ
        public String getSqlQueryFromRead(ReadRequest vistaRequest)
        {
            if (!_configs.ContainsKey(vistaRequest.getFile()))
            {
                throw new ArgumentException("No SQL config for that file!");
            }

            // fix IEN for SQL which uses underscores in place of commas
            String correctedIen = vistaRequest.getIens().Replace(",", "_");
            if (correctedIen.StartsWith("_"))
            {
                correctedIen = correctedIen.Substring(1);
            }
            if (correctedIen.EndsWith("_"))
            {
                correctedIen = correctedIen.Substring(0, correctedIen.Length - 1);
            }

            SqlTableConfigMap map = _configs[vistaRequest.getFile()];
            IList<String> vistaRequestFields = StringUtils.split(vistaRequest.getFields(), StringUtils.SEMICOLON);
            String sqlTableName = map.sqlTableName;
            Dictionary<String, String> fieldsDict = buildVistaFieldKeySqlColumnValueDict(
                map.vistaFieldsParsed, 
                map.sqlColumnsParsed, 
                map.sqlSpecialColumnsParsed == null ? null : new List<String>(map.sqlSpecialColumnsParsed.Keys));
            IList<String> listOfSqlColumns = new List<String>();

            foreach (String vistaField in vistaRequestFields)
            {
                if (!fieldsDict.ContainsKey(vistaField))
                {
                    throw new ArgumentException("Field number " + vistaField + " is not present in SQL!");
                }
                listOfSqlColumns.Add(fieldsDict[vistaField]);
            }

            String sqlFields = StringUtils.join(listOfSqlColumns, ", ");
            String sourceSystemId = vistaRequest.getSource().id;

            if (correctedIen.Contains("_")) // subfile!
            {
                return String.Format("SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, {0} FROM {1} WHERE IEN='{2}' AND SITECODE='{3}'", sqlFields, sqlTableName, correctedIen, sourceSystemId);
            }
            else
            {
                return String.Format("SELECT IEN, {0} FROM {1} WHERE IEN='{2}' AND SITECODE='{3}'", sqlFields, sqlTableName, correctedIen, sourceSystemId);
            }
        }

        #endregion

        #region CRRUD - READ RANGE
        public String getSqlQueryFromReadRange(ReadRangeRequest vistaRequest)
        {
            if (!_configs.ContainsKey(vistaRequest.getFile()))
            {
                throw new ArgumentException("No SQL config for that file!");
            }

            // fix IEN for SQL which uses underscores in place of commas
            String correctedIen = vistaRequest.getIens().Replace(",", "_");
            if (correctedIen.StartsWith("_"))
            {
                correctedIen = correctedIen.Substring(1);
            }
            if (correctedIen.EndsWith("_"))
            {
                correctedIen = correctedIen.Substring(0, correctedIen.Length - 1);
            }

            SqlTableConfigMap map = _configs[vistaRequest.getFile()];
            IList<String> vistaRequestFields = StringUtils.split(vistaRequest.getFields(), StringUtils.SEMICOLON);
            String sqlTableName = map.sqlTableName;
            Dictionary<String, String> fieldsDict = buildVistaFieldKeySqlColumnValueDict(
                map.vistaFieldsParsed,
                map.sqlColumnsParsed,
                map.sqlSpecialColumnsParsed == null ? null : new List<String>(map.sqlSpecialColumnsParsed.Keys));
            IList<String> listOfSqlColumns = new List<String>();

            foreach (String vistaField in vistaRequestFields)
            {
                if (!fieldsDict.ContainsKey(vistaField))
                {
                    throw new ArgumentException("Field number " + vistaField + " is not present in SQL!");
                }
                listOfSqlColumns.Add(fieldsDict[vistaField]);
            }

            String sqlFields = StringUtils.join(listOfSqlColumns, ", ");
            String sourceSystemId = vistaRequest.getSource().id;
            String startVal = String.IsNullOrEmpty(vistaRequest.getFrom()) ? "0" : vistaRequest.getFrom();
            String max = vistaRequest.getMax();
            String xref = vistaRequest.getCrossRef();
            
            String subQuerySql = ""; // must create subquery where results are sorted appropriately
            if (String.IsNullOrEmpty(correctedIen)) // TOP LEVEL FILE!!
            {
                if (String.IsNullOrEmpty(xref) || String.Equals(xref, "#"))
                {
                    subQuerySql = String.Format("(SELECT * FROM {0} ORDER BY cast(IEN as number))", sqlTableName);
                }
                else if (String.Equals(xref, "B", StringComparison.CurrentCultureIgnoreCase))
                {
                    String x01FieldColumnName = fieldsDict[".01"];
                    subQuerySql = String.Format("(SELECT * FROM {0} ORDER BY {1})", sqlTableName, x01FieldColumnName);
                }
                else
                {
                    throw new ArgumentException("SQL VistA cached queries currently only support IEN and 'B' cross references");
                }
            }
            else
            {
                if (String.IsNullOrEmpty(xref) || String.Equals(xref, "#"))
                {
                    subQuerySql = String.Format("(SELECT * FROM {0} WHERE IEN LIKE('%\\_{1}') ESCAPE '\\' ORDER BY cast(substr(IEN, 0, instr(IEN, '_')) AS number))", sqlTableName, correctedIen);
                }
                else if (String.Equals(xref, "B", StringComparison.CurrentCultureIgnoreCase))
                {
                    String x01FieldColumnName = fieldsDict[".01"];
                    subQuerySql = String.Format("(SELECT * FROM {0} WHERE IEN LIKE('%\\_{1}') ESCAPE '\\' ORDER BY {2})", sqlTableName, correctedIen, x01FieldColumnName);
                }
                else
                {
                    throw new ArgumentException("SQL VistA cached queries currently only support IEN and 'B' cross references");
                }
            }

            String sql = "";
            if (!String.IsNullOrEmpty(correctedIen))
            {
                if (String.IsNullOrEmpty(xref) || String.Equals(xref, "#"))
                {
                    sql = String.Format("SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, {0} FROM {1} WHERE IEN LIKE('%\\_{2}') ESCAPE '\\' AND SITECODE='{3}' AND cast(substr(IEN, 0, instr(IEN, '_')) AS number)>{4}", sqlFields, subQuerySql, correctedIen, sourceSystemId, startVal);
                }
                else if (String.Equals("B", xref))
                {
                    String x01FieldColumnName = fieldsDict[".01"];
                    sql = String.Format("SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, {0} FROM {1} WHERE IEN LIKE('%\\_{2}') ESCAPE '\\' AND SITECODE='{3}' AND {4}>'{5}'", sqlFields, subQuerySql, correctedIen, sourceSystemId, x01FieldColumnName, startVal);
                }
                else
                {
                    throw new ArgumentException("SQL VistA cached queries currently only support IEN and 'B' cross references");
                }
            }
            else
            {
                if (String.IsNullOrEmpty(xref) || String.Equals(xref, "#"))
                {
                    sql = String.Format("SELECT IEN, {0} FROM {1} WHERE SITECODE='{2}' AND cast(IEN as number)>{3}", sqlFields, subQuerySql, sourceSystemId, startVal);
                }
                else if (String.Equals("B", xref))
                {
                    String x01FieldColumnName = fieldsDict[".01"];
                    sql = String.Format("SELECT IEN, {0} FROM {1} WHERE SITECODE='{2}' AND {3}>'{4}'", sqlFields, subQuerySql, sourceSystemId, x01FieldColumnName, startVal);
                }
                else
                {
                    throw new ArgumentException("SQL VistA cached queries currently only support IEN and 'B' cross references");
                }
            }

            // add max, if present
            if (!String.IsNullOrEmpty(max)) 
            {
                sql = sql + String.Concat(" LIMIT ", max);
            }

            return sql;
        }
        #endregion

        internal Dictionary<String, String> buildVistaFieldKeySqlColumnValueDict(IList<String> vistaFieldsFromConfig, IList<String> sqlColumnsFromConfig, IList<String> specialCols)
        {
            IList<String> joinedSqlCols = ListUtils.join<String>(sqlColumnsFromConfig, specialCols); 

            if (vistaFieldsFromConfig.Count != joinedSqlCols.Count)
            {
                throw new ArgumentException("The VistA fields and SQL columns don't match! Please check the config files for inconsistencies...");
            }

            Dictionary<String, String> result = new Dictionary<string, string>();

            for (int i = 0; i < vistaFieldsFromConfig.Count; i++)
            {
                result.Add(vistaFieldsFromConfig[i], joinedSqlCols[i]);
            }

            return result;
        }
    }
}