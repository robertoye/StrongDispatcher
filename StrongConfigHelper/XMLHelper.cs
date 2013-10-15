using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace StrongConfigHelper
{
    /// <summary>
    /// xml文件分析工具
    /// </summary>
    public class XMLHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeName"></param>
        /// <param name="raiseErr"></param>
        /// <returns></returns>
        public static XmlNode FindXmlNode(XmlNode node, string nodeName, bool raiseErr)
        {
            XmlNode rt = node.SelectSingleNode(nodeName);

            if (raiseErr && rt == null)
            {
                throw new Exception(string.Format("配置文件格式错误，{0}下找不到子节点{1}！", node.Name, nodeName));
            }
            return rt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="nodeName"></param>
        /// <param name="raiseErr"></param>
        /// <returns></returns>
        public static XmlNode FindXmlNode(XmlDocument doc, string nodeName, bool raiseErr)
        {
            XmlNode rt = doc.SelectSingleNode(nodeName);

            if (raiseErr && rt == null)
            {
                throw new Exception(string.Format("配置文件格式错误，找不到节点{0}！", nodeName));
            }
            return rt;
        }             

        /// <summary>
        /// 获取xml节点属性值
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attName"></param>
        /// <param name="raiseErr"></param>
        /// <returns></returns>
        public static string GetXmlNodeAttributes(XmlNode node, string attName, bool raiseErr)
        {
            string strResult = "";
            XmlAttribute xmlatt = node.Attributes[attName];

            if (xmlatt != null)
            {
                strResult = xmlatt.Value;
            }
            else
            {
                if (raiseErr)
                {
                    throw new Exception(string.Format("配置文件格式错误，节点{0}找不到属性{1}！", node.Name, attName));
                }
            }
            return strResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attName"></param>
        /// <param name="raiseErr"></param>
        /// <returns></returns>
        public static int GetXmlNodeAttributesToInt(XmlNode node, string attName, bool raiseErr)
        {
            int result = 0;
            string strResult = XMLHelper.GetXmlNodeAttributes(node, attName, raiseErr);
            if (raiseErr && !int.TryParse(strResult, out result))
            {
                throw new Exception(string.Format("配置文件格式错误，找不到属性{0},或者属性{0}不是int格式！", attName));
            }

            return result;
        }  
    }
}
