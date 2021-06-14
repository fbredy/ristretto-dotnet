using System;

namespace Ristretto
{
    public class UnpackedScalar
    {
        public static readonly UnpackedScalar ZERO = new UnpackedScalar(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        public int[] s { get; }

        public UnpackedScalar(int[] s)
        {
            if (s.Length != 9)
            {
                throw new ArgumentException("Invalid radix-2^29 representation");
            }
            this.s = s;
        }

        static readonly int MASK_29_BITS = (1 << 29) - 1;

        static readonly int MASK_24_BITS = (1 << 24) - 1;

        /// <summary>
        /// Unpack a 32 byte / 256 bit scalar into 9 29-bit limbs.
        /// </summary>
        /// <param name="input">input byte array</param>
        /// <returns></returns>
        public static UnpackedScalar FromByteArray(byte[] input)
        {
            if (input.Length != 32)
            {
                throw new ArgumentException("Input must by 32 bytes");
            }

            int[] words = new int[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    words[i] |= ((input[(i * 4) + j]) & 0xff) << (j * 8);
                }
            }

            int[] s = new int[9];

            s[0] = (words[0] & MASK_29_BITS);
            s[1] = (((int)((uint)words[0] >> 29) | (words[1] << 3)) & MASK_29_BITS);
            s[2] = (((int)((uint)words[1] >> 26) | (words[2] << 6)) & MASK_29_BITS);
            s[3] = (((int)((uint)words[2] >> 23) | (words[3] << 9)) & MASK_29_BITS);
            s[4] = (((int)((uint)words[3] >> 20) | (words[4] << 12)) & MASK_29_BITS);
            s[5] = (((int)((uint)words[4] >> 17) | (words[5] << 15)) & MASK_29_BITS);
            s[6] = (((int)((uint)words[5] >> 14) | (words[6] << 18)) & MASK_29_BITS);
            s[7] = (((int)((uint)words[6] >> 11) | (words[7] << 21)) & MASK_29_BITS);
            s[8] = ((int)((uint)words[7] >> 8) & MASK_24_BITS);

            return new UnpackedScalar(s);
        }

        /// <summary>
        /// Pack the limbs of this UnpackedScalar into 32 bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            byte[] result = new byte[32];

            // All limbs are 29 bits, but let's use the unsigned right shift anyway.
            result[0] = (byte)((uint)this.s[0] >> 0);
            result[1] = (byte)((uint)this.s[0] >> 8);
            result[2] = (byte)((uint)this.s[0] >> 16);
            result[3] = (byte)((int)((uint)this.s[0] >> 24) | (this.s[1] << 5));
            result[4] = (byte)((uint)this.s[1] >> 3);
            result[5] = (byte)((uint)this.s[1] >> 11);
            result[6] = (byte)((uint)this.s[1] >> 19);
            result[7] = (byte)((int)((uint)this.s[1] >> 27) | (this.s[2] << 2));
            result[8] = (byte)((uint)this.s[2] >> 6);
            result[9] = (byte)((uint)this.s[2] >> 14);
            result[10] = (byte)((int)((uint)this.s[2] >> 22) | (this.s[3] << 7));
            result[11] = (byte)((uint)this.s[3] >> 1);
            result[12] = (byte)((uint)this.s[3] >> 9);
            result[13] = (byte)((uint)this.s[3] >> 17);
            result[14] = (byte)((int)((uint)this.s[3] >> 25) | (this.s[4] << 4));
            result[15] = (byte)((uint)this.s[4] >> 4);
            result[16] = (byte)((uint)this.s[4] >> 12);
            result[17] = (byte)((uint)this.s[4] >> 20);
            result[18] = (byte)((int)((uint)this.s[4] >> 28) | (this.s[5] << 1));
            result[19] = (byte)((uint)this.s[5] >> 7);
            result[20] = (byte)((uint)this.s[5] >> 15);
            result[21] = (byte)((int)((uint)this.s[5] >> 23) | (this.s[6] << 6));
            result[22] = (byte)((uint)this.s[6] >> 2);
            result[23] = (byte)((uint)this.s[6] >> 10);
            result[24] = (byte)((uint)this.s[6] >> 18);
            result[25] = (byte)((int)((uint)this.s[6] >> 26) | (this.s[7] << 3));
            result[26] = (byte)((uint)this.s[7] >> 5);
            result[27] = (byte)((uint)this.s[7] >> 13);
            result[28] = (byte)((uint)this.s[7] >> 21);
            result[29] = (byte)((uint)this.s[8] >> 0);
            result[30] = (byte)((uint)this.s[8] >> 8);
            result[31] = (byte)((uint)this.s[8] >> 16);

            return result;
        }

        /// <summary>
        /// Compute a + b mod ℓ
        /// </summary>
        /// <param name="b">the Scalar to add to this.</param>
        /// <returns>a + b mod ℓ</returns>
        public UnpackedScalar Add(UnpackedScalar b)
        {
            int[] sum = new int[9];

            int carry = 0;
            for (int i = 0; i < 9; i++)
            {
                carry = this.s[i] + b.s[i] + (carry >> 29);
                sum[i] = carry & MASK_29_BITS;
            }

            // Subtract l if the sum is >= l
            return new UnpackedScalar(sum).Subtract(Constants.L);
        }

        /// <summary>
        /// Compute a - b mod ℓ.
        /// </summary>
        /// <param name="b">the Scalar to subtract from this.</param>
        /// <returns>a - b mod ℓ</returns>
        public UnpackedScalar Subtract(UnpackedScalar b)
        {
            int[] difference = new int[9];

            int borrow = 0;
            for (int i = 0; i < 9; i++)
            {
                borrow = this.s[i] - (b.s[i] + (int)((uint)borrow >> 31));
                difference[i] = borrow & MASK_29_BITS;
            }

            // Conditionally add l if the difference is negative
            int underflowMask = ((int)((uint)borrow >> 31) ^ 1) - 1;
            int carry = 0;
            for (int i = 0; i < 9; i++)
            {
                carry = (int)(((uint)carry) >> 29) + difference[i] + (Constants.L.s[i] & underflowMask);
                difference[i] = carry & MASK_29_BITS;
            }

            return new UnpackedScalar(difference);
        }

        private static long Multiply(int a, int b)
        {
            return ((long)a) * ((long)b);
        }

        /// <summary>
        /// Compute a * b mod ℓ.
        /// </summary>
        /// <param name="val">the Scalar to multiply with this.</param>
        /// <returns>the unreduced limbs.</returns>
        public long[] MultiplyInternal(UnpackedScalar val)
        {
            int[] a = this.s;
            int[] b = val.s;
            long[] z = new long[17];
                        
            z[0] = Multiply(a[0], b[0]);                                                             // c00
            z[1] = Multiply(a[0], b[1]) + Multiply(a[1], b[0]);                                              // c01
            z[2] = Multiply(a[0], b[2]) + Multiply(a[1], b[1]) + Multiply(a[2], b[0]);                               // c02
            z[3] = Multiply(a[0], b[3]) + Multiply(a[1], b[2]) + Multiply(a[2], b[1]) + Multiply(a[3], b[0]);                // c03
            z[4] = Multiply(a[0], b[4]) + Multiply(a[1], b[3]) + Multiply(a[2], b[2]) + Multiply(a[3], b[1]) + Multiply(a[4], b[0]); // c04
            z[5] = Multiply(a[1], b[4]) + Multiply(a[2], b[3]) + Multiply(a[3], b[2]) + Multiply(a[4], b[1]); // c05
            z[6] = Multiply(a[2], b[4]) + Multiply(a[3], b[3]) + Multiply(a[4], b[2]); // c06
            z[7] = Multiply(a[3], b[4]) + Multiply(a[4], b[3]); // c07
            z[8] = (Multiply(a[4], b[4])) - z[3]; // c08 - c03

            z[10] = z[5] - (Multiply(a[5], b[5]));                                             // c05mc10
            z[11] = z[6] - (Multiply(a[5], b[6]) + Multiply(a[6], b[5]));                              // c06mc11
            z[12] = z[7] - (Multiply(a[5], b[7]) + Multiply(a[6], b[6]) + Multiply(a[7], b[5]));               // c07mc12
            z[13] = Multiply(a[5], b[8]) + Multiply(a[6], b[7]) + Multiply(a[7], b[6]) + Multiply(a[8], b[5]); // c13
            z[14] = Multiply(a[6], b[8]) + Multiply(a[7], b[7]) + Multiply(a[8], b[6]); // c14
            z[15] = Multiply(a[7], b[8]) + Multiply(a[8], b[7]); // c15
            z[16] = Multiply(a[8], b[8]); // c16

            z[5] = z[10] - (z[0]); // c05mc10 - c00
            z[6] = z[11] - (z[1]); // c06mc11 - c01
            z[7] = z[12] - (z[2]); // c07mc12 - c02
            z[8] = z[8] - (z[13]); // c08mc13 - c03
            z[9] = z[14] + (z[4]); // c14 + c04
            z[10] = z[15] + (z[10]); // c15 + c05mc10
            z[11] = z[16] + (z[11]); // c16 + c06mc11

            int aa0 = a[0] + a[5];
            int aa1 = a[1] + a[6];
            int aa2 = a[2] + a[7];
            int aa3 = a[3] + a[8];

            int bb0 = b[0] + b[5];
            int bb1 = b[1] + b[6];
            int bb2 = b[2] + b[7];
            int bb3 = b[3] + b[8];

            z[5] = (Multiply(aa0, bb0)) + z[5]; // c20 + c05mc10 - c00
            z[6] = (Multiply(aa0, bb1) + Multiply(aa1, bb0)) + z[6]; // c21 + c06mc11 - c01
            z[7] = (Multiply(aa0, bb2) + Multiply(aa1, bb1) + Multiply(aa2, bb0)) + z[7]; // c22 + c07mc12 - c02
            z[8] = (Multiply(aa0, bb3) + Multiply(aa1, bb2) + Multiply(aa2, bb1) + Multiply(aa3, bb0)) + z[8]; // c23 + c08mc13 - c03
            z[9] = (Multiply(aa0, b[4]) + Multiply(aa1, bb3) + Multiply(aa2, bb2) + Multiply(aa3, bb1) + Multiply(a[4], bb0)) - z[9]; // c24 - c14 - c04
            z[10] = (Multiply(aa1, b[4]) + Multiply(aa2, bb3) + Multiply(aa3, bb2) + Multiply(a[4], bb1)) - z[10]; // c25 - c15 - c05mc10
            z[11] = (Multiply(aa2, b[4]) + Multiply(aa3, bb3) + Multiply(a[4], bb2)) - z[11]; // c26 - c16 - c06mc11
            z[12] = (Multiply(aa3, b[4]) + Multiply(a[4], bb3)) - z[12]; // c27 - c07mc12
                                                           
            return z;
        }

        /// <summary>
        /// Compute limbs/R mod ℓ, where R is the Montgomery modulus 2^261.
        /// </summary>
        /// <param name="limbs">the value to reduce.</param>
        /// <returns>limbs/R mod ℓ</returns>
        public static UnpackedScalar MontgomeryReduce(long[] limbs)
        {
            // Note: l5,l6,l7 are zero, so their multiplies can be skipped
            int[] l = Constants.L.s;
            long sum, carry;
            int n0, n1, n2, n3, n4, n5, n6, n7, n8;
            int[] r = new int[9];

            // The first half computes the Montgomery adjustment factor n, and begins adding
            // n*l to make limbs divisible by R
            
            sum = (limbs[0]);
            n0 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n0, l[0])) >> 29);
            sum = (carry + limbs[1] + Multiply(n0, l[1]));
            n1 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n1, l[0])) >> 29);
            sum = (carry + limbs[2] + Multiply(n0, l[2]) + Multiply(n1, l[1]));
            n2 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n2, l[0])) >> 29);
            sum = (carry + limbs[3] + Multiply(n0, l[3]) + Multiply(n1, l[2]) + Multiply(n2, l[1]));
            n3 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n3, l[0])) >> 29);
            sum = (carry + limbs[4] + Multiply(n0, l[4]) + Multiply(n1, l[3]) + Multiply(n2, l[2]) + Multiply(n3, l[1]));
            n4 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n4, l[0])) >> 29);
            sum = (carry + limbs[5] + Multiply(n1, l[4]) + Multiply(n2, l[3]) + Multiply(n3, l[2]) + Multiply(n4, l[1]));
            n5 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n5, l[0])) >> 29);
            sum = (carry + limbs[6] + Multiply(n2, l[4]) + Multiply(n3, l[3]) + Multiply(n4, l[2]) + Multiply(n5, l[1]));
            n6 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n6, l[0])) >> 29);
            sum = (carry + limbs[7] + Multiply(n3, l[4]) + Multiply(n4, l[3]) + Multiply(n5, l[2]) + Multiply(n6, l[1]));
            n7 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n7, l[0])) >> 29);
            sum = (carry + limbs[8] + Multiply(n0, l[8]) + Multiply(n4, l[4]) + Multiply(n5, l[3]) + Multiply(n6, l[2]) + Multiply(n7, l[1]));
            n8 = (int)(((sum & 0xffffffff) * Constants.LFACTOR) & MASK_29_BITS);
            carry = (long)((ulong)(sum + Multiply(n8, l[0])) >> 29);

            // limbs is divisible by R now, so we can divide by R by simply storing the upper half as the result
            sum = (carry + limbs[9] + Multiply(n1, l[8]) + Multiply(n5, l[4]) + Multiply(n6, l[3]) + Multiply(n7, l[2]) + Multiply(n8, l[1]));
            r[0] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[10] + Multiply(n2, l[8]) + Multiply(n6, l[4]) + Multiply(n7, l[3]) + Multiply(n8, l[2]));
            r[1] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[11] + Multiply(n3, l[8]) + Multiply(n7, l[4]) + Multiply(n8, l[3]));
            r[2] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[12] + Multiply(n4, l[8]) + Multiply(n8, l[4]));
            r[3] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[13] + Multiply(n5, l[8]));
            r[4] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[14] + Multiply(n6, l[8]));
            r[5] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[15] + Multiply(n7, l[8]));
            r[6] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            sum = (carry + limbs[16] + Multiply(n8, l[8]));
            r[7] = (int)(sum & MASK_29_BITS);
            carry = (long)((ulong)sum >> 29);
            r[8] = (int)(carry & 0xffffffff);
            
            // Result may be >= l, so attempt to subtract l
            return new UnpackedScalar(r).Subtract(Constants.L);
        }

        /// <summary>
        /// Compute a * b mod ℓ.
        /// </summary>
        /// <param name="b">the Scalar to multiply with this.</param>
        /// <returns>a * b mod ℓ</returns>
        public UnpackedScalar Multiply(UnpackedScalar b)
        {
            UnpackedScalar ab = UnpackedScalar.MontgomeryReduce(this.MultiplyInternal(b));
            return UnpackedScalar.MontgomeryReduce(ab.MultiplyInternal(Constants.RR));
        }
    }
}
