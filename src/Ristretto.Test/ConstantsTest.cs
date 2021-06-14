using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ristretto.Test
{
    [TestClass]
    public class ConstantsTest
    {
        [TestMethod]
        public void checkEdwardsD()
        {
            Assert.AreEqual(
                FieldElement.FromByteArray(StrUtils.HexToBytes("a3785913ca4deb75abd841414d0a700098e879777940c78c73fe6f2bee6c0352")),
                Constants.EDWARDS_D);
        }

        [TestMethod]
        public void checkEdwards2D()
        {
            FieldElement two = FieldElement.ONE.Add(FieldElement.ONE);
            Assert.AreEqual(Constants.EDWARDS_2D, Constants.EDWARDS_D.Multiply(two));
        }

        [TestMethod]
        public void checkSqrtADMinusOne()
        {
            //System.out.println(
            //        FieldElement.fromByteArray(Constants.SQRT_AD_MINUS_ONE.toByteArray()).printInternalRepresentation());
            Assert.AreEqual(Constants.EDWARDS_D, Constants.SQRT_AD_MINUS_ONE.Square().Add(FieldElement.ONE).Negate());
        }

        [TestMethod]
        public void checkInvSqrtAMinusD()
        {
            Assert.AreEqual(
                Constants.EDWARDS_D,
                Constants.INVSQRT_A_MINUS_D.Invert().Square().Add(FieldElement.ONE).Negate());
        }

        [TestMethod]
        public void checkSqrtM1()
        {
            Assert.AreEqual(Constants.SQRT_M1, FieldElement
                    .FromByteArray(StrUtils.HexToBytes("b0a00e4a271beec478e42fad0618432fa7d7fb3d99004d2b0bdfc14f8024832b")));
        }

        [TestMethod]
        public void checkEd25519Basepoint()
        {
            CompressedEdwardsY encoded = new CompressedEdwardsY(
                    StrUtils.HexToBytes("5866666666666666666666666666666666666666666666666666666666666666"));
            EdwardsPoint B = encoded.Decompress();
            Assert.AreEqual(Constants.ED25519_BASEPOINT.X, B.X);
            Assert.AreEqual(Constants.ED25519_BASEPOINT.Y, B.Y);
            Assert.AreEqual(Constants.ED25519_BASEPOINT.Z, B.Z);
            Assert.AreEqual(Constants.ED25519_BASEPOINT.T, B.T);
        }
    }
}
