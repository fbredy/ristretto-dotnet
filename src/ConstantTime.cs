namespace Ristretto
{

    /// <summary>
    /// Constant-time functions.
    /// </summary>
    public sealed class ConstantTime
    {
        /// <summary>
        /// Constant-time byte comparison.
        /// </summary>
        /// <param name="b">a byte, represented as an int</param>
        /// <param name="c">a byte, represented as an int</param>
        /// <returns>1 if b and c are equal, 0 otherwise.</returns>
        public static int equal(int b, int c)
        {
            int result = 0;
            int xor = b ^ c;
            for (int i = 0; i < 8; i++)
            {
                result |= xor >> i;
            }
            return (result ^ 0x01) & 0x01;
        }

        /// <summary>
        /// Constant-time byte array comparison.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int Equal(byte[] b, byte[] c)
        {
            // Fail-fast if the lengths differ
            if (b.Length != c.Length)
            {
                return 0;
            }

            // Now use a constant-time comparison
            int result = 0;
            for (int i = 0; i < b.Length; i++)
            {
                result |= b[i] ^ c[i];
            }

            return equal(result, 0);
        }

        /// <summary>
        /// Constant-time byte[] comparison.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool Equals(byte[] b, byte[] c)
        {
            return Equal(b, c) == 1;
        }

        /// <summary>
        /// Constant-time determine if byte is negative.
        /// </summary>
        /// <param name="b">the byte to check, represented as an int.</param>
        /// <returns>1 if the byte is negative, 0 otherwise.</returns>
        public static int IsNegative(int b)
        {
            return (b >> 8) & 1;
        }

        /// <summary>
        /// Get the i'th bit of a byte array.
        /// </summary>
        /// <param name="h">the byte array.</param>
        /// <param name="i">the bit index.</param>
        /// <returns>0 or 1, the value of the i'th bit in h</returns>
        public static int bit(byte[] h, int i)
        {
            return (h[i >> 3] >> (i & 7)) & 1;
        }
    }

}
