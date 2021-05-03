using System;

namespace Ristretto
{
    /// <summary>
    /// EdwardsPoint
    /// </summary>
    public class EdwardsPoint
    {
        public static readonly EdwardsPoint IDENTITY =
            new EdwardsPoint(FieldElement.ZERO, FieldElement.ONE, FieldElement.ONE, FieldElement.ZERO);

        public FieldElement X { get; set; }
        public FieldElement Y { get; set; }
        public FieldElement Z { get; set; }
        public FieldElement T { get; set; }

        /**
         * Only for internal use.
         */
        public EdwardsPoint(FieldElement X, FieldElement Y, FieldElement Z, FieldElement T)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.T = T;
        }

        /**
         * Overrides class serialization to use the canonical encoded format.
         */
        //private void writeObject(ObjectOutputStream @out)
        //{
        //    @out.write(this.compress().toByteArray());
        //}

        /**
         * Overrides class serialization to use the canonical encoded format.
         */
        //private void readObject(ObjectInputStream @in)
        //{
        //    byte[] encoded = new byte[32];
        //    @in.EdwardsPoint(encoded);

        //    try
        //    {
        //        EdwardsPoint point = new CompressedEdwardsY(encoded).decompress();
        //        this.X = point.X;
        //        this.Y = point.Y;
        //        this.Z = point.Z;
        //        this.T = point.T;
        //    }
        //    catch (InvalidEncodingException iee)
        //    {
        //        throw new InvalidOperationException(iee.Message);
        //    }
        //}

        //@SuppressWarnings("unused")
        //private void readObjectNoData()
        //{
        //    throw new InvalidOperationException("Cannot deserialize EdwardsPoint from no data");
        //}




        /// <summary>
        /// Compress this point to CompressedEdwardsY format.
        /// </summary>
        /// <returns>the encoded point</returns>
        public CompressedEdwardsY Compress()
        {
            FieldElement recip = this.Z.Invert();
            FieldElement x = this.X.Multiply(recip);
            FieldElement y = this.Y.Multiply(recip);
            byte[] s = y.ToByteArray();
            s[31] |= (byte)(x.IsNegative() << 7);
            return new CompressedEdwardsY(s);
        }

        /// <summary>
        /// Constant-time selection between two EdwardsPoints.
        /// </summary>
        /// <param name="that">the other point.</param>
        /// <param name="b">must be 0 or 1, otherwise results are undefined.</param>
        /// <returns>a copy of this if b == 0, or a copy of that if b == 1.</returns>
        public EdwardsPoint CtSelect(EdwardsPoint that, int b)
        {
            return new EdwardsPoint(this.X.CtSelect(that.X, b), this.Y.CtSelect(that.Y, b), this.Z.CtSelect(that.Z, b),
                    this.T.CtSelect(that.T, b));
        }

        /// <summary>
        /// Equality check overridden to be constant-time. Fails fast if the objects are of different types.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>true if this and other are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is EdwardsPoint))
            {
                return false;
            }

            EdwardsPoint other = (EdwardsPoint)obj;
            return Compress().Equals(other.Compress());
        }

        public override int GetHashCode()
        {
            return Compress().GetHashCode();
        }

        /// <summary>
        /// Convert the representation of this point from extended coordinates to projective coordinates.
        /// </summary>
        /// <returns></returns>
        ProjectivePoint ToProjective()
        {
            return new ProjectivePoint(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Convert to a ProjectiveNielsPoint.
        /// </summary>
        /// <returns></returns>
        public ProjectiveNielsPoint ToProjectiveNiels()
        {
            return new ProjectiveNielsPoint(
                this.Y.Add(this.X), 
                    this.Y.Subtract(this.X), 
                        this.Z,
                            this.T.Multiply(Constants.EDWARDS_2D));
        }

        /// <summary>
        /// Dehomogenize to an AffineNielsPoint.
        /// </summary>
        /// <returns></returns>
        public AffineNielsPoint ToAffineNiels()
        {
            FieldElement recip = this.Z.Invert();
            FieldElement x = this.X.Multiply(recip);
            FieldElement y = this.Y.Multiply(recip);
            FieldElement xy2D = x.Multiply(y).Multiply(Constants.EDWARDS_2D);
            return new AffineNielsPoint(y.Add(x), y.Subtract(x), xy2D);
        }

        /// <summary>
        /// Point addition.
        /// </summary>
        /// <param name="Q">the point to add to this one.</param>
        /// <returns>P + Q</returns>
        public EdwardsPoint Add(EdwardsPoint Q)
        {
            return this.Add(Q.ToProjectiveNiels()).ToExtended();
        }

        /// <summary>
        /// Point addition.
        /// </summary>
        /// <param name="Q">the point to add to this one, in projective "Niels coordinates".</param>
        /// <returns>P + Q</returns>
        public CompletedPoint Add(ProjectiveNielsPoint Q)
        {
            FieldElement YPlusX = this.Y.Add(this.X);
            FieldElement YMinusX = this.Y.Subtract(this.X);
            FieldElement PP = YPlusX.Multiply(Q.YPlusX);
            FieldElement MM = YMinusX.Multiply(Q.YMinusX);
            FieldElement TT2D = this.T.Multiply(Q.T2D);
            FieldElement ZZ = this.Z.Multiply(Q.Z);
            FieldElement ZZ2 = ZZ.Add(ZZ);
            return new CompletedPoint(PP.Subtract(MM), PP.Add(MM), ZZ2.Add(TT2D), ZZ2.Subtract(TT2D));
        }

        /// <summary>
        /// Point addition.
        /// </summary>
        /// <param name="q">the point to add to this one, in affine "Niels coordinates".</param>
        /// <returns>P+q</returns>
        public CompletedPoint Add(AffineNielsPoint q)
        {
            FieldElement YPlusX = this.Y.Add(this.X);
            FieldElement YMinusX = this.Y.Subtract(this.X);
            FieldElement PP = YPlusX.Multiply(q.YPlusx);
            FieldElement MM = YMinusX.Multiply(q.YMinusx);
            FieldElement Txy2D = this.T.Multiply(q.Xy2D);
            FieldElement Z2 = this.Z.Add(this.Z);
            return new CompletedPoint(PP.Subtract(MM), PP.Add(MM), Z2.Add(Txy2D), Z2.Subtract(Txy2D));
        }

        /// <summary>
        /// Point subtraction.
        /// </summary>
        /// <param name="Q">the point to subtract from this one.</param>
        /// <returns>P - Q</returns>
        public EdwardsPoint Subtract(EdwardsPoint Q)
        {
            return this.Subtract(Q.ToProjectiveNiels()).ToExtended();
        }

        /// <summary>
        /// Point subtraction
        /// </summary>
        /// <param name="Q">the point to subtract from this one, in projective "Niels coordinates".</param>
        /// <returns>P - Q</returns>
        CompletedPoint Subtract(ProjectiveNielsPoint Q)
        {
            FieldElement YPlusX = this.Y.Add(this.X);
            FieldElement YMinusX = this.Y.Subtract(this.X);
            FieldElement PM = YPlusX.Multiply(Q.YMinusX);
            FieldElement MP = YMinusX.Multiply(Q.YPlusX);
            FieldElement TT2D = this.T.Multiply(Q.T2D);
            FieldElement ZZ = Z.Multiply(Q.Z);
            FieldElement ZZ2 = ZZ.Add(ZZ);
            return new CompletedPoint(PM.Subtract(MP), PM.Add(MP), ZZ2.Subtract(TT2D), ZZ2.Add(TT2D));
        }

        /// <summary>
        ///  Point subtraction.
        /// </summary>
        /// <param name="q"> the point to subtract from this one, in affine "Niels coordinates".</param>
        /// <returns>P - q</returns>
        CompletedPoint Subtract(AffineNielsPoint q)
        {
            FieldElement YPlusX = this.Y.Add(this.X);
            FieldElement YMinusX = this.Y.Subtract(this.X);
            FieldElement PM = YPlusX.Multiply(q.YMinusx);
            FieldElement MP = YMinusX.Multiply(q.YPlusx);
            FieldElement Txy2D = this.T.Multiply(q.Xy2D);
            FieldElement Z2 = this.Z.Add(this.Z);
            return new CompletedPoint(PM.Subtract(MP), PM.Add(MP), Z2.Subtract(Txy2D), Z2.Add(Txy2D));
        }

        /// <summary>
        /// Point negation.
        /// </summary>
        /// <returns>-P</returns>
        public EdwardsPoint Negate()
        {
            return new EdwardsPoint(this.X.Negate(), this.Y, this.Z, this.T.Negate());
        }

        /// <summary>
        /// Point doubling.
        /// </summary>
        /// <returns>$[2] P$</returns>
        public EdwardsPoint Double()
        {
            return this.ToProjective().Doubling().ToExtended();
        }

        /// <summary>
        /// Constant-time variable-base scalar multiplication.
        /// </summary>
        /// <param name="s">the Scalar to multiply by.</param>
        /// <returns>[s] P</returns>
        public EdwardsPoint Multiply(Scalar s)
        {
            // Construct a lookup table of [P,2P,3P,4P,5P,6P,7P,8P]
            ProjectiveNielsPoint.LookupTable lookupTable = ProjectiveNielsPoint.BuildLookupTable(this);

            // Compute
            //
            // s = s_0 + s_1*16^1 + ... + s_63*16^63,
            //
            // with -8 ≤ s_i < 8 for 0 ≤ i < 63 and -8 ≤ s_63 ≤ 8.
            sbyte[] e = s.ToRadix16;

            // Compute s*P as
            //
            // @formatter:off
            //    s*P = P*(s_0 +   s_1*16^1 +   s_2*16^2 + ... +   s_63*16^63)
            //    s*P =  P*s_0 + P*s_1*16^1 + P*s_2*16^2 + ... + P*s_63*16^63
            //    s*P = P*s_0 + 16*(P*s_1 + 16*(P*s_2 + 16*( ... + P*s_63)...))
            // @formatter:on
            //
            // We sum right-to-left.
            EdwardsPoint Q = EdwardsPoint.IDENTITY;
            for (int i = 63; i >= 0; i--)
            {
                Q = Q.MultiplyByPow2(4);
                Q = Q.Add(lookupTable.Select(e[i])).ToExtended();
            }
            return Q;
        }

        /// <summary>
        ///  Compute $r = [a] A + [b] B$ in variable time, where $B$ is the Ed25519 basepoint.
        /// </summary>
        /// <param name="a">a Scalar</param>
        /// <param name="A">an EdwardsPoint.</param>
        /// <param name="b">a Scalar.</param>
        /// <returns>[a] A + [b] B</returns>
        public static EdwardsPoint VartimeDoubleScalarMultiplyBasepoint(Scalar a, EdwardsPoint A, Scalar b)
        {
            sbyte[] aNaf = a.nonAdjacentForm();
            sbyte[] bNaf = b.nonAdjacentForm();

            ProjectiveNielsPoint.NafLookupTable tableA = ProjectiveNielsPoint.BuildNafLookupTable(A);
            AffineNielsPoint.NafLookupTable tableB = Constants.AFFINE_ODD_MULTIPLES_OF_BASEPOINT;

            int i;
            for (i = 255; i >= 0; --i)
            {
                if (aNaf[i] != 0 || bNaf[i] != 0)
                    break;
            }

            ProjectivePoint r = EdwardsPoint.IDENTITY.ToProjective();
            for (; i >= 0; --i)
            {
                CompletedPoint t = r.Doubling();

                if (aNaf[i] > 0)
                {
                    t = t.ToExtended().Add(tableA.Select(aNaf[i]));
                }
                else if (aNaf[i] < 0)
                {
                    t = t.ToExtended().Subtract(tableA.Select(-aNaf[i]));
                }

                if (bNaf[i] > 0)
                {
                    t = t.ToExtended().Add(tableB.Select(bNaf[i]));
                }
                else if (bNaf[i] < 0)
                {
                    t = t.ToExtended().Subtract(tableB.Select(-bNaf[i]));
                }

                r = t.ToProjective();
            }

            return r.ToExtended();
        }

        /// <summary>
        /// Multiply by the cofactor.
        /// </summary>
        /// <returns> [8] P</returns>
        public EdwardsPoint MultiplyByCofactor()
        {
            return this.MultiplyByPow2(3);
        }

        /// <summary>
        /// Compute [2^k] P by successive doublings.
        /// </summary>
        /// <param name="k">the exponent of 2. Must be positive and non-zero.</param>
        /// <returns>[2^k] P</returns>
        public EdwardsPoint MultiplyByPow2(int k)
        {
            if (!(k > 0))
            {
                throw new ArgumentException("Exponent must be positive and non-zero");
            }
            ProjectivePoint s = this.ToProjective();
            for (int i = 0; i < k - 1; i++)
            {
                s = s.Doubling().ToProjective();
            }
            // Unroll last doubling so we can go directly to extended coordinates.
            return s.Doubling().ToExtended();
        }

        /// <summary>
        /// Determine if this point is the identity.
        /// </summary>
        /// <returns>true if this point is the identity, false otherwise.</returns>
        public bool IsIdentity()
        {
            return this.Equals(EdwardsPoint.IDENTITY);
        }

        /// <summary>
        /// Determine if this point is in the 8-torsion subgroup $(\mathcal E[8])$, and therefore of small order.
        /// </summary>
        /// <returns>true if this point is of small order, false otherwise.</returns>
        public bool IsSmallOrder()
        {
            return this.MultiplyByCofactor().IsIdentity();
        }

        /// <summary>
        /// Determine if this point is contained in the prime-order subgroup $(\mathcal
        /// E[\ell])$, and has no torsion component.
        /// </summary>
        /// <returns>true if this point has zero torsion component and is in the prime-order subgroup, false otherwise.</returns>
        public bool IsTorsionFree()
        {
            return this.Multiply(Constants.BASEPOINT_ORDER).IsIdentity();
        }

        public override string ToString()
        {
            string ir = "EdwardsPoint(\n";
            ir += $"    X: {this.X},\n";
            ir += $"    Y: {this.Y},\n";
            ir += $"    Z: {this.Z},\n";
            ir += $"    T: {this.T},\n)";
            return ir;
        }
    }
}
