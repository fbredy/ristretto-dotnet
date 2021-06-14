using System;

namespace Ristretto
{
    /// <summary>
    /// An Edwards point encoded in "Edwards y" / "Ed25519" format.
    /// In "Edwards y" / "Ed25519" format, the curve point (x, y) is determined by
    /// the y-coordinate and the sign of x.
    /// The first 255 bits of a CompressedEdwardsY represent the y-coordinate.The
    /// high bit of the 32nd byte represents the sign of x.
    /// </summary>
    [Serializable]
    public class CompressedEdwardsY
    {
        /// <summary>
        /// The encoded point.
        /// </summary>
        [NonSerialized]
        private byte[] data;

        public CompressedEdwardsY(byte[] data)
        {
            if (data.Length != 32)
            {
                throw new ArgumentException("Invalid CompressedEdwardsY encoding");
            }
            this.data = data;
        }

        /**
         * Overrides class serialization to use the canonical encoded format.
         */
        //private void writeObject(ObjectOutputStream output)
        //{
        //    output.write(this.toByteArray());
        //}

        /**
         * Overrides class serialization to use the canonical encoded format.
         */
        //private void readObject(ObjectInputStream input)
        //{
        //    byte[] encoded = new byte[32];
        //    input.readFully(encoded);
        //    this.data = encoded;
        //}

        //@SuppressWarnings("unused")
        //private void readObjectNoData()
        //{
        //    throw new InvalidOperationException("Cannot deserialize CompressedEdwardsY from no data");
        //}

        /// <summary>
        /// Attempts to decompress to an EdwardsPoint.
        /// </summary>
        /// <returns>an EdwardsPoint, if this is a valid encoding.</returns>
        public EdwardsPoint Decompress()
        {
            FieldElement Y = FieldElement.FromByteArray(data);
            FieldElement YY = Y.Square();

            // u = y²-1
            FieldElement u = YY.Subtract(FieldElement.ONE);

            // v = dy²+1
            FieldElement v = YY.Multiply(Constants.EDWARDS_D).Add(FieldElement.ONE);

            FieldElement.SqrtRatioM1Result sqrt = FieldElement.SqrtRatioM1(u, v);
            if (sqrt.WasSquare != 1)
            {
                throw new InvalidEncodingException("not a valid EdwardsPoint");
            }

            FieldElement X = sqrt.Result.Negate().CtSelect(sqrt.Result,
                    ConstantTime.Equal(sqrt.Result.IsNegative(), ConstantTime.Bit(data, 255)));

            return new EdwardsPoint(X, Y, FieldElement.ONE, X.Multiply(Y));
        }

        /// <summary>
        /// Encode the point to its compressed 32-byte form.
        /// </summary>
        /// <returns>the encoded point.</returns>
        public byte[] ToByteArray()
        {
            return data;
        }

        /// <summary>
        /// Equality check overridden to be constant-time.
        /// Fails fast if the objects are of different types.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>true if this and other are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is CompressedEdwardsY))
            {
                return false;
            }

            CompressedEdwardsY other = (CompressedEdwardsY)obj;
            return ConstantTime.Equals(this.data, other.data);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }
    }
}
