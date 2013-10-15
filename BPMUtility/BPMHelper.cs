/*
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
using System.Data;
using System.IO;

using BPM;
using BPM.Client;

namespace BPMUtility
{
    //
    public delegate MemoryStream GeneratePostXML(string processName, string BPMUserFullName, DataRow row);

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
        /// 发起流程申请
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="xmlStream"></param>
        public string StartProcess(string processName, DataRow row)
        {
            using (BPMConnection bpmConn = new BPMConnection())
            {
                bpmConn.Open(_eServer, _userAccount, _pwd);
                if (_tag == null)
                {
                    throw new Exception("发起流程前必须指定格式化数据流的方法GeneratePostXML！");
                }
                MemoryStream xmlStream = _tag(processName, _userFullName, row);
                PostResult result = BPMProcess.Post(bpmConn, xmlStream);

                return string.Format("[State:{0},TaskID:{1},SN:'{2}']", result.State, result.TaskID, result.SN);
            }
        }
    }
}