using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading; 

namespace Test1
{
    public class Class1
    {

        protected static log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        

        public void Launch1()
        {
            _log.Info("Test1.CLass1.Launch 执行成功！");
        }        
    }
}
