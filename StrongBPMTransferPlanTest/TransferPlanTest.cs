using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using NUnit.Framework;
using StrongBPMTransferPlan;

namespace StrongBPMTransferPlanTest
{
    [TestFixture]
    public class TransferPlanTest
    {
        [Test]
        public void launch()
        {
            log4net.Config.DOMConfigurator.Configure();
            //TransferPlan pl = new TransferPlan();
            TransferPlan.Launch(); 
        }
    }
}
