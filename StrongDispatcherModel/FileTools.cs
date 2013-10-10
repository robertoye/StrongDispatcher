using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace StrongDispatcherModel
{
    internal class FileTools
    {
        /// <summary>
        /// 判定文件是否存在，先当前目录，后绝对路径，存在的话返回文件全名
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static string DllFileExist(string filename)
        {            
            string dir = System.AppDomain.CurrentDomain.BaseDirectory;
            FileInfo dllFile = new FileInfo(string.Format("{0}{1}", dir, filename));
            if(!dllFile.Exists)
            {
                dllFile = new FileInfo(filename);
                if(!dllFile.Exists)
                {
                    throw new Exception(string.Format("文件{0}不存在",filename));
                }               
            }            
            return dllFile.FullName;
        }
    }
}
