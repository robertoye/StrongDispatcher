/*
 * 任务调度 
 * 
 * 
 * 任务模型 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading;

namespace StrongDispatcherModel
{
    /// <summary>
    /// 任务配置信息
    /// </summary>
    public class Mission
    {
        private static List<Mission> _list;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strConfFileName"></param>
        /// <returns></returns>
        public static List<Mission> LoadConf(string strConfFileName)
        {
            if (_list != null)
            {
                return _list;
            }
            _list = new List<Mission>();
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(strConfFileName);
                XmlNode rt = XMLTools.FindXmlNode(doc, "conf", true);                
                string runningStatus = XMLTools.GetXmlNodeAttributes(rt, "runingstatus", true);
                XmlNode runningNode = XMLTools.FindXmlNode(rt, runningStatus, true);

                //守护线程 --守护线程功能取消
                //守护线程错误重启时间间隔
                //int errormonitorinterval = XMLTools.GetXmlNodeAttributesToInt(runningNode, "errormonitorinterval", true);
                //Mission daemon = new Mission(eTaskType.Daemon, "Daemon", "", "", "", "", errormonitorinterval * 1000, 0, eMissionStatus.Running);                
                //_list.Add(daemon);

                XmlNodeList missionNode = runningNode.ChildNodes;
                foreach (XmlNode node in missionNode)
                {
                    string missionName = XMLTools.GetXmlNodeAttributes(node, "name", true);
                    //判定是否有重名应用,重名应用选取
                    if (!_list.Exists((c) => { return c.MissionName == missionName; }))
                    {
                        Mission mi = new Mission(node, missionName);
                        _list.Add(mi);
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception(err.Message); 
            }
            return _list;
        }

        #region --私有变量

        private eTaskType _TaskType;
        private string _MissionName;
        private string _DllLocation;
        private string _ClassName;
        private string _LaunchMethod;
        private string _ShutDownMethod;
        private int _LaunchInterval;
        private int _ErrorTryInterval;
        private eMissionStatus _MissionStatus;        

        private DateTime _NextTryTime;
        private Thread _MissionOwner;
        private ThreadState _MissionOwnerStatus;

        #endregion --私有变量

        #region --私有构建方法

        private Mission(eTaskType taskType, string missionName, string dllLocation, string className, string launchMethod,
                string shutDownMethod, int launchInterval, int _errorTryInterval, eMissionStatus missionStatus)
        {
            _TaskType = taskType;
            _MissionName = missionName;
            _DllLocation = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, dllLocation); ;
            _ClassName = className;
            _LaunchMethod = launchMethod;
            _ShutDownMethod = shutDownMethod;
            _LaunchInterval = launchInterval;
            _ErrorTryInterval = _errorTryInterval;
            _MissionStatus = missionStatus;
            _MissionOwnerStatus = ThreadState.Unstarted;
        }

        /// <summary>
        /// 读取xml节点，输出ConfItem
        /// </summary>
        /// <param name="node"></param>
        /// <param name="missionName"></param>
        /// <returns></returns>
        private Mission(XmlNode node, string missionName)
        {
            string location = XMLTools.GetXmlNodeAttributes(node, "location", true);
            string className = XMLTools.GetXmlNodeAttributes(node, "classname", true);
            string launchMethod = XMLTools.GetXmlNodeAttributes(node, "launchmethod", true);
            string shutDownMethod = XMLTools.GetXmlNodeAttributes(node, "shutdownmethod", true);
            int launchInterval = XMLTools.GetXmlNodeAttributesToInt(node, "launchinterval", true);
            int errorTryInterval = XMLTools.GetXmlNodeAttributesToInt(node, "errortryinterval", true);
            string missiondstatus = XMLTools.GetXmlNodeAttributes(node, "missionstatus", false);

            _TaskType = eTaskType.Normal;
            _MissionName = missionName;
            _DllLocation = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, location);
            _ClassName = className;
            _LaunchMethod = launchMethod;
            _ShutDownMethod = shutDownMethod;
            _LaunchInterval = launchInterval * 1000;
            _ErrorTryInterval = errorTryInterval * 1000;
            _MissionStatus = (eMissionStatus)Enum.Parse(typeof(eMissionStatus), missiondstatus, false);
            _MissionOwnerStatus = ThreadState.Unstarted;
        }
        #endregion --私有构建方法

        #region --公开属性
        public eTaskType TaskType
        {
            get
            {
                return _TaskType;
            }
        }

        public string MissionName
        {
            get
            {
                return _MissionName;
            }
        }
        public string DllLocation
        {
            get
            {
                return _DllLocation;
            }
        }
        public string ClassName
        {
            get
            {
                return _ClassName;
            }
        }
        public string LaunchMethod
        {
            get
            {
                return _LaunchMethod;
            }
        }
        public string ShutDownMethod
        {
            get
            {
                return _ShutDownMethod;
            }
        }
        public int LaunchInterval
        {
            get
            {
                return _LaunchInterval;
            }
        }

        public int ErrorTryInterval
        {
            get
            {
                return _ErrorTryInterval;
            }
        }

        public eMissionStatus MissionStatus
        {
            get
            {
                return _MissionStatus;
            }
            set
            {
                _MissionStatus = value;
            }
        }

        public DateTime NextTryTime
        {
            get
            {
                return _NextTryTime;
            }
            set
            {
                _NextTryTime = value;
            }
        }        

        /// <summary>
        /// 对应任务处理的线程
        /// </summary>
        public Thread MissionOwner
        {
            get
            {
                return _MissionOwner;
            }
            set
            {
                _MissionOwner = value;
            }
        }

        /// <summary>
        /// 对应任务处理线程的状态
        /// </summary>
        public ThreadState MissionOwnerStatus
        {
            get
            {
                return _MissionOwnerStatus;
            }
            set
            {
                _MissionOwnerStatus = value;
            }
        }

        #endregion --公开属性
    }    
}
