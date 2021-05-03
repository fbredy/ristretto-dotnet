using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ristretto.Test
{
    [TestClass]
    public class EdwardsBasepointTableTest
    {
        [TestMethod]
        public void scalarMulVsEd25519py()
        {
            EdwardsBasepointTable Bt = new EdwardsBasepointTable(Constants.ED25519_BASEPOINT);
            EdwardsPoint aB = Bt.Multiply(EdwardsPointTest.A_SCALAR);
            Assert.AreEqual(EdwardsPointTest.A_TIMES_BASEPOINT, aB.Compress());
        }
    }
}
