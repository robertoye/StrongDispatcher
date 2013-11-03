/*StrongDispatcher中涉及到FLowPortal工作流发起的基本方法
 * 
 * created by Roberto Ye  @2013-09-30
 * 
 * ================================================
 * 变更记录：
 * 
 * 
 * 
 * 增加方法：OUFullNameByUserAccount zhye @2013-10-30
 * 增加静态方法：SetBPMTaskHeader zhye @2013-10-30
 * 增加静态方法：ConvertLanchDataToStream zhye @2013-10-30 
 * 
 * ================================================
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using BPM;
using BPM.Client;

namespace BPMUtility
{
    //
    public delegate MemoryStream GeneratePostXML(string processName, string BPMUserFullName, DataSet ds,int PID);

     /// <summary>
    /// 
    /// </summary>
    /// <param name="processName"></param>
    /// <param name="BPMUserFullName"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    public class BPMHelper
    {
        public GeneratePostXML _tag;        
        private string _eServer;
        private string _userAccount;
        private string _pwd;
        private string _userFullName;


        public BPMHelper(string BPMServer, string UserAccount, string PWD)
        {            
            _eServer = BPMServer;
            _userAccount = UserAccount;
            _pwd = PWD;

            using (BPMConnection bpmConn = new BPMConnection())
            {
                bpmConn.Open(_eServer, _userAccount, _pwd);
                MemberCollection positions = OrgSvr.GetUserPositions(bpmConn, bpmConn.UID);

                if (positions.Count == 0)
                {
                    throw new BPMException(string.Format("用户不属于任何组织，账号：{0}", bpmConn.UID));
                }
                _userFullName = positions[0].FullName;
            }
        }


        /// <summary>
        /// 根据用户账号取得用户在组织架构中的全名
        /// </summary>
        /// <param name="strUserAccount"></param>
        /// <returns></returns>
        public string OUFullNameByUserAccount(string strUserAccount)
        {
            string strResult = "";
            if (strUserAccount == _userAccount)
            {
                strResult = _userFullName;
            }
            else
            {
                using (BPMConnection bpmConn = new BPMConnection())
                {
                    bpmConn.Open(_eServer, _userAccount, _pwd);
                    MemberCollection positions = OrgSvr.GetUserPositions(bpmConn, strUserAccount);

                    if (positions.Count == 0)
                    {
                        throw new BPMException(string.Format("用户不属于任何组织，账号：{0}", strUserAccount));
                    }
                    strResult = positions[0].FullName;
                }
            }
            return strResult;
        }

        /// <summary>
        /// 发起流程申请
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="xmlStream"></param>
        public string StartProcess(string processName, DataSet ds,int PID)
        {
            using (BPMConnection bpmConn = new BPMConnection())
            {
                bpmConn.Open(_eServer, _userAccount, _pwd);
                if (_tag == null)
                {
                    throw new Exception("发起流程前必须指定格式化数据流的方法GeneratePostXML！");
                }
                MemoryStream xmlStream = _tag(processName, _userFullName, ds, PID);
                PostResult result = BPMProcess.Post(bpmConn, xmlStream);

                return string.Format("[State:{0},TaskID:{1},SN:'{2}']", result.State, result.TaskID, result.SN);
            }
        }
        /// <summary>
        /// 设置发起流程数据表头
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="firstStepActionName"></param>
        /// <returns></returns>
        public static DataTable SetBPMTaskHeader(string processName,string firstStepActionName,string memberPosition)
        {
            //设置系统数据Header
            DataTable tableHeader = new DataTable("Header");
            tableHeader.Columns.Add(new DataColumn("Method", typeof(string)));
            tableHeader.Columns.Add(new DataColumn("ProcessName", typeof(string)));
            tableHeader.Columns.Add(new DataColumn("Action", typeof(string)));
            tableHeader.Columns.Add(new DataColumn("OwnerMemberFullName", typeof(string)));
            tableHeader.Columns.Add(new DataColumn("UploadFileGuid", typeof(string)));

            DataRow rowHeader = tableHeader.NewRow();
            //设置Header数据
            rowHeader["Method"] = "Post";
            rowHeader["ProcessName"] = processName;
            rowHeader["Action"] = firstStepActionName;
            rowHeader["OwnerMemberFullName"] = memberPosition;//
            tableHeader.Rows.Add(rowHeader);
            return tableHeader;
        }
        /// <summary>
        /// 将发起数据序列化后转化成MemoryStream
        /// </summary>
        /// <param name="tableHeader"></param>
        /// <param name="formDataSet"></param>
        /// <returns></returns>
        public static MemoryStream ConvertLanchDataToStream(DataTable tableHeader, DataSet formDataSet)
        {
            //生成XML
            StringBuilder sb = new StringBuilder();
            StringWriter w = new StringWriter(sb);

            w.WriteLine("<?xml version=\"1.0\"?>");
            w.WriteLine("<XForm>");
            tableHeader.WriteXml(w, XmlWriteMode.IgnoreSchema, false);
            formDataSet.WriteXml(w);
            w.WriteLine("</XForm>");
            w.Close();
            String xmlData = sb.ToString();
            xmlData = xmlData.Replace("<DocumentElement>", "");
            xmlData = xmlData.Replace("</DocumentElement>", "");
            MemoryStream xmlStream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(xmlData));
            return xmlStream;
        } 
    }
}