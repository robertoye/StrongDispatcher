using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; 


namespace Test2
{
    public class Class2
    {
        protected static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        
        public void Launch2()
        {
            _log.Info("Test2.CLass2.Launch2 执行成功！");
        }
    }
}
