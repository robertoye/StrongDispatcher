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

namespace StrongBPMTC
{
    public class TC
    {
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        
        protected static string _proccessName = "特采";
        protected static string _billTypeName = "特采申请单";
        /// <summary>
        /// 发起流程方法
        /// </summary>
        public static void Launch()
        {
            string ConfName = string.Format(@"{0}\{1}", System.AppDomain.CurrentDomain.BaseDirectory, "TC.xml");
            //读取配置文件
            TCConf conf = TCConf.LoadConf(ConfName);
            //获取数据源，如果不需要发起，直接退出，避免后面的尝试连接程序性能消耗
            DataSet ds = GetDataSource(conf.ERPConnString);            
            
            if (ds.Tables[0].Rows.Count == 0)
            {
                return;
            }

            try
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row["ReqFlowId"].ToString() == "2")
                    {
                        //判定发起人位置；
                        string strOrganizing = row["Organizing"].ToString().Substring(0, 2) ;
                        string UID = "hboa";
                        string pName = "特采";

                        switch(strOrganizing)
                        {
                            case "南京":
                                UID = "njoa";
                                break;
                            case "阳江":
                                UID = "yjoa";
                                break;                        
                            default:
                                UID = "hboa";
                                break;
                        }
                        if (row["FlowTypeID"].ToString() == "1")
                        {
                            pName = "紧急放行";
                        }

                        BPMHelper bpm = new BPMHelper(conf.BPMServer, UID,"");
                        bpm._tag = new GeneratePostXML(TCXML);
                        int pId = int.Parse(row["ID"].ToString());
                        string result = bpm.StartProcess(pName, ds,pId);
                        //此处需将信息记录到日志中
                        _log.Info(String.Format("{0}{1}发起流程成功；流程发起返回信息{2}", pName,pId, result));
                        row["OAflowID"] = 3;;
                    }
                }
            }
            catch (Exception err)
            {
                _log.Info(err);
                throw new Exception(err.Message);
            }
            finally
            {
                //对已启动的流程设置已触发标记
                FinalDo(conf.ERPConnString, ds);
            }
        }

        /// <summary>
        /// 获取发起流程源数据
        /// </summary>
        /// <returns></returns>
        private static DataSet GetDataSource(string strConn)
        {
            DataSet ds = new DataSet();
            try
            {
                string strSql = "SELECT * FROM OA_QcResult_Info WHERE OAFlowID=2 and InOaProc = 0";
                DataTable table = SQLHelper.GetDataTable(strConn, strSql,"Main");
                foreach (DataRow row in table.Rows)
                {
                    string sql = string.Format("UPDATE OA_QcResult_Info SET InOaProc=1 WHERE ID={0}", row["ID"].ToString());
                    SQLHelper.ExecuteSql(strConn, sql, false);
                }
                //设置主键
                table.PrimaryKey = new DataColumn[] { table.Columns["ID"] };                
                ds.Tables.Add(table);
            }
            catch (Exception err)
            {
                _log.Error(err);
                throw (err);
            }
            return ds;
        }
        /// <summary>
        /// 将提交数据转化为MemoryStream
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="memberPosition"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static MemoryStream TCXML(string processName, string memberPosition, DataSet ds,int pId)
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
            table1.Columns.Add(new DataColumn("str1", typeof(string)));
            table1.Columns.Add(new DataColumn("str2", typeof(string)));
            table1.Columns.Add(new DataColumn("str3", typeof(string)));
            table1.Columns.Add(new DataColumn("str4", typeof(string)));
            table1.Columns.Add(new DataColumn("str6", typeof(string)));
            table1.Columns.Add(new DataColumn("str7", typeof(string)));
            table1.Columns.Add(new DataColumn("str10", typeof(string)));
            table1.Columns.Add(new DataColumn("int1", typeof(int)));
            table1.Columns.Add(new DataColumn("int21", typeof(int))); //B1类是否已经类似申请是否经过审批同意
            table1.Columns.Add(new DataColumn("str20", typeof(string)));//来自那个基地
            table1.Columns.Add(new DataColumn("str22", typeof(string)));//技研部工程师账号

            DataRow row1 = table1.NewRow();
            row1["str1"] = row["ID"];
            row1["str2"] = row["IsFirst"];
            row1["str3"] = row["MaterialType"];
            row1["str4"] = row["Organizing"];
            row1["str6"] = row["Remark"];
            row1["str7"] = string.Format("{0};检验单号:{1};供应商:{2};物料名称:{3}:{4}", row["Proposer"].ToString(),
                row["QcResultId"].ToString(), row["Supplier"].ToString(), row["MCode"].ToString(), row["MName"].ToString());
            row1["str10"] = row["DomainAccount"];
            row1["int1"] = row["FlowTypeID"];
            row1["int21"] = row["B1QcResultID"];
            row1["str20"] = row["Organizing"];
            row1["str22"] = row["JYDomainAccount"];
            table1.Rows.Add(row1);
            formDataSet.Tables.Add(table1);

            return BPMHelper.ConvertLanchDataToStream(tableHeader, formDataSet);
        }
        /// <summary>
        /// 完成发起后修改源状态
        /// </summary>
        /// <param name="table"></param>
        private static void FinalDo(string strConn, DataSet ds)
        {            
            //设置标志
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string sql = string.Format("UPDATE OA_QcResult_Info SET InOaProc=0 WHERE ID={0} and OAFlowID=2", row["ID"].ToString());
                if (Convert.ToInt32(row["OAFlowID"]) == 3)
                {
                    sql = string.Format("UPDATE OA_QcResult_Info SET OAFlowID=3 WHERE ID={0} and OAFlowID=2", row["ID"].ToString());
                }
                SQLHelper.ExecuteSql(strConn,sql,false);
            }            
        }
    }
}
