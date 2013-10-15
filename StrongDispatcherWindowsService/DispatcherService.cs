using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using log4net;

using StrongDispatcherModel;

namespace StrongDispatcherWindowsService
{
    public partial class DispatcherService : ServiceBase
    {
        protected List<Mission> MissionList;
        public DispatcherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log4net.Config.DOMConfigurator.Configure();
            ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            _logger.Info("started");
            
            //装载配置文件
            try
            {
                //MessageBox.Show(System.AppDomain.CurrentDomain.BaseDirectory);
                //读取配置文件
                string ConfName = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "conf.xml");
                //读取配置文件
                MissionList = Mission.LoadConf(ConfName);

                //一次性启动全部普通任务线程
                List<Mission> miNeedRun = MissionList.FindAll((c) => { return c.TaskType == eTaskType.Normal && c.MissionStatus != eMissionStatus.Stop; });
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
                    //Console.WriteLine(string.Format("# {0}: Mission thread {1} runing! ", DateTime.Now, mi.MissionName));
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        protected override void OnStop()
        {
            foreach (Mission mi in MissionList)
            {
                if (mi.MissionOwner != null)
                {
                    mi.MissionOwner.Abort();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mi"></param>         
        private static void ThreadDoLaunch(Mission mi)
        {
            while (true)
            {
                try
                {
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                    InvokeAssemblyMethod(mi, mi.LaunchMethod);
                    Console.WriteLine(string.Format("{0}:Normal Mission {1} Call Methord {2} Succeed!", DateTime.Now, mi.MissionName, mi.LaunchMethod));
                    Thread.Sleep(mi.LaunchInterval);
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                }
                catch (Exception err)
                {
                    mi.MissionStatus = eMissionStatus.ErrorHalt;
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                    Console.WriteLine(string.Format("{0}:Normal Mission {1} Call Methord {2} Failure!", DateTime.Now, mi.MissionName, mi.LaunchMethod));
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
