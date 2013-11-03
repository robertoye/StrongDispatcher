/*
 * 中心仓调拨计划监控发起OA中FlowPortal工作流方法代码
 * created by Roberto Ye  @2013-10-31
 * 
 * ================================================
 * 变更记录：
 * 
 * 
 * ================================================
 * 
 * 详细字段对应关系说明  
 * 主表：WarehouseTransferPlanM 
 * ID               int1
 * OfficeID         int2
 * OfficeName       str1
 * WarehouseID      int3
 * WarehouseName    str2
 * PlanYear         int4
 * PlanMonth        int5
 * ReqflowID        int6    
 * FlowName         str3
 * CompleteTime     ***
 * RequestMan       str4
 * Notes            str5
 * OAFlowID         ***
 * 
 * 明细表：WarehouseTransferPlanD
 * ID                       int1
 * MCode        产品代码    str1
 * ShortName    名称          str2
 * Spec         规格          str3
 * WholeUnit    单位          str4
 * Qty          数量          decimal1
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

namespace StrongBPMTransferPlan
{
	public class TransferPlan
	{
		protected static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		protected static string _proccessName = "中心仓调拨计划";
		protected static string _billTypeName = "调拨计划申请单";

		protected static BPMHelper _bpm;

		/// <summary>
		/// 发起流程方法
		/// </summary>
		public static void Launch()
		{
			string ConfName = string.Format(@"{0}\{1}", System.AppDomain.CurrentDomain.BaseDirectory, "TransferPlan.xml");
			//读取配置文件
			TransferPlanConf conf = TransferPlanConf.LoadConf(ConfName);
			//获取数据源，如果不需要发起，直接退出，避免后面的尝试连接程序性能消耗
			DataSet ds = GetDataSource(conf.ERPConnString);
			if (ds.Tables["WarehouseTransferPlanM"].Rows.Count == 0) return;

			try
			{
				_bpm = new BPMHelper(conf.BPMServer, conf.UserAccount, conf.PWD);
				//声明原数据输出xml stream的方法
				_bpm._tag = new GeneratePostXML(TransferPlanXML);

				foreach (DataRow row in ds.Tables["WarehouseTransferPlanM"].Rows)
				{
					int pId = int.Parse(row["Id"].ToString());
					if (row["OAFlowId"] is DBNull || row["OAFlowId"].ToString() == "1")
					{
						string result = _bpm.StartProcess(_proccessName, ds,pId);						
						string msg = BPMMsgHepler.StartProccessSucceed(_proccessName, _billTypeName, pId, result);
						_log.Info(msg);
						row["OAFlowId"] = 3;
					}
				}
			}
			catch (Exception err)
			{
				_log.Error(err);
				throw(err);
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
				DataTable dtM = SQLHelper.GetDataTable(strConn, "execute dbo.WebGetWarehouseTransferPlanWaitToOA", "WarehouseTransferPlanM");
				//一定要增加主键，不然在输出xml中不能利用Find方法 <see cref="TransferPlanXML" />
				dtM.PrimaryKey = new DataColumn[] { dtM.Columns["ID"] };
				StringBuilder st = new StringBuilder("(-1,");
				foreach (DataRow row in dtM.Rows)
				{
					int pId = int.Parse(row["ID"].ToString());
					st.Append(string.Format("{0},",pId));
					string sql = string.Format("UPDATE WarehouseTransferPlanM SET OAFlowId=2 WHERE isnull(OAFlowId,1) = 1 and ID={0}", pId);
					SQLHelper.ExecuteSql(strConn, sql, false);
					string msg = BPMMsgHepler.GetSourceSucceed(_proccessName,_billTypeName,pId,"SET OAFlowId=2");
					_log.Info(msg);
				}
				st.Append("-1)");
				
				//获取子表单            
				string strDetail = string.Format(@"select a.ID,a.WHTranMID,a.MCode,b.ShortName,b.Spec,b.WholeUnit,a.Qty 
										from WarehouseTransferPlanD a join Material b on(a.MCode=b.MCode) 
											where a.WHTranMID in {0}", st.ToString());
				DataTable dtD = SQLHelper.GetDataTable(strConn, strDetail, "WarehouseTransferPlanD");
				ds.Tables.Add(dtM);
				ds.Tables.Add(dtD);
				ds.Relations.Add(new DataRelation("FK", ds.Tables["WarehouseTransferPlanM"].Columns["ID"], ds.Tables["WarehouseTransferPlanD"].Columns["WHTranMID"]));
			}
			catch(Exception err)
			{
				_log.Error(string.Format("流程发起错误：中心仓调拨计划获取源数据失败，错误信息：{0}；",err.Message));
				throw(err);
			}
			return ds;
		}

		/// <summary>
		/// 将提交数据转化为MemoryStream
		/// </summary>
		/// <param name="processName"></param>
		/// <param name="memberPosition"></param>
		/// <param name="ds">源数据集</param>
		/// <param name="pId">主键值</param>
		/// <returns></returns>
		private static MemoryStream TransferPlanXML(string processName, string memberPosition,DataSet ds,int pId)
		{
			DataRow row = ds.Tables["WarehouseTransferPlanM"].Rows.Find(pId);
			//设置标准的xml stream头，如果memberPosition不使用BPMHelper的缺省连接账号，请在此处定义，并通过			
			//string mem = _bpm.OUFullNameByUserAccount(account);

			DataTable tableHeader = BPMHelper.SetBPMTaskHeader(processName, "提交", memberPosition);
			
			//设置表单BillHeader数据
			DataSet formDataSet = new DataSet("FormData");

			DataTable table0 = new DataTable("BillHeader");
			table0.Columns.Add(new DataColumn("ProceeName", typeof(string)));

			DataRow row0 = table0.NewRow();
			row0["ProceeName"] = processName;
			table0.Rows.Add(row0);
			formDataSet.Tables.Add(table0);

			//设置表单主数据
			DataTable table1 = new DataTable("BillExtAppMain");
			table1.Columns.Add(new DataColumn("int1", typeof(int)));
			table1.Columns.Add(new DataColumn("int2", typeof(int)));
			table1.Columns.Add(new DataColumn("int3", typeof(int)));
			table1.Columns.Add(new DataColumn("int4", typeof(int)));
			table1.Columns.Add(new DataColumn("int5", typeof(int)));
			table1.Columns.Add(new DataColumn("int6", typeof(int)));
			table1.Columns.Add(new DataColumn("str1", typeof(string)));
			table1.Columns.Add(new DataColumn("str2", typeof(string)));
			table1.Columns.Add(new DataColumn("str3", typeof(string)));
			table1.Columns.Add(new DataColumn("str4", typeof(string)));
			table1.Columns.Add(new DataColumn("str5", typeof(string)));
			table1.Columns.Add(new DataColumn("str6", typeof(string)));

			DataRow row1 = table1.NewRow();
			row1["int1"] = int.Parse(row["ID"].ToString());
			row1["int2"] = int.Parse(row["OfficeID"].ToString());
			row1["int3"] = int.Parse(row["WarehouseID"].ToString());
			row1["int4"] = int.Parse(row["PlanYear"].ToString());
			row1["int5"] = int.Parse(row["PlanMonth"].ToString());
			row1["int6"] = int.Parse(row["ReqflowID"].ToString());

			row1["str1"] = row["OfficeName"].ToString();
			row1["str2"] = row["WarehouseName"].ToString();
			row1["str3"] = row["FlowName"].ToString();
			row1["str4"] = row["RequestMan"].ToString();
			row1["str5"] = row["Notes"].ToString();
			row1["str6"] = string.Format("{0}{1} {2}年{3}月调拨计划 ",row["OfficeName"].ToString(),row["WarehouseName"].ToString(),
								row["PlanYear"].ToString(),row["PlanMonth"].ToString());

			table1.Rows.Add(row1);
			formDataSet.Tables.Add(table1);

			//设置表单明细数据
			DataTable table2 = new DataTable("BillExtAppDetail");
			table2.Columns.Add(new DataColumn("int1", typeof(int)));                        
			table2.Columns.Add(new DataColumn("str1", typeof(string)));
			table2.Columns.Add(new DataColumn("str2", typeof(string)));
			table2.Columns.Add(new DataColumn("str3", typeof(string)));            
			table2.Columns.Add(new DataColumn("str4", typeof(string)));
			table2.Columns.Add(new DataColumn("decimal1", typeof(decimal)));

			foreach(DataRow dr in row.GetChildRows("FK"))
			{
				DataRow rowd = table2.NewRow();
				rowd["int1"] = int.Parse(dr["ID"].ToString());
				rowd["str1"] = dr["MCode"].ToString();
				rowd["str2"] = dr["ShortName"].ToString();
				rowd["str3"] = dr["Spec"].ToString();
				rowd["str4"] = dr["WholeUnit"].ToString();
				rowd["decimal1"] = decimal.Parse(dr["Qty"].ToString());
				table2.Rows.Add(rowd);
			}
			formDataSet.Tables.Add(table2);

			return BPMHelper.ConvertLanchDataToStream(tableHeader, formDataSet);
		}      

		/// <summary>
		/// 完成发起后修改源状态
		/// </summary>
		/// <param name="table"></param>
		private static void FinalDo(string strConn, DataSet ds)
		{
			//设置标志
			foreach (DataRow row in ds.Tables["WarehouseTransferPlanM"].Rows)
			{
				int pId = int.Parse(row["Id"].ToString());
				string sql = string.Format("UPDATE WarehouseTransferPlanM SET OAFlowId=3 WHERE ID={0} and OAFlowId=2",pId);
				string msg = BPMMsgHepler.SucceedWirteBack(_proccessName, _billTypeName, pId, "SET OAFlowId=3"); 
				if (Convert.ToInt32(row["OAFlowId"]) == 1)
				{   
					sql = string.Format("UPDATE WarehouseTransferPlanM SET OAFlowId=1 WHERE ID={0} and OAFlowId=2", pId);
					msg = BPMMsgHepler.SucceedWirteBack(_proccessName, _billTypeName, pId, "SET OAFlowId=1"); 
				}                    
				SQLHelper.ExecuteSql(strConn, sql, false);
				_log.Info(msg);
			}
		}
	}
}
