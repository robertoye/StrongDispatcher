using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using StrongConfigHelper;

namespace StrongBPMTC
{
    public class TCConf
    {
        private static TCConf _conf;

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
        public static TCConf LoadConf(string strConfFileName)
        {
            if (_conf != null)
            {
                return _conf;
            }
            _conf = new TCConf(); 
            
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
                throw new Exception(err.Message);
            }

            return _conf;
        }
    }
}
