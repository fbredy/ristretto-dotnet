using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ristretto.Test
{
    [TestClass]
    public class EdwardsPointTest
    {
        /**
         * Compressed Edwards Y form of the Ed25519 basepoint.
         */
        public static readonly CompressedEdwardsY ED25519_BASEPOINT_COMPRESSED = new CompressedEdwardsY(
                StrUtils.hexToBytes("5866666666666666666666666666666666666666666666666666666666666666"));

        /**
         * Compressed Edwards Y form of 2*basepoint.
         */
        public static readonly CompressedEdwardsY BASE2_CMPRSSD = new CompressedEdwardsY(
                StrUtils.hexToBytes("c9a3f86aae465f0e56513864510f3997561fa2c9e85ea21dc2292309f3cd6022"));

        /**
         * Compressed Edwards Y form of 16*basepoint.
         */
        public static readonly CompressedEdwardsY BASE16_CMPRSSD = new CompressedEdwardsY(
                StrUtils.hexToBytes("eb2767c137ab7ad8279c078eff116ab0786ead3a2e0f989f72c37f82f2969670"));

        /**
         * 4493907448824000747700850167940867464579944529806937181821189941592931634714
         */
        public static readonly Scalar A_SCALAR = new Scalar(
                StrUtils.hexToBytes("1a0e978a90f6622d3747023f8ad8264da758aa1b88e040d1589e7b7f2376ef09"));

        /**
         * 2506056684125797857694181776241676200180934651973138769173342316833279714961
         */
        public static readonly Scalar B_SCALAR = new Scalar(
                StrUtils.hexToBytes("91267acf25c2091ba217747b66f0b32e9df2a56741cfdac456a7d4aab8608a05"));

        /**
         * A_SCALAR * basepoint, computed with ed25519.py
         */
        public static readonly CompressedEdwardsY A_TIMES_BASEPOINT = new CompressedEdwardsY(
                StrUtils.hexToBytes("ea27e26053df1b5956f14d5dec3c34c384a269b74cc3803ea8e2e7c9425e40a5"));

        /**
         * A_SCALAR * (A_TIMES_BASEPOINT) + B_SCALAR * BASEPOINT computed with
         * ed25519.py
         */
        public static readonly CompressedEdwardsY DOUBLE_SCALAR_MULT_RESULT = new CompressedEdwardsY(
                StrUtils.hexToBytes("7dfd6c45af6d6e0eba20371a236459c4c0468343de704b85096ffe354f132b42"));

        /**
         * The 8-torsion subgroup $\mathcal E [8]$.
         * <p>
         * In the case of Curve25519, it is cyclic; the $i$-th element of the array is
         * $[i]P$, where $P$ is a point of order $8$ generating $\mathcal E[8]$.
         * <p>
         * Thus $\mathcal E[8]$ is the points indexed by 0,2,4,6, and $\mathcal E[2]$ is
         * the points indexed by 0,4.
         */
        public static readonly CompressedEdwardsY[] EIGHT_TORSION_COMPRESSED = new CompressedEdwardsY[] {
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("0100000000000000000000000000000000000000000000000000000000000000")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("c7176a703d4dd84fba3c0b760d10670f2a2053fa2c39ccc64ec7fd7792ac037a")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("0000000000000000000000000000000000000000000000000000000000000080")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("26e8958fc2b227b045c3f489f2ef98f0d5dfac05d3c63339b13802886d53fc05")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("ecffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("26e8958fc2b227b045c3f489f2ef98f0d5dfac05d3c63339b13802886d53fc85")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("0000000000000000000000000000000000000000000000000000000000000000")),
            new CompressedEdwardsY(
                    StrUtils.hexToBytes("c7176a703d4dd84fba3c0b760d10670f2a2053fa2c39ccc64ec7fd7792ac03fa")) };

        [TestMethod]
        public void basepointDecompressionCompression()
        {
            EdwardsPoint B = ED25519_BASEPOINT_COMPRESSED.Decompress();
            Assert.AreEqual(ED25519_BASEPOINT_COMPRESSED, B.Compress());
        }

        //[TestMethod]
        //public void basepointSerializeDeserialize()
        //{ 
        //    ByteArrayOutputStream baos = new ByteArrayOutputStream();
        //    ObjectOutputStream oos = new ObjectOutputStream(baos);
        //    oos.writeObject(Constants.ED25519_BASEPOINT);
        //        oos.close();
        //        ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
        //    ObjectInputStream ois = new ObjectInputStream(bais);
        //    EdwardsPoint B = (EdwardsPoint)ois.readObject();
        //    Assert.AreEqual(B, is(Constants.ED25519_BASEPOINT));
        //}

        [TestMethod]
        public void decompressionSignHandling()
        {
            // Manually set the high bit of the last byte to flip the sign
            byte[]
        minusBasepointBytes = ED25519_BASEPOINT_COMPRESSED.ToByteArray();
            minusBasepointBytes[31] |= 1 << 7;
            EdwardsPoint minusB = new CompressedEdwardsY(minusBasepointBytes).Decompress();
            // Test projective coordinates exactly since we know they should
            // only differ by a flipped sign.
            Assert.AreEqual(Constants.ED25519_BASEPOINT.X.Negate(), minusB.X);
            Assert.AreEqual(Constants.ED25519_BASEPOINT.Y, minusB.Y);
            Assert.AreEqual(Constants.ED25519_BASEPOINT.Z, minusB.Z);
            Assert.AreEqual(Constants.ED25519_BASEPOINT.T.Negate(), minusB.T);
        }

        [TestMethod]
        public void ctSelectReturnsCorrectResult()
        {
            Assert.AreEqual(Constants.ED25519_BASEPOINT, Constants.ED25519_BASEPOINT.CtSelect(EdwardsPoint.IDENTITY, 0));
            Assert.AreEqual(EdwardsPoint.IDENTITY, Constants.ED25519_BASEPOINT.CtSelect(EdwardsPoint.IDENTITY, 1));
            Assert.AreEqual(EdwardsPoint.IDENTITY, EdwardsPoint.IDENTITY.CtSelect(Constants.ED25519_BASEPOINT, 0));
            Assert.AreEqual(Constants.ED25519_BASEPOINT, EdwardsPoint.IDENTITY.CtSelect(Constants.ED25519_BASEPOINT, 1));
        }

        [TestMethod]
        public void basepointPlusBasepointVsBasepoint2Constant()
        {
            EdwardsPoint B2 = Constants.ED25519_BASEPOINT.Add(Constants.ED25519_BASEPOINT);
            Assert.AreEqual(BASE2_CMPRSSD, B2.Compress());
        }

        [TestMethod]
        public void basepointPlusBasepointProjectiveNielsVsBasepoint2Constant()
        {
            EdwardsPoint B2 = Constants.ED25519_BASEPOINT.Add(Constants.ED25519_BASEPOINT.ToProjectiveNiels()).ToExtended();
            Assert.AreEqual(BASE2_CMPRSSD, B2.Compress());
        }

        [TestMethod]
        public void basepointPlusBasepointAffineNielsVsBasepoint2Constant()
        {
            EdwardsPoint B2 = Constants.ED25519_BASEPOINT.Add(Constants.ED25519_BASEPOINT.ToAffineNiels()).ToExtended();
            Assert.AreEqual(BASE2_CMPRSSD, B2.Compress());
        }

        [TestMethod]
        public void basepointDoubleVsBasepoint2Constant()
        {
            EdwardsPoint B2 = Constants.ED25519_BASEPOINT.Double();
            Assert.AreEqual(BASE2_CMPRSSD, B2.Compress());
        }

        [TestMethod]
        public void basepointDoubleMinusBasepoint()
        {
            EdwardsPoint B2 = Constants.ED25519_BASEPOINT.Double();
            Assert.AreEqual(Constants.ED25519_BASEPOINT, B2.Subtract(Constants.ED25519_BASEPOINT));
        }

        [TestMethod]
        public void basepointNegateVsZeroMinusBasepoint()
        {
            Assert.AreEqual(Constants.ED25519_BASEPOINT.Negate(),
                EdwardsPoint.IDENTITY.Subtract(Constants.ED25519_BASEPOINT));
        }

        [TestMethod]
        public void scalarMulVsEd25519py()
        {
            EdwardsPoint aB = Constants.ED25519_BASEPOINT.Multiply(A_SCALAR);
            Assert.AreEqual(A_TIMES_BASEPOINT, aB.Compress());
        }

        [TestMethod]
        public void testVartimeDoubleScalarMultiplyBasepoint()
        {
            // Little-endian
            Scalar zero = Scalar.ZERO;
            Scalar one = Scalar.ONE;
            Scalar two = new Scalar(StrUtils.hexToBytes("0200000000000000000000000000000000000000000000000000000000000000"));
            Scalar a = new Scalar(StrUtils.hexToBytes("d072f8dd9c07fa7bc8d22a4b325d26301ee9202f6db89aa7c3731529e37e437c"));
            EdwardsPoint A = new CompressedEdwardsY(
                    StrUtils.hexToBytes("d4cf8595571830644bd14af416954d09ab7159751ad9e0f7a6cbd92379e71a66")).Decompress();
            EdwardsPoint B = Constants.ED25519_BASEPOINT;
            EdwardsPoint I = EdwardsPoint.IDENTITY;

            // 0 * I + 0 * B = I
            Assert.AreEqual(I, EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(zero, I, zero));
            // 1 * I + 0 * B = I
            Assert.AreEqual(I, EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(one, I, zero));
            // 1 * I + 1 * B = B
            Assert.AreEqual(B, EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(one, I, one));
            // 1 * B + 1 * B = 2 * B
            Assert.AreEqual(B.Double(), EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(one, B, one));
            // 1 * B + 2 * B = 3 * B
            Assert.AreEqual(B.Double().Add(B), EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(one, B, two));
            // 2 * B + 2 * B = 4 * B
            Assert.AreEqual(B.Double().Double(), EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(two, B, two));

            // 0 * B + a * B = A
            Assert.AreEqual(A, EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(zero, B, a));
            // a * B + 0 * B = A
            Assert.AreEqual(A, EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(a, B, zero));
            // a * B + a * B = 2 * A
            Assert.AreEqual(A.Double(), EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(a, B, a));
        }

        [TestMethod]
        public void doubleScalarMulBasepointVsEd25519py()
        {
            EdwardsPoint A = A_TIMES_BASEPOINT.Decompress();
            EdwardsPoint result = EdwardsPoint.VartimeDoubleScalarMultiplyBasepoint(A_SCALAR, A, B_SCALAR);
            Assert.AreEqual(DOUBLE_SCALAR_MULT_RESULT, result.Compress());
        }

        [TestMethod]
        public void basepointMulByPow24VsBasepoint16Constant()
        {
            Assert.AreEqual(BASE16_CMPRSSD.Decompress(), Constants.ED25519_BASEPOINT.MultiplyByPow2(4));
        }

        [TestMethod]
        public void isIdentity()
        {
            Assert.IsTrue(EdwardsPoint.IDENTITY.IsIdentity());
            Assert.IsFalse(Constants.ED25519_BASEPOINT.IsIdentity());
        }

        [TestMethod]
        public void isSmallOrder()
        {
            // The basepoint has large prime order
            Assert.IsFalse(Constants.ED25519_BASEPOINT.IsSmallOrder());
            // EIGHT_TORSION_COMPRESSED has all points of small order.
            for (int i = 0; i < EIGHT_TORSION_COMPRESSED.Length; i++)
            {
                Assert.IsTrue(EIGHT_TORSION_COMPRESSED[i].Decompress().IsSmallOrder());
            }
        }

        [TestMethod]
        public void isTorsionFree()
        {
            // The basepoint is torsion-free.
            Assert.IsTrue(Constants.ED25519_BASEPOINT.IsTorsionFree());

            // Adding the identity leaves it torsion-free.
            Assert.IsTrue(Constants.ED25519_BASEPOINT.Add(EdwardsPoint.IDENTITY).IsTorsionFree());

            // Adding any of the 8-torsion points to it (except the identity) affects the
            // result.
            Assert.AreEqual(EIGHT_TORSION_COMPRESSED[0], EdwardsPoint.IDENTITY.Compress());
            for (int i = 1; i < EIGHT_TORSION_COMPRESSED.Length; i++)
            {
                EdwardsPoint withTorsion = Constants.ED25519_BASEPOINT.Add(EIGHT_TORSION_COMPRESSED[i].Decompress());
                Assert.IsFalse(withTorsion.IsTorsionFree());
            }
        }
    }

}
