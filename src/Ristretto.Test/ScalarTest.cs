using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ristretto.Test
{
    [TestClass]
    public class ScalarTest
    {
        // Example from RFC 8032 test case 1
        public static readonly byte[] TV1_R_INPUT = StrUtils.HexToBytes("b6b19cd8e0426f5983fa112d89a143aa97dab8bc5deb8d5b6253c928b65272f4044098c2a990039cde5b6a4818df0bfb6e40dc5dee54248032962323e701352d");
        public static readonly byte[] TV1_R = StrUtils.HexToBytes("f38907308c893deaf244787db4af53682249107418afc2edc58f75ac58a07404");
        public static readonly byte[] TV1_H = StrUtils.HexToBytes("86eabc8e4c96193d290504e7c600df6cf8d8256131ec2c138a3e7e162e525404");
        public static readonly byte[] TV1_A = StrUtils.HexToBytes("307c83864f2833cb427a2ef1c00a013cfdff2768d980c0a3a520f006904de94f");
        public static readonly byte[] TV1_S = StrUtils.HexToBytes("5fb8821590a33bacc61e39701cf9b46bd25bf5f0595bbe24655141438e7a100b");


        /// <summary>
        /// x = 2238329342913194256032495932344128051776374960164957527413114840482143558222
        /// </summary>
        static readonly Scalar X = new Scalar(StrUtils.HexToBytes("4e5ab4345d4708845913b4641bc27d5252a585101bcc4244d449f4a879d9f204"));

        /// <summary>
        ///  1/x = 6859937278830797291664592131120606308688036382723378951768035303146619657244
        /// </summary>
        static readonly Scalar XINV = new Scalar(StrUtils.HexToBytes("1cdc17fce0e9a5bbd9247e56bb016347bbba31edd5a9bb96d50bcd7a3f962a0f"));

        /// <summary>
        /// y = 2592331292931086675770238855846338635550719849568364935475441891787804997264
        /// </summary>
        static readonly Scalar Y = new Scalar(StrUtils.HexToBytes("907633fe1c4b66a4a28d2dd7678386c353d0de5455d4fc9de8ef7ac31f35bb05"));

        /// <summary>
        /// x*y = 5690045403673944803228348699031245560686958845067437804563560795922180092780
        /// </summary>
        static readonly Scalar X_TIMES_Y = new Scalar(StrUtils.HexToBytes("6c3374a1894f62210aaa2fe186a6f92ce0aa75c2779581c295fc08179a73940c"));

        /// <summary>
        ///  sage: l = 2^252 + 27742317777372353535851937790883648493 
        ///  sage: big = 2^256 - 1 
        ///  sage: repr((big % l).digits(256))
        /// </summary>
        static readonly Scalar CANONICAL_2_256_MINUS_1 = new Scalar(StrUtils.HexToBytes("1c95988d7431ecd670cf7d73f45befc6feffffffffffffffffffffffffffff0f"));

        static readonly Scalar A_SCALAR = new Scalar(StrUtils.HexToBytes("1a0e978a90f6622d3747023f8ad8264da758aa1b88e040d1589e7b7f2376ef09"));

        static readonly sbyte[] A_NAF = new sbyte[] { 0, 13, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, -9, 0, 0, 0, 0, -11, 0, 0,
            0, 0, 3, 0, 0, 0, 0, 1, 0, 0, 0, 0, 9, 0, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 11, 0, 0, 0, 0, 11,
            0, 0, 0, 0, 0, -9, 0, 0, 0, 0, 0, -3, 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0,
            9, 0, 0, 0, 0, -15, 0, 0, 0, 0, -7, 0, 0, 0, 0, -9, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, -3, 0,
            0, 0, 0, -11, 0, 0, 0, 0, -7, 0, 0, 0, 0, -13, 0, 0, 0, 0, 11, 0, 0, 0, 0, -9, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
            0, -15, 0, 0, 0, 0, 1, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 0, 11, 0,
            0, 0, 0, 0, 15, 0, 0, 0, 0, 0, -9, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, -15, 0,
            0, 0, 0, 0, 15, 0, 0, 0, 0, 15, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 };


        [TestMethod]
        public void packageConstructorDoesNotThrowOnValid()
        {
            byte[] s = new byte[32];
            s[31] = 0x7f;
            new Scalar(s);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void packageConstructorThrowsOnHighBitSet()
        {
            byte[] s = new byte[32];
            s[31] = (byte)0x80;
            new Scalar(s);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void packageConstructorThrowsOnTooShort()
        {
            new Scalar(new byte[31]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void packageConstructorThrowsOnTooLong()
        {
            new Scalar(new byte[33]);
        }

        [TestMethod]
        public void packageConstructorPreventsMutability()
        {
            // Create byte array representing a zero scalar
            byte[] bytes = new byte[32];

            // Create a scalar from bytes
            Scalar s = new Scalar(bytes);
            Assert.AreEqual(Scalar.ZERO, s);

            // Modify the bytes
            bytes[0] = 1;

            // The scalar should be unaltered
            Assert.AreEqual(Scalar.ZERO, s);
        }

        [TestMethod]
        public void toByteArrayPreventsMutability()
        {
            // Create a zero scalar
            Scalar s = new Scalar(new byte[32]);
            Assert.AreEqual(Scalar.ZERO, s);

            // Grab the scalar as bytes
            byte[] bytes = s.ToByteArray();

            // Modify the bytes
            bytes[0] = 1;

            // The scalar should be unaltered
            Assert.AreEqual(Scalar.ZERO, s);
        }

        [TestMethod]
        public void Reduce()
        {
            Scalar biggest = Scalar.FromBytesModOrder(StrUtils.HexToBytes("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            Assert.AreEqual(CANONICAL_2_256_MINUS_1, biggest);
        }

        [TestMethod]
        public void ReduceWide()
        {
            Scalar biggest = Scalar.FromBytesModOrderWide(StrUtils.HexToBytes(
                    "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff0000000000000000000000000000000000000000000000000000000000000000"));
            Assert.AreEqual(CANONICAL_2_256_MINUS_1, biggest);
        }

        [TestMethod]
        public void canonicalDecoding()
        {
            // Canonical encoding of 1667457891
            byte[] canonicalBytes = StrUtils.HexToBytes("6363636300000000000000000000000000000000000000000000000000000000");

            Scalar.FromCanonicalBytes(canonicalBytes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void nonCanonicalDecodingUnreduced()
        {
            // Encoding of
            // 7265385991361016183439748078976496179028704920197054998554201349516117938192
            // = 28380414028753969466561515933501938171588560817147392552250411230663687203
            // (mod l)
            // Non-canonical because unreduced mod l
            byte[] nonCanonicalBytesBecauseUnreduced = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                nonCanonicalBytesBecauseUnreduced[i] = 16;
            }

            Scalar.FromCanonicalBytes(nonCanonicalBytesBecauseUnreduced);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void nonCanonicalDecodingHighbit()
        {
            // Encoding with high bit set, to check that the parser isn't pre-masking the
            // high bit
            byte[] nonCanonicalBytesBecauseHighbit = new byte[32];
            nonCanonicalBytesBecauseHighbit[31] = (byte)0x80;

            Scalar.FromCanonicalBytes(nonCanonicalBytesBecauseHighbit);
        }

        [TestMethod]
        public void fromBitsClearsHighbit()
        {
            Scalar exact = Scalar
                    .FromBits(StrUtils.HexToBytes("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            CollectionAssert.AreEqual(
                StrUtils.HexToBytes("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"),
                exact.ToByteArray());
        }

        [TestMethod]
        public void addDoesNotReduceNonCanonical()
        {
            Scalar largestEd25519Scalar = Scalar
                    .FromBits(StrUtils.HexToBytes("f8ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"));
            Scalar result = Scalar.FromCanonicalBytes(
                    StrUtils.HexToBytes("7e344775474a7f9723b63a8be92ae76dffffffffffffffffffffffffffffff0f"));
            Assert.AreNotEqual(largestEd25519Scalar.Add(Scalar.ONE), result);
            Assert.AreEqual(largestEd25519Scalar.Add(Scalar.ONE).Reduce(), result);
        }

        [TestMethod]
        public void subtractDoesNotReduceNonCanonical()
        {
            Scalar largestEd25519Scalar = Scalar
                    .FromBits(StrUtils.HexToBytes("f8ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff7f"));
            Scalar result = Scalar.FromCanonicalBytes(
                    StrUtils.HexToBytes("7c344775474a7f9723b63a8be92ae76dffffffffffffffffffffffffffffff0f"));
            Assert.AreNotEqual(largestEd25519Scalar.Subtract(Scalar.ONE), result);
            Assert.AreEqual(largestEd25519Scalar.Subtract(Scalar.ONE).Reduce(), result);
        }

        [TestMethod]
        public void multiply()
        {
            Assert.AreEqual(X_TIMES_Y, X.Multiply(Y));
            Assert.AreEqual(Y, X_TIMES_Y.Multiply(XINV));
        }

        [TestMethod]
        public void nonAdjacentForm()
        {
            sbyte[] naf = A_SCALAR.nonAdjacentForm();
            CollectionAssert.AreEqual(A_NAF, naf);
        }


        [TestMethod]
        public void testVectorFromBytesModOrderWide()
        {
            Assert.AreEqual(new Scalar(TV1_R), Scalar.FromBytesModOrderWide(TV1_R_INPUT));
        }

        [TestMethod]
        public void testVectorMultiplyAndAdd()
        {
            Scalar h = new Scalar(TV1_H);
            Scalar a = new Scalar(TV1_A);
            Scalar r = new Scalar(TV1_R);
            Scalar S = new Scalar(TV1_S);
            Assert.AreEqual(S, h.MultiplyAndAdd(a, r));
            Assert.AreEqual(S, h.Multiply(a).Add(r));
            Assert.AreEqual(S.Subtract(r), h.Multiply(a));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void fromBytesModOrderWideThrowsOnTooShort()
        {
            Scalar.FromBytesModOrderWide(new byte[63]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void fromBytesModOrderWideThrowsOnTooLong()
        {
            Scalar.FromBytesModOrderWide(new byte[65]);
        }

        static readonly Scalar FORTYTWO = new Scalar(
                StrUtils.HexToBytes("2A00000000000000000000000000000000000000000000000000000000000000"));
        static readonly Scalar S1234567890 = new Scalar(
                StrUtils.HexToBytes("D202964900000000000000000000000000000000000000000000000000000000"));
        
        static readonly sbyte[] RADIX16_ZERO = StrUtils.HexToSBytes(
                "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        
        static readonly sbyte[] RADIX16_ONE = StrUtils.HexToSBytes(
                "01000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
        
        static readonly sbyte[] RADIX16_42 = StrUtils.HexToSBytes(
                "FA030000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");

        /**
         * Test method for {@link GroupElement#toRadix16(byte[])}.
         */
        [TestMethod]
        public void testToRadix16()
        {
            sbyte[] zero = Scalar.ZERO.ToRadix16;
            for (int i = 0; i < zero.Length; i++ )
            {
                Assert.IsTrue(zero[i] == RADIX16_ZERO[i]);
            }
            CollectionAssert.AreEqual(RADIX16_ZERO, Scalar.ZERO.ToRadix16);
            CollectionAssert.AreEqual(RADIX16_ONE, Scalar.ONE.ToRadix16);
            CollectionAssert.AreEqual(RADIX16_42, FORTYTWO.ToRadix16);

            sbyte[] from1234567890 = S1234567890.ToRadix16;
            double total = 0;
            for (int i = 0; i < from1234567890.Length; i++)
            {
                Assert.IsTrue(from1234567890[i]>=(sbyte)-8);
                Assert.IsTrue(from1234567890[i]<=(sbyte)7);
                total += from1234567890[i] * Math.Pow(16, i);
            }
            Assert.AreEqual(1234567890, total);

            sbyte[] tv1HR16 = (new Scalar(TV1_H)).ToRadix16;
            for (int i = 0; i < tv1HR16.Length; i++)
            {
                Assert.IsTrue(tv1HR16[i]>=(sbyte)-8);
                Assert.IsTrue(tv1HR16[i]<=(sbyte)7);
            }
        }
    }
}
