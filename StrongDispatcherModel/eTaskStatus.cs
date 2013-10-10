using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrongDispatcherModel
{    
    /// <summary>
    /// 线程运行状态
    /// </summary>
    public enum eTaskStatus
    {
        //正在处理
        Doing = 1,
        //休眠
        Sleeping =2,
        //退出
        Cancel = 3
    }
}
