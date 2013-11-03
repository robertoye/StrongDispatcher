/*StrongDispatcher
 * 
 * created by Roberto Ye  @2013-09-30
 * 
 * ================================================
 * 变更记录：
 * 
 * 
 * 
 * 
 * 
 *  
 * ================================================
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using StrongDispatcherModel;

namespace StrongDispatcherWindowsService
{
    public partial class DispatcherService : ServiceBase
    {
        protected static log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        
        protected static bool _ServiceRunningStatus;

        protected List<Mission> _MissionList;

        public DispatcherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger.Info("Strong Dispatch服务启动！");
            _ServiceRunningStatus = true;
            
            //装载配置文件
            try
            {   
                //读取配置文件
                string ConfName = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "conf.xml");
                //读取配置文件
                _MissionList = Mission.LoadConf(ConfName);

                //一次性启动全部普通任务线程
                List<Mission> miNeedRun = _MissionList.FindAll((c) => { return c.TaskType == eTaskType.Normal && c.MissionStatus != eMissionStatus.Stop; });
                List<Thread> threads = new List<Thread>();
                foreach (Mission mi in miNeedRun)
                {
                    Thread parameterThread = new Thread(delegate()
                    {
                        ThreadDoLaunch(mi);
                    });

                    parameterThread.Name = mi.MissionName;
                    mi.MissionOwner = parameterThread;
                    threads.Add(parameterThread);
                    parameterThread.Start();
                }
            }
            catch (Exception err)
            {
                _logger.Error(err);
            }
        }

        protected override void OnStop()
        {
            _ServiceRunningStatus = false;
            _logger.Info("Strong Dispatch服务开始停止！");
            foreach (Mission mi in _MissionList)
            {
                if (mi.MissionOwner != null)
                {
                    mi.MissionOwner.Abort();
                }
            }            
            Thread.Sleep(60000);
            _logger.Info("Strong Dispatch服务已经停止！");            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mi"></param>         
        private static void ThreadDoLaunch(Mission mi)
        {
            while (_ServiceRunningStatus)
            {
                try
                {
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                    InvokeAssemblyMethod(mi, mi.LaunchMethod);
                    _logger.Info(string.Format("Strong Dispatch服务：调用任务{0}的方法{1}.{2}成功！下次任务在{3}毫秒后再次启动！", mi.MissionName,
                                mi.ClassName, mi.LaunchMethod, mi.LaunchInterval));
                    Thread.Sleep(mi.LaunchInterval);
                }
                catch (Exception err)
                {
                    mi.MissionStatus = eMissionStatus.ErrorHalt;
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                    _logger.Error(err);
                    _logger.Info(string.Format("Strong Dispatch服务：调用任务{0}的方法{1}.{2}失败！下次任务在{3}毫秒后再次启动！", mi.MissionName,
                            mi.ClassName, mi.LaunchMethod, mi.ErrorTryInterval));
                    Thread.Sleep(mi.ErrorTryInterval);
                }
            }
        }
        /// <summary>
        /// 反射调用方法
        /// </summary>
        /// <param name="cfi"></param>
        /// <param name="methodName"></param>
        private static void InvokeAssemblyMethod(Mission cfi, string methodName)
        {
            System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFile(cfi.DllLocation);

            lock (ass)
            {
                Type type = ass.GetType(cfi.ClassName);//必须使用名称空间+类名称
                System.Reflection.MethodInfo method = type.GetMethod(methodName);//方法的名称
                Object obj = ass.CreateInstance(cfi.ClassName);//必须使用名称空间+类名称
                string s = (string)method.Invoke(obj, null); //实例方法的调用           
            }
        }  
    }
}
