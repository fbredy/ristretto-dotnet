namespace Ristretto
{
    /// <summary>
    /// A pre-computed table of multiples of a basepoint, for accelerating fixed-base scalar multiplication.
    /// </summary>
    public class EdwardsBasepointTable
    {
        private readonly AffineNielsPoint.LookupTable[] tables;

        /// <summary>
        /// Create a table of pre-computed multiples of basepoint.
        /// </summary>
        /// <param name="basepoint"></param>
        public EdwardsBasepointTable(EdwardsPoint basepoint)
        {
            this.tables = new AffineNielsPoint.LookupTable[32];
            EdwardsPoint Bi = basepoint;
            for (int i = 0; i < 32; i++)
            {
                this.tables[i] = AffineNielsPoint.BuildLookupTable(Bi);
                // Only every second summand is precomputed (16^2 = 256)
                Bi = Bi.MultiplyByPow2(8);
            }
        }

        /// <summary>
        /// Constant-time fixed-base scalar multiplication.
        /// </summary>
        /// <param name="s">the Scalar to multiply by.</param>
        /// <returns>[s]B</returns>
        public EdwardsPoint Multiply(Scalar s)
        {
            sbyte[] e = s.ToRadix16;
            EdwardsPoint h = EdwardsPoint.IDENTITY;

            for (int i = 1; i < 64; i += 2)
            {
                h = h.Add(this.tables[i / 2].Select(e[i])).ToExtended();
            }

            h = h.MultiplyByPow2(4);

            for (int i = 0; i < 64; i += 2)
            {
                h = h.Add(this.tables[i / 2].Select(e[i])).ToExtended();
            }

            return h;
        }
    }
}
