using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;

namespace BPMUtility
{
    public class SQLHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConn"></param>
        /// <param name="strSql"></param>
        /// <param name="isProcedure"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string strConn,string strSql,string strTableName)
        {
            DataTable table = new DataTable(); 
            using (SqlConnection cn = new SqlConnection(strConn))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(strSql, cn);                
                cmd.CommandType = CommandType.Text;

                IDataReader reader = cmd.ExecuteReader();
                table.Load(reader);
                reader.Dispose();
            }
            table.TableName = strTableName;
            return table;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConn"></param>
        /// <param name="strSql"></param>
        /// <param name="isProcedure"></param>
        public static void ExecuteSql(string strConn, string strSql, bool isProcedure)
        {
            using (SqlConnection cn = new SqlConnection(strConn))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(strSql, cn);
                cmd.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
