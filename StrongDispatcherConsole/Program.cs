using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using log4net;

using StrongDispatcherModel;

namespace StrongDispatcherConsole
{
    class Program
    {
        protected static log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            //string logConf = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "StrongDispatcherConsole.exe.config");
            //log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(logConf));
            log4net.Config.XmlConfigurator.Configure();
            
            _logger.Info("started");
            _logger.Error("started");

            List<Mission> MissionList;
            //装载配置文件
            try
            {
                //MessageBox.Show(System.AppDomain.CurrentDomain.BaseDirectory);
                //读取配置文件
                string ConfName = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "conf.xml");                
                //读取配置文件
                MissionList = Mission.LoadConf(ConfName);
                _logger.Info("读取配置文件conf.xml成功！");

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
                    Console.WriteLine(string.Format("# {0}: Mission thread {1} runing! ", DateTime.Now, mi.MissionName));
                }

                //foreach (Thread tt in threads)
                //{
                //    tt.Start();
                //    Console.WriteLine(string.Format("# {0}: Mission thread {1} runing! ", DateTime.Now, tt.Name));
                //}
                
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                _logger.Error(err);                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mi"></param>         
        private static void ThreadDoLaunch(Mission mi)
        {   
            while(true)
            {
                try
                {
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                    InvokeAssemblyMethod(mi, mi.LaunchMethod);
                    Console.WriteLine(string.Format("{0}:Normal Mission {1} Call Methord {2} Succeed!", DateTime.Now,mi.MissionName,mi.LaunchMethod));
                    Thread.Sleep(mi.LaunchInterval);
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                }
                catch (Exception err)
                {
                    mi.MissionStatus = eMissionStatus.ErrorHalt;
                    //mi.MissionOwnerStatus = Thread.CurrentThread.ThreadState;
                    Console.WriteLine(string.Format("{0}:Normal Mission {1} Call Methord {2} Failure!Error Msg:{3}", DateTime.Now, mi.MissionName, mi.LaunchMethod,err.Message));
                    Thread.Sleep(mi.ErrorTryInterval);
                    //throw (err);
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
