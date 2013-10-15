using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StrongConfigHelper
{
    public class FileHelper
    {
        /// <summary>
        /// 判定文件是否存在，先当前目录，后绝对路径，存在的话返回文件全名
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string DllFileExists(string filename)
        {            
            string dir = System.AppDomain.CurrentDomain.BaseDirectory;
            FileInfo dllFile = new FileInfo(string.Format("{0}{1}", dir, filename));
            if(!dllFile.Exists)
            {
                dllFile = new FileInfo(filename);
                if(!dllFile.Exists)
                {
                    throw new Exception(string.Format("目录{0}中，文件{0}不存在", dir,filename));
                }               
            }            
            return dllFile.FullName;
        }
        /// <summary>
        /// 检验方法
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static string MethodExist(string filename, string className, string methodName)
        {
            string strResult="";

            System.Reflection.Assembly ass = System.Reflection.Assembly.LoadFile(filename);
            if (ass == null)
            {
                strResult = string.Format("文件'{0}'不存在或者不可读！", filename);
                return strResult;
            }
            
            Type type = ass.GetType(className);//必须使用名称空间+类名称
            if (type == null)
            {
                strResult = string.Format("文件'{0}'中不存在类'{1}'！", filename, className);
                return strResult;
            }

            System.Reflection.MethodInfo method = type.GetMethod(methodName);//方法的名称
            if (method == null)
            {
                strResult = string.Format("文件'{0}'中的类'{1}'不存在方法{2}！", filename, className, methodName);            
            }
            return strResult;
        }
    }
}
