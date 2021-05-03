namespace Ristretto
{
    /// <summary>
    /// A point ((X:Z), (Y:T)) on the  P^1 \times  P^1 model of the curve.
    /// </summary>
    public class CompletedPoint
    {
        readonly FieldElement X;
        readonly FieldElement Y;
        readonly FieldElement Z;
        readonly FieldElement T;

        public CompletedPoint(FieldElement X, FieldElement Y, FieldElement Z, FieldElement T)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.T = T;
        }

        /// <summary>
        /// Convert this point from the  P^1 \times  P^1 model to the  P^2 model.
        /// </summary>
        /// <returns>This costs 3 M.</returns>
        public ProjectivePoint ToProjective()
        {
            return new ProjectivePoint(this.X.Multiply(this.T), Y.Multiply(this.Z), this.Z.Multiply(this.T));
        }

        /// <summary>
        /// Convert this point from the  P^1 * P^1 model to the P^3 model.
        /// This costs 4  M.
        /// </summary>
        /// <returns></returns>
        public EdwardsPoint ToExtended()
        {
            return new EdwardsPoint(
                this.X.Multiply(this.T), 
                    Y.Multiply(this.Z), 
                    this.Z.Multiply(this.T),
                        this.X.Multiply(this.Y));
        }
    }

}
