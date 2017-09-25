using System;
using System.Activities.Statements;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PeriodAid.UnityTool
{
    public class ExcelHelper
    {
        public IList<T> ReadExcelToEntityList<T>(string filePath) where T : class, new()
        {
            System.Data.DataTable tbl = ReadExcelToDataTable(filePath);//读取Excel数据到DataTable

            IList<T> list = DataTableToList<T>(tbl);

            return list;
        }
        public static DataTable
         ReadExcelToDataTable(string filePath)
        {
            if (filePath == string.Empty)
                throw new ArgumentNullException
                ("路径参数不能为空");
            string ConnectionString ="Provider=Microsoft.ACE.OLEDB.12.0;Persist Security Info = False; Data Source =" + filePath + ";Extended Properties = 'Excel 8.0;HDR=Yes;IMEX=1'";
            OleDbDataAdapter adapter= new OleDbDataAdapter("select * From[Sheet1$]",ConnectionString); 
            DataTable table = new DataTable("TempTable");
            adapter.Fill(table);
            return table;
        }
        public static List<T> DataTableToList<T>(DataTable dt) where T : class, new() {
            if (dt == null) return null;
            List<T> list = new List<T>();
            foreach (DataRow dr in dt.Rows) {
                T t = new T();
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pro in propertys) {
                    if (dt.Columns.Contains(pro.Name)) {
                        object value = dr[pro.Name];
                        value = Convert.ChangeType(value, pro.PropertyType);
                        if (value != DBNull.Value)
                        {
                            pro.SetValue(t, value, null);
                        }
                    }
                }
                list.Add(t);
            }
            return list;
        }
    }
}