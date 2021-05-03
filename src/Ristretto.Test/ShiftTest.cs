using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ristretto.Test
{
    [TestClass]
    public class ShiftTest
    {
        [TestMethod]
        public void Should()
        {
            int plop = 257;
            byte x = (byte)plop;
            Assert.AreEqual(1, x);
        }
    }
}
