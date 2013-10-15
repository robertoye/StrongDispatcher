using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StrongBPMLoseInTrafic;
using NUnit.Framework;

namespace StrongBPMLoseInTraficTest
{
    [TestFixture]
    public class LoseInTraficTest
    {
        [Test]
        public void launch()
        {
            LoseInTrafic.Launch();
            Assert.IsTrue(true);

            //2013-10-14 测试发起流程通过
        }
    }
}
