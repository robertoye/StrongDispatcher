using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StrongBPMLoseInTrafic;
using NUnit.Framework;

namespace StrongBPMLoseInTraficTest
{
    [TestFixture]
    public class LoseInTraficConfTest
    {
        [Test]
        public void LoadConf()
        {
            LoseInTraficConf conf = LoseInTraficConf.LoadConf(@"D:\WorkShop\GitHub\StrongDispatcher\StrongBPMLoseInTrafic\LoseInTrafic.xml");

            Assert.AreEqual(conf.RuningStatus.ToLower(), "test");
            Assert.AreEqual(conf.BPMServer, "oa-test");
        }
    }
}
