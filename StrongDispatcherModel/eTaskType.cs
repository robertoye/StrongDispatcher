using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrongDispatcherModel
{
    /// <summary>
    /// 线程类别
    /// </summary>
    public enum eTaskType
    {
        //守护线程
        Daemon = 1,
        //普通任务线程
        Normal = 2
    }
}
