using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using StrongDispatcherModel;

namespace StrongDispatcherConsole
{
    class Program
    {
        private static List<Mission> MissionList;        
        private static int _ErrorMonitorInterval;

        static void Main(string[] args)
        {
            //装载配置文件
            try
            {
                //MessageBox.Show(System.AppDomain.CurrentDomain.BaseDirectory);
                //读取配置文件
                string ConfName = string.Format(@"{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, "conf.xml");
                //读取配置文件
                MissionList = Mission.LoadConf(ConfName);

                foreach (Mission mi in MissionList)
                {
                    if (mi.TaskType == eTaskType.Daemon)
                    {
                        Thread daemonThread = new Thread(delegate()
                        {
                            ErrorTryToDo(MissionList,mi.LanchInterval);
                        });
                        daemonThread.Name = mi.MissionName;
                        daemonThread.Start();
                        Console.WriteLine(string.Format("{0}:Daemon Mission '{1}' Startted;", DateTime.Now, mi.MissionName));
                        mi.MissionOwner = daemonThread;
                    }
                    else
                    {
                        Thread missionThread = new Thread(delegate()
                        {
                            ThreadDo(mi);
                        });
                        missionThread.Name = mi.MissionName;
                        missionThread.Start();
                        Console.WriteLine(string.Format("{0}:Normal Mission '{1}' Startted;", DateTime.Now, mi.MissionName));
                        mi.MissionOwner = missionThread;
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        private static void ThreadDo(Mission mi)
        {
            while (mi.MissionOwnerStatus == eTaskStatus.Sleeping)
            {
                try
                {
                    mi.MissionOwnerStatus = eTaskStatus.Doing;
                    InvokeAssemblyMethod(mi, mi.LanchMethod);
                    Console.WriteLine(string.Format("{0}:Normal Mission {1} Call Methord {2} Succeed!", DateTime.Now, mi.MissionName,mi.LanchMethod));
                    mi.MissionOwnerStatus = eTaskStatus.Sleeping;
                }
                catch (Exception err)
                {
                    mi.MissionOwnerStatus = eTaskStatus.Cancel;
                    mi.MissionStatus = eMissionStatus.ErrorHalt;

                    mi.NextTryTime = DateTime.Now.AddMinutes(mi.ErrorTryInterval);
                    Thread.ResetAbort();
                    Console.WriteLine(string.Format("{0}:Normal Mission {1} Call Methord {2} Failure!", DateTime.Now, mi.MissionName, mi.LanchMethod));
                }
                Thread.Sleep(mi.LanchInterval);
            }
        }

        private static void InvokeAssemblyMethod(Mission cfi, string methodName)
        {
            System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFile(cfi.DllLocation);
            Type type = ass.GetType(cfi.ClassName);//必须使用名称空间+类名称

            System.Reflection.MethodInfo method = type.GetMethod(methodName);//方法的名称
            Object obj = ass.CreateInstance(cfi.ClassName);//必须使用名称空间+类名称

            string s = (string)method.Invoke(obj, null); //实例方法的调用
        }

        private static void ErrorTryToDo(List<Mission> miList, int ErrorMonitorInterval)
        {
            foreach (Mission mi in miList)
            {
                if (mi.MissionStatus == eMissionStatus.ErrorHalt && DateTime.Now <= mi.NextTryTime)
                {
                    Thread parameterThread = new Thread(delegate()
                    {
                        ThreadDo(mi);
                    });
                    parameterThread.Name = mi.MissionName;
                    parameterThread.Start(mi);
                }
            }

            Thread.Sleep(ErrorMonitorInterval);
        }
    }
}
