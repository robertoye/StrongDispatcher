using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrongDispatcherModel
{
    /// <summary>
    /// 线程运行状态
    /// </summary>
    public enum eMissionStatus
    {
        //运行
        Running,
        //已经停止
        Stop,
        //错误停止
        ErrorHalt
    }
}
