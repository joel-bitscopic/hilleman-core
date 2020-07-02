using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.bitscopic.hilleman.core.dao.sql
{
    public class SqlTableConfigMap
    {
        public String vistaFields;
        public IList<String> vistaFieldsParsed;
        public String sqlColumns;
        public IList<String> sqlColumnsParsed;
        public Dictionary<String, String> sqlSpecialColumnsParsed;
        public bool subFile;
        public String sqlTableName;
        public String vistaFileName;
        public String vistaFileNumber;

        public SqlTableConfigMap() { }
    }
}