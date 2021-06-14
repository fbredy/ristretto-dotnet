using System;

namespace Ristretto
{
    /// <summary>
    /// A Ristretto element in compressed wire format.
    /// The Ristretto encoding is canonical, so two elements are equal if and only if their encodings are equal.
    /// </summary>
    [Serializable]
    public class CompressedRistretto
    {
        /// <summary>
        /// The encoded element.
        /// </summary>
        [NonSerialized]
        private byte[] data;

        public CompressedRistretto(byte[] data)
        {
            if (data.Length != 32)
            {
                throw new ArgumentException("Invalid CompressedRistretto encoding");
            }
            this.data = data;
        }

        /// <summary>
        /// Attempts to decompress to a RistrettoElement.
        /// This is the ristretto255 DECODE function.
        /// </summary>
        /// <returns>a RistrettoElement, if this is the canonical encoding of an element of the ristretto255 group.</returns>
        public RistrettoElement Decompress()
        {
            // 1. First, interpret the string as an integer s in little-endian
            // representation. If the resulting value is >= p, decoding fails.
            // 2. If IS_NEGATIVE(s) returns TRUE, decoding fails.
            FieldElement s = FieldElement.FromByteArray(this.data);
            byte[] sBytes = s.ToByteArray();
            int sIsCanonical = ConstantTime.Equal(this.data, sBytes);
            if (sIsCanonical == 0 || s.IsNegatives())
            {
                throw new InvalidEncodingException("Invalid ristretto255 encoding");
            }

            // 3. Process s as follows:
            FieldElement ss = s.Square();
            FieldElement u1 = FieldElement.ONE.Subtract(ss);
            FieldElement u2 = FieldElement.ONE.Add(ss);
            FieldElement u2Sqr = u2.Square();

            FieldElement v = Constants.NEG_EDWARDS_D.Multiply(u1.Square()).Subtract(u2Sqr);

            FieldElement.SqrtRatioM1Result invsqrt = FieldElement.SqrtRatioM1(FieldElement.ONE, v.Multiply(u2Sqr));

            FieldElement denX = invsqrt.Result.Multiply(u2);
            FieldElement denY = invsqrt.Result.Multiply(denX).Multiply(v);

            FieldElement x = s.Add(s).Multiply(denX).CtAbs();
            FieldElement y = u1.Multiply(denY);
            FieldElement t = x.Multiply(y);

            // 4. If was_square is FALSE, or IS_NEGATIVE(t) returns TRUE, or y = 0, decoding
            // fails. Otherwise, return the internal representation in extended coordinates
            // (x, y, 1, t).
            if (invsqrt.WasSquare == 0 || t.IsNegatives() || y.IsZero())
            {
                throw new InvalidEncodingException("Invalid ristretto255 encoding");
            }
            else
            {
                return new RistrettoElement(new EdwardsPoint(x, y, FieldElement.ONE, t));
            }
        }

        /// <summary>
        /// Encode the element to its compressed 32-byte form.
        /// </summary>
        /// <returns>the encoded element.</returns>
        public byte[] ToByteArray()
        {
            return data;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CompressedRistretto))
            {
                return false;
            }

            CompressedRistretto other = (CompressedRistretto)obj;
            return ConstantTime.Equals(data, other.data);
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }
    }
}
