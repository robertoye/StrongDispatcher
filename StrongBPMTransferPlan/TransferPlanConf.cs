/* 
 * 中心仓调拨计划配置类
 * 
 * created by Roberto Ye  @2013-10-31
 * 
 * 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using StrongConfigHelper;

namespace StrongBPMTransferPlan
{
    public class TransferPlanConf
    {
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected static string _proccessName = "中心仓调拨计划";
        
        private static TransferPlanConf _conf;
        public string RuningStatus;

        public string BPMServer;
        public string UserAccount;
        public string PWD;

        public string ERPConnString;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConfFileName"></param>
        /// <returns></returns>
        public static TransferPlanConf LoadConf(string strConfFileName)
        {
            if (_conf == null)
            {
                _conf = new TransferPlanConf();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(strConfFileName);
                    XmlNode rt = XMLHelper.FindXmlNode(doc, "conf", true);
                    _conf.RuningStatus = XMLHelper.GetXmlNodeAttributes(rt, "runingstatus", true);

                    XmlNode runningNode = XMLHelper.FindXmlNode(rt, _conf.RuningStatus, true);
                    XmlNode bpmNode = XMLHelper.FindXmlNode(runningNode, "BPMServer", true);
                    XmlNode erpNode = XMLHelper.FindXmlNode(runningNode, "ERP", true);

                    _conf.BPMServer = XMLHelper.GetXmlNodeAttributes(bpmNode, "Name", true);
                    _conf.UserAccount = XMLHelper.GetXmlNodeAttributes(bpmNode, "UserAccount", true);
                    _conf.PWD = XMLHelper.GetXmlNodeAttributes(bpmNode, "PWD", true);

                    _conf.ERPConnString = XMLHelper.GetXmlNodeAttributes(erpNode, "ConnString", true);
                }
                catch (Exception err)
                {
                    _log.Error(string.Format("{0}工作流发起方法装载配置文件{2}错误，错误信息：{3}",_proccessName,strConfFileName, err.Message));
                    throw (err);
                }
            }
            return _conf;
        }
    }
}
