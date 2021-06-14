using System;
using System.Linq;

namespace Ristretto
{
    [Serializable]
    public class RistrettoElement
    {
        public static readonly RistrettoElement IDENTITY = new RistrettoElement(EdwardsPoint.IDENTITY);

        /// <summary>
        /// The internal representation. Not canonical.
        /// </summary>
        public EdwardsPoint Representation { get; private set; }

        /// <summary>
        /// Only for internal use.
        /// </summary>
        /// <param name="repr">EdwardsPoint</param>
        public RistrettoElement(EdwardsPoint representation)
        {
            this.Representation = representation;
        }

        
        /// <summary>
        /// The function MAP(t) from section 3.2.4 of the ristretto255 ID.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static RistrettoElement Map(FieldElement t)
        {
            FieldElement r = t.Square().Multiply(Constants.SQRT_M1);
            FieldElement u = r.Add(FieldElement.ONE).Multiply(Constants.ONE_MINUS_D_SQ);
            FieldElement v = FieldElement.MINUS_ONE.Subtract(r.Multiply(Constants.EDWARDS_D))
              .Multiply(r.Add(Constants.EDWARDS_D));

            FieldElement.SqrtRatioM1Result sqrt = FieldElement.SqrtRatioM1(u, v);
            FieldElement s = sqrt.Result;

            FieldElement sPrime = s.Multiply(t).CtAbs().Negate();
            s = sPrime.CtSelect(s, sqrt.WasSquare);
            FieldElement c = r.CtSelect(FieldElement.MINUS_ONE, sqrt.WasSquare);

            FieldElement N = c.Multiply(r.Subtract(FieldElement.ONE)).Multiply(Constants.D_MINUS_ONE_SQ).Subtract(v);
            FieldElement sSq = s.Square();

            FieldElement w0 = s.Add(s).Multiply(v);
            FieldElement w1 = N.Multiply(Constants.SQRT_AD_MINUS_ONE);
            FieldElement w2 = FieldElement.ONE.Subtract(sSq);
            FieldElement w3 = FieldElement.ONE.Add(sSq);

            return new RistrettoElement(
                    new EdwardsPoint(w0.Multiply(w3), w2.Multiply(w1), w1.Multiply(w3), w0.Multiply(w2)));
        }

        /// <summary>
        /// Construct a ristretto255 element from a uniformly-distributed 64-byte string.
        /// This is the ristretto255 FROM_UNIFORM_BYTES function.
        /// </summary>
        /// <param name="b">data</param>
        /// <returns>the resulting element.</returns>
        public static RistrettoElement FromUniformBytes(byte[] b)
        {
            // 1. Interpret the least significant 255 bits of b[ 0..32] as an
            // integer r0 in little-endian representation. Reduce r0 modulo p.
            byte[] b0 = b.Take(32).ToArray(); //Array.CopyOfRange(b, 0, 32);
            FieldElement r0 = FieldElement.FromByteArray(b0);

            // 2. Interpret the least significant 255 bits of b[32..64] as an
            // integer r1 in little-endian representation. Reduce r1 modulo p.
            byte[] b1 = b.Skip(32).Take(32).ToArray(); // Arrays.copyOfRange(b, 32, 64);
            FieldElement r1 = FieldElement.FromByteArray(b1);

            // 3. Compute group element P1 as MAP(r0)
            RistrettoElement P1 = RistrettoElement.Map(r0);

            // 4. Compute group element P2 as MAP(r1).
            RistrettoElement P2 = RistrettoElement.Map(r1);

            // 5. Return the group element P1 + P2.
            return P1.Add(P2);
        }

        /// <summary>
        /// Compress this element using the Ristretto encoding.
        /// This is the ristretto255 ENCODE function.
        /// </summary>
        /// <returns>the encoded element.</returns>
        public CompressedRistretto Compress()
        {
            // 1. Process the internal representation into a field element s as follows:
            FieldElement u1 = this.Representation.Z.Add(this.Representation.Y).Multiply(this.Representation.Z.Subtract(this.Representation.Y));
            FieldElement u2 = this.Representation.X.Multiply(this.Representation.Y);

            // Ignore was_square since this is always square
            FieldElement.SqrtRatioM1Result invsqrt = FieldElement.SqrtRatioM1(FieldElement.ONE,
                    u1.Multiply(u2.Square()));

            FieldElement den1 = invsqrt.Result.Multiply(u1);
            FieldElement den2 = invsqrt.Result.Multiply(u2);
            FieldElement zInv = den1.Multiply(den2).Multiply(this.Representation.T);

            FieldElement ix = this.Representation.X.Multiply(Constants.SQRT_M1);
            FieldElement iy = this.Representation.Y.Multiply(Constants.SQRT_M1);
            FieldElement enchantedDenominator = den1.Multiply(Constants.INVSQRT_A_MINUS_D);

            int rotate = this.Representation.T.Multiply(zInv).IsNegative();

            FieldElement x = this.Representation.X.CtSelect(iy, rotate);
            FieldElement y = this.Representation.Y.CtSelect(ix, rotate);
            FieldElement z = this.Representation.Z;
            FieldElement denInv = den2.CtSelect(enchantedDenominator, rotate);

            y = y.CtSelect(y.Negate(), x.Multiply(zInv).IsNegative());

            FieldElement s = denInv.Multiply(z.Subtract(y));
            int sIsNegative = s.IsNegative();
            s = s.CtSelect(s.Negate(), sIsNegative);

            // 2. Return the canonical little-endian encoding of s.
            return new CompressedRistretto(s.ToByteArray());
        }

        /// <summary>
        /// Constant-time selection between two RistrettoElements.
        /// </summary>
        /// <param name="that">that the other element.</param>
        /// <param name="b">must be 0 or 1, otherwise results are undefined.</param>
        /// <returns>a copy of this if b == 0, or a copy of that if b == 1.</returns>
        public RistrettoElement CtSelect(RistrettoElement that, int b)
        {
            return new RistrettoElement(this.Representation.CtSelect(that.Representation, b));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RistrettoElement))
            {
                return false;
            }

            RistrettoElement other = (RistrettoElement)obj;
            FieldElement X1Y2 = this.Representation.X.Multiply(other.Representation.Y);
            FieldElement Y1X2 = this.Representation.Y.Multiply(other.Representation.X);
            FieldElement Y1Y2 = this.Representation.Y.Multiply(other.Representation.Y);
            FieldElement X1X2 = this.Representation.X.Multiply(other.Representation.X);
            return (X1Y2.ctEquals(Y1X2) | Y1Y2.ctEquals(X1X2)) == 1;
        }

        public override int GetHashCode()
        {
            return Compress().GetHashCode();
        }


        /// <summary>
        /// Group addition.
        /// </summary>
        /// <param name="Q">the element to add</param>
        /// <returns>ristretto Element added</returns>
        public RistrettoElement Add(RistrettoElement Q)
        {
            return new RistrettoElement(this.Representation.Add(Q.Representation));
        }

        /// <summary>
        /// Group subtraction.
        /// </summary>
        /// <param name="Q">the element to subtract from this one.</param>
        /// <returns>P - Q</returns>
        public RistrettoElement Subtract(RistrettoElement Q)
        {
            return new RistrettoElement(this.Representation.Subtract(Q.Representation));
        }

        /// <summary>
        /// Negation
        /// </summary>
        /// <returns>-P</returns>
        public RistrettoElement Negate()
        {
            return new RistrettoElement(this.Representation.Negate());
        }

        /// <summary>
        /// Element Doubling
        /// </summary>
        /// <returns>2P</returns>
        public RistrettoElement Doubling()
        {
            return new RistrettoElement(this.Representation.Double());
        }

        /// <summary>
        /// Constant-time variable-base scalar multiplication.
        /// </summary>
        /// <param name="s">the Scalar to multiply by.</param>
        /// <returns>[s]P</returns>
        public RistrettoElement Multiply(Scalar s)
        {
            return new RistrettoElement(this.Representation.Multiply(s));
        }

        public override string ToString()
        {
            return "RistrettoElement(" + this.Representation + ")";
        }
    }
}
