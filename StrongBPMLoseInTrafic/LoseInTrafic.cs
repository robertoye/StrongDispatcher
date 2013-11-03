/*
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;

using BPMUtility;

namespace StrongBPMLoseInTrafic
{
    public class LoseInTrafic
    {
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 发起流程方法
        /// </summary>
        public static void Launch()
        {
            string ConfName = string.Format(@"{0}\{1}", System.AppDomain.CurrentDomain.BaseDirectory, "LoseInTrafic.xml");
            //读取配置文件
            LoseInTraficConf conf = LoseInTraficConf.LoadConf(ConfName);

            //获取数据源，如果不需要发起，直接退出，避免后面的尝试连接程序性能消耗
            DataSet ds = GetDataSource(conf.ERPConnString);            
            if (ds.Tables[0].Rows.Count == 0)
            {
                return;
            }

            try
            {                
                BPMHelper bpm = new BPMHelper(conf.BPMServer, conf.UserAccount,conf.PWD);
                bpm._tag = new GeneratePostXML(LoseInTraficXML);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row["ReqFlowId"].ToString() == "2")
                    {
                        string result = bpm.StartProcess("中心仓途损申请", ds,int.Parse(row["ID"].ToString()));
                        //此处需将信息记录到日志中
                        row["ReqFlowId"] = 6;
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.Message);
            }
            finally
            {
                //对已启动的流程设置已触发标记
                FinalDo(conf.ERPConnString,ds);
            }
        }
        /// <summary>
        /// 获取发起流程源数据
        /// </summary>
        /// <returns></returns>
        private static DataSet GetDataSource(string strConn)
        {
            string strSql = "execute dbo.WebGetMoutreqWaitToOA";
            DataTable table = SQLHelper.GetDataTable(strConn,strSql,"Main");
            
            foreach (DataRow row in table.Rows)
            {
                string sql = string.Format("UPDATE MoutReqM SET ReqFlowId=5 WHERE ReqFlowId=2 and ID={0}", row["ID"].ToString());
                SQLHelper.ExecuteSql(strConn, sql, false);
            }
            DataSet ds = new DataSet();
            ds.Tables.Add(table);
            return ds;
        }

        /// <summary>
        /// 将提交数据转化为MemoryStream
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="memberPosition"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static MemoryStream LoseInTraficXML(string processName, string memberPosition, DataSet ds,int pId)
        {
            DataRow row = ds.Tables[0].Rows.Find(pId);
            DataTable tableHeader = BPMHelper.SetBPMTaskHeader(processName, "提交", memberPosition);

            //设置表单BillHeader数据
            DataSet formDataSet = new DataSet("FormData");
            DataTable table0 = new DataTable("BillHeader");
            table0.Columns.Add(new DataColumn("ProceeName", typeof(string)));

            DataRow row0 = table0.NewRow();
            row0["ProceeName"] = processName;
            table0.Rows.Add(row0);

            formDataSet.Tables.Add(table0);

            //设置表单数据            
            DataTable table1 = new DataTable("BillExtAppMain");
            table1.Columns.Add(new DataColumn("int1", typeof(int)));
            table1.Columns.Add(new DataColumn("int2", typeof(int)));
            table1.Columns.Add(new DataColumn("int3", typeof(int)));
            table1.Columns.Add(new DataColumn("str1", typeof(string)));
            table1.Columns.Add(new DataColumn("str2", typeof(string)));
            table1.Columns.Add(new DataColumn("str6", typeof(string)));

            DataRow row1 = table1.NewRow();
            row1["int1"] = row["ID"];
            row1["int2"] = row["TranTypeId"];
            row1["int3"] = row["SalesOrderMId"];
            row1["str1"] = row["TranTypeName"].ToString();
            row1["str2"] = row["DeptName"].ToString();
            row1["str6"] = row["Reason"];
            table1.Rows.Add(row1);
            formDataSet.Tables.Add(table1);

            return BPMHelper.ConvertLanchDataToStream(tableHeader, formDataSet);
        }
        
        /// <summary>
        /// 完成发起后修改源状态
        /// </summary>
        /// <param name="table"></param>
        private static void FinalDo(string strConn,DataSet ds)
        {            
            //设置标志
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string sql = string.Format("UPDATE MoutReqM SET ReqFlowId=2 WHERE ID={0} and ReqFlowId=5",row["ID"].ToString());
                if (Convert.ToInt32(row["ReqFlowId"]) == 6)
                {
                    sql = string.Format("UPDATE MoutReqM SET ReqFlowId=6 WHERE ID={0} and ReqFlowId=5", row["ID"].ToString());                        
                }       
                SQLHelper.ExecuteSql(strConn, sql, false);             
            }
        }
    }
}
