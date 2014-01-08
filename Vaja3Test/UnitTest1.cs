using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsControlLibrary1;
using Inteligenca_rojev;

namespace Vaja3Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Stavba s = new Stavba(5, 5);
            var count = Fitness.KvadratnaMetodaDDA(0, 0, 4, 4, s);
            Assert.AreEqual(count, 0);
            
        }
    }
}
