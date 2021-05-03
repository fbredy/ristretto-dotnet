namespace Ristretto
{
    /// <summary>
    /// A point (X:Y:Z) on the  P^2 model of the curve.
    /// </summary>
    public class ProjectivePoint
    {
        readonly FieldElement X;
        readonly FieldElement Y;
        readonly FieldElement Z;

        public ProjectivePoint(FieldElement X, FieldElement Y, FieldElement Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
                
        /// <summary>
        /// Convert this point from the  P^2 model to the  P^3 model.
        /// This costs 3  M + 1 S.
        /// </summary>
        /// <returns></returns>
        public EdwardsPoint ToExtended()
        {
            return new EdwardsPoint(this.X.Multiply(this.Z), Y.Multiply(this.Z), this.Z.Square(), this.X.Multiply(this.Y));
        }

        /// <summary>
        /// Point doubling: add this point to itself.
        /// </summary>
        /// <returns>[2]P as a CompletedPoint.</returns>
        public CompletedPoint Doubling()
        {
            FieldElement XX = this.X.Square();
            FieldElement YY = this.Y.Square();
            FieldElement ZZ2 = this.Z.SquareAndDouble();
            FieldElement XPlusY = this.X.Add(this.Y);
            FieldElement XPlusYSq = XPlusY.Square();
            FieldElement YYPlusXX = YY.Add(XX);
            FieldElement YYMinusXX = YY.Subtract(XX);
            return new CompletedPoint(XPlusYSq.Subtract(YYPlusXX), YYPlusXX, YYMinusXX, ZZ2.Subtract(YYMinusXX));
        }
    }

}
