using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.bitscopic.hilleman.core.domain
{
    public class MyDataTable
    {
        public Dictionary<String, Int32> columnIndicesByName;
        public Dictionary<Int32, String> columnNamesByIndex;

        internal List<String[]> _dataRows;
        internal bool _caseInsensitiveColumnNames = true;

        public String[] getRow(Int32 rowIndex)
        {
            return _dataRows[rowIndex];
        }

        public String[] this[Int32 rowIndex]
        {
            get { return _dataRows[rowIndex]; }
        }

        public MyDataTable() { }

        public MyDataTable(System.Data.DataTable dotnetDataTable)
        {
            if (dotnetDataTable == null || dotnetDataTable.Rows.Count == 0 || dotnetDataTable.Columns.Count == 0)
            {
                return;
            }

            instantiateDictionaries(_caseInsensitiveColumnNames);
            bool allColsAreStrings = true;
            for (int i = 0; i < dotnetDataTable.Columns.Count; i++)
            {
                columnIndicesByName.Add(dotnetDataTable.Columns[i].ColumnName, i);
                columnNamesByIndex.Add(i, dotnetDataTable.Columns[i].ColumnName);

                if (allColsAreStrings && !(dotnetDataTable.Columns[i].DataType == typeof(String)))
                {
                    allColsAreStrings = false;
                }
            }

            _dataRows = new List<string[]>();
            foreach (System.Data.DataRow row in dotnetDataTable.Rows)
            {
                object[] rowVals = row.ItemArray;
                String[] rowValsAsString = new string[rowVals.Length];
                for (int i = 0; i < rowVals.Length; i++)
                {
                    if (allColsAreStrings)
                    {
                        rowValsAsString[i] = rowVals[i] as String;
                    }
                    else
                    {
                        rowValsAsString[i] = Convert.ToString(rowVals[i]);
                    }
                }
            }
        }

        internal void instantiateDictionaries(bool caseInsensitive)
        {
            if (caseInsensitive)
            {
                this.columnIndicesByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                this.columnIndicesByName = new Dictionary<string, int>();
            }

            this.columnNamesByIndex = new Dictionary<int, string>();
        }
    }
}
