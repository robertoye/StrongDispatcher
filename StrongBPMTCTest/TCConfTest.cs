using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StrongBPMTC;
using NUnit.Framework;

namespace StrongBPMTCTest
{
    [TestFixture]
    public class TCConfTest
    {
        [Test]
        public void LoadConf()
        {
            TCConf conf = TCConf.LoadConf(@"D:\WorkShop\GitHub\StrongDispatcher\StrongBPMTC\TC.xml");

            Assert.AreEqual(conf.RuningStatus.ToLower(), "test");
            Assert.AreEqual(conf.BPMServer, "oa-test");
        }
    }
}
