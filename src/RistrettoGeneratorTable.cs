namespace Ristretto
{
    public class RistrettoGeneratorTable
    {
        private readonly EdwardsBasepointTable table;

        /// <summary>
        /// Create a table of pre-computed multiples of generator.
        /// </summary>
        /// <param name="generator"></param>
        public RistrettoGeneratorTable(RistrettoElement generator)
        {
            this.table = new EdwardsBasepointTable(generator.Representation);
        }

        /// <summary>
        /// Constant-time fixed-base scalar multiplication.
        /// </summary>
        /// <param name="s">the Scalar to multiply by.</param>
        /// <returns></returns>
        public RistrettoElement Multiply(Scalar s)
        {
            return new RistrettoElement(table.Multiply(s));
        }
    }

}
