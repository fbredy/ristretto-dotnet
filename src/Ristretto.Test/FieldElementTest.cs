using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ristretto.Test
{
    [TestClass]
    public class FieldElementTest
    {
        // Test vectors below, and the tests they are used in, are from
        // curve25519-dalek.
        // https://github.com/dalek-cryptography/curve25519-dalek/blob/4bdccd7b7c394d9f8ffc4b29d5acc23c972f3d7a/src/field.rs#L280-L301

        // Random element a of GF(2^255-19), from Sage
        // a = 1070314506888354081329385823235218444233221\
        // 2228051251926706380353716438957572
        static readonly byte[] A_BYTES = { 0x04, (byte) 0xfe, (byte) 0xdf, (byte) 0x98, (byte) 0xa7, (byte) 0xfa, 0x0a, 0x68,
            (byte) 0x84, (byte) 0x92, (byte) 0xbd, 0x59, 0x08, 0x07, (byte) 0xa7, 0x03, (byte) 0x9e, (byte) 0xd1,
            (byte) 0xf6, (byte) 0xf2, (byte) 0xe1, (byte) 0xd9, (byte) 0xe2, (byte) 0xa4, (byte) 0xa4, 0x51, 0x47, 0x36,
            (byte) 0xf3, (byte) 0xc3, (byte) 0xa9, 0x17 };

        // Byte representation of a**2
        static readonly byte[] ASQ_BYTES = { 0x75, (byte) 0x97, 0x24, (byte) 0x9e, (byte) 0xe6, 0x06, (byte) 0xfe, (byte) 0xab,
            0x24, 0x04, 0x56, 0x68, 0x07, (byte) 0x91, 0x2d, 0x5d, 0x0b, 0x0f, 0x3f, 0x1c, (byte) 0xb2, 0x6e,
            (byte) 0xf2, (byte) 0xe2, 0x63, (byte) 0x9c, 0x12, (byte) 0xba, 0x73, 0x0b, (byte) 0xe3, 0x62 };

        // Byte representation of 1/a
        static readonly byte[] AINV_BYTES = { (byte) 0x96, 0x1b, (byte) 0xcd, (byte) 0x8d, 0x4d, 0x5e, (byte) 0xa2, 0x3a,
            (byte) 0xe9, 0x36, 0x37, (byte) 0x93, (byte) 0xdb, 0x7b, 0x4d, 0x70, (byte) 0xb8, 0x0d, (byte) 0xc0, 0x55,
            (byte) 0xd0, 0x4c, 0x1d, 0x7b, (byte) 0x90, 0x71, (byte) 0xd8, (byte) 0xe9, (byte) 0xb6, 0x18, (byte) 0xe6,
            0x30 };

        // Byte representation of a^((p-5)/8)
        static readonly byte[] AP58_BYTES = { 0x6a, 0x4f, 0x24, (byte) 0x89, 0x1f, 0x57, 0x60, 0x36, (byte) 0xd0, (byte) 0xbe,
            0x12, 0x3c, (byte) 0x8f, (byte) 0xf5, (byte) 0xb1, 0x59, (byte) 0xe0, (byte) 0xf0, (byte) 0xb8, 0x1b, 0x20,
            (byte) 0xd2, (byte) 0xb5, 0x1f, 0x15, 0x21, (byte) 0xf9, (byte) 0xe3, (byte) 0xe1, 0x61, 0x21, 0x55 };

        [TestMethod]
        public void testAMulAVsASquaredConstant()
        {
            FieldElement a = FieldElement.FromByteArray(A_BYTES);
            FieldElement asq = FieldElement.FromByteArray(ASQ_BYTES);
            Assert.AreEqual(asq, a.Multiply(a));
        }

        [TestMethod]
        public void testASquareVsASquaredConstant()
        {
            FieldElement a = FieldElement.FromByteArray(A_BYTES);
            FieldElement asq = FieldElement.FromByteArray(ASQ_BYTES);
            Assert.AreEqual(asq, a.Square());
        }

        [TestMethod]
        public void testASquare2VsASquaredConstant()
        {
            FieldElement a = FieldElement.FromByteArray(A_BYTES);
            FieldElement asq = FieldElement.FromByteArray(ASQ_BYTES);
            Assert.AreEqual(asq.Add(asq), a.SquareAndDouble());
        }

        [TestMethod]
        public void testAInvertVsInverseOfAConstant()
        {
            FieldElement a = FieldElement.FromByteArray(A_BYTES);
            FieldElement ainv = FieldElement.FromByteArray(AINV_BYTES);
            FieldElement shouldBeInverse = a.Invert();
            Assert.AreEqual(ainv, shouldBeInverse);
            Assert.AreEqual(FieldElement.ONE, a.Multiply(shouldBeInverse));
        }

        [TestMethod]
        public void sqrtRatioM1Behavior()
        {
            FieldElement zero = FieldElement.ZERO;
            FieldElement one = FieldElement.ONE;
            FieldElement i = Constants.SQRT_M1;
            FieldElement two = one.Add(one); // 2 is nonsquare mod p.
            FieldElement four = two.Add(two); // 4 is square mod p.
            FieldElement.SqrtRatioM1Result sqrt;

            // 0/0 should return (1, 0) since u is 0
            sqrt = FieldElement.SqrtRatioM1(zero, zero);
            Assert.AreEqual(1, sqrt.WasSquare);
            Assert.AreEqual(zero, sqrt.Result);
            Assert.AreEqual(0, sqrt.Result.IsNegative());

            // 1/0 should return (0, 0) since v is 0, u is nonzero
            sqrt = FieldElement.SqrtRatioM1(one, zero);
            Assert.AreEqual(0, sqrt.WasSquare);
            Assert.AreEqual(zero, sqrt.Result);
            Assert.AreEqual(0, sqrt.Result.IsNegative());

            // 2/1 is nonsquare, so we expect (0, sqrt(i*2))
            sqrt = FieldElement.SqrtRatioM1(two, one);
            Assert.AreEqual(0, sqrt.WasSquare);
            Assert.AreEqual(two.Multiply(i), sqrt.Result.Square());
            Assert.AreEqual(0, sqrt.Result.IsNegative());

            // 4/1 is square, so we expect (1, sqrt(4))
            sqrt = FieldElement.SqrtRatioM1(four, one);
            Assert.AreEqual(1, sqrt.WasSquare);
            Assert.AreEqual(four, sqrt.Result.Square());
            Assert.AreEqual(0, sqrt.Result.IsNegative());

            // 1/4 is square, so we expect (1, 1/sqrt(4))
            sqrt = FieldElement.SqrtRatioM1(one, four);
            Assert.AreEqual(1, sqrt.WasSquare);
            Assert.AreEqual(one, sqrt.Result.Square().Multiply(four));
            Assert.AreEqual(0, sqrt.Result.IsNegative());
        }

        [TestMethod]
        public void testAP58VsAP58Constant()
        {
            FieldElement a = FieldElement.FromByteArray(A_BYTES);
            FieldElement ap58 = FieldElement.FromByteArray(AP58_BYTES);
            Assert.AreEqual(ap58, a.PowP58());
        }

        [TestMethod]
        public void equality()
        {
            FieldElement a = FieldElement.FromByteArray(A_BYTES);
            FieldElement ainv = FieldElement.FromByteArray(AINV_BYTES);
            Assert.AreEqual(a, a);
            Assert.AreNotEqual(a, ainv);
        }

        // Notice that the last element has the high bit set, which
        // should be ignored.
        static readonly byte[] B_BYTES = { 113, (byte) 191, (byte) 169, (byte) 143, 91, (byte) 234, 121, 15, (byte) 241,
            (byte) 131, (byte) 217, 36, (byte) 230, 101, 92, (byte) 234, 8, (byte) 208, (byte) 170, (byte) 251, 97, 127,
            70, (byte) 210, 58, 23, (byte) 166, 87, (byte) 240, (byte) 169, (byte) 184, (byte) 178 };

        [TestMethod]
        public void fromByteArrayHighbitIsIgnored()
        {
            byte[] cleared_bytes = B_BYTES;
            cleared_bytes[31] &= 127;
            FieldElement withHighbitSet = FieldElement.FromByteArray(B_BYTES);
            FieldElement withoutHighbitSet = FieldElement.FromByteArray(cleared_bytes);
            Assert.AreEqual(withHighbitSet, withoutHighbitSet);
        }

        [TestMethod]
        public void encodingIsCanonical()
        {
            // Encode 1 wrongly as 1 + (2^255 - 19) = 2^255 - 18
            byte[] oneEncodedWronglyBytes = { (byte) 0xee, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff,
                (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff,
                (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff,
                (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff, (byte) 0xff,
                (byte) 0xff, 0x7f };

            // Decode to a field element
            FieldElement one = FieldElement.FromByteArray(oneEncodedWronglyBytes);

            // .. then check that the encoding is correct
            byte[] oneBytes = one.ToByteArray();
            Assert.AreEqual((byte)1, oneBytes[0]);
            for (int i = 1; i < 32; i++)
            {
                Assert.AreEqual((byte)0, oneBytes[i]);
            }
        }

        [TestMethod]
        public void encodeAndDecodeOnZero()
        {
            byte[] zero = new byte[32];
            FieldElement a = FieldElement.FromByteArray(zero);

            Assert.AreEqual(FieldElement.ZERO, a);
            CollectionAssert.AreEqual(zero, a.ToByteArray());
        }

        [TestMethod]
        public void ctSelectReturnsCorrectResult()
        {
            int[] a_t = new int[10];
            int[] b_t = new int[10];
            for (int i = 0; i < 10; i++)
            {
                a_t[i] = i;
                b_t[i] = 10 - i;
            }

            FieldElement a = new FieldElement(a_t);
            FieldElement b = new FieldElement(b_t);

            Assert.AreEqual(a, a.CtSelect(b, 0));
            Assert.AreEqual(b, a.CtSelect(b, 1));
            Assert.AreEqual(b, b.CtSelect(a, 0));
            Assert.AreEqual(a, b.CtSelect(a, 1));
        }
    }
}
