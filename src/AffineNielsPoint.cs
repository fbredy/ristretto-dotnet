using System;

namespace Ristretto
{
    /// <summary>
    /// A pre-computed point on the affine model of the curve, represented as (y+x, y-x, 2dxy) in "Niels coordinates".
    /// </summary>
    public class AffineNielsPoint
    {
        static readonly AffineNielsPoint IDENTITY = 
            new AffineNielsPoint(FieldElement.ONE, FieldElement.ONE, FieldElement.ZERO);

        public FieldElement YPlusx { get; }
        public FieldElement YMinusx { get; }
        public FieldElement Xy2D { get; }

        public AffineNielsPoint(FieldElement yPlusx, FieldElement yMinusx, FieldElement xy2D)
        {
            this.YPlusx = yPlusx;
            this.YMinusx = yMinusx;
            this.Xy2D = xy2D;
        }

        /// <summary>
        /// Constant-time selection between two AffineNielsPoints.
        /// </summary>
        /// <param name="that">the other point.</param>
        /// <param name="b">must be 0 or 1, otherwise results are undefined.</param>
        /// <returns>a copy of this if b == 0, or a copy of that if b == 1.</returns>

        public AffineNielsPoint CtSelect(AffineNielsPoint that, int b)
        {
            return new AffineNielsPoint(this.YPlusx.CtSelect(that.YPlusx, b), this.YMinusx.CtSelect(that.YMinusx, b),
                    this.Xy2D.CtSelect(that.Xy2D, b));
        }

        /// <summary>
        /// Point negation.         
        /// </summary>
        /// <returns>-P</returns>
        public AffineNielsPoint Negate()
        {
            return new AffineNielsPoint(this.YMinusx, this.YPlusx, this.Xy2D.Negate());
        }

        /// <summary>
        /// Construct a lookup table of [P, [2] P, [3] P, [4] P, [5] P, [6] P, [7] P, [8] P].
        /// </summary>
        /// <param name="edwardPoint">P the point to calculate multiples for</param>
        /// <returns>the lookup table.</returns>
        public static LookupTable BuildLookupTable(EdwardsPoint edwardPoint)
        {
            AffineNielsPoint[] affineNielsPoints = new AffineNielsPoint[8];
            affineNielsPoints[0] = edwardPoint.ToAffineNiels();
            for (int i = 0; i < 7; i++)
            {
                affineNielsPoints[i + 1] = edwardPoint.Add(affineNielsPoints[i]).ToExtended().ToAffineNiels();
            }
            return new AffineNielsPoint.LookupTable(affineNielsPoints);
        }

        public class LookupTable
        {
            private readonly AffineNielsPoint[] table;

            public LookupTable(AffineNielsPoint[] table)
            {
                this.table = table;
            }

            /// <summary>
            /// Given -8 < index < 8, return table[index] P in constant time.
            /// </summary>
            /// <param name="index">the index.</param>
            /// <returns>the pre-computed point.</returns>
            public AffineNielsPoint Select(int index)
            {
                if (index < -8 || index > 8)
                {
                    throw new ArgumentException("x is not in range -8 <= x <= 8");
                }

                // Is x negative?
                int xNegative = ConstantTime.IsNegative(index);
                // |x|
                int xabs = index - (((-xNegative) & index) << 1);

                // |x| P
                AffineNielsPoint t = AffineNielsPoint.IDENTITY;
                for (int i = 1; i < 9; i++)
                {
                    t = t.CtSelect(this.table[i - 1], ConstantTime.equal(xabs, i));
                }

                // -|x| P
                AffineNielsPoint tminus = t.Negate();
                // [x]P
                return t.CtSelect(tminus, xNegative);
            }
        }

        /// <summary>
        /// Construct a lookup table of [P, [3] P, [5] P, [7] P, [9] P, [11] P, [13] P, [15] P].
        /// </summary>
        /// <param name="point"> the point to calculate multiples for.</param>
        /// <returns>the lookup table</returns>
        public static NafLookupTable BuildNafLookupTable(EdwardsPoint point)
        {
            AffineNielsPoint[] points = new AffineNielsPoint[8];
            points[0] = point.ToAffineNiels();
            EdwardsPoint P2 = point.Double();
            for (int i = 0; i < 7; i++)
            {
                points[i + 1] = P2.Add(points[i]).ToExtended().ToAffineNiels();
            }
            return new AffineNielsPoint.NafLookupTable(points);
        }

        public class NafLookupTable
        {
            private readonly AffineNielsPoint[] table;

            public NafLookupTable(AffineNielsPoint[] table)
            {
                this.table = table;
            }

            /// <summary>
            /// Given public, odd index with 0 < index < 2^4, return [index] A.
            /// </summary>
            /// <param name="index"> index.</param>
            /// <returns> the pre-computed point.</returns>
            public AffineNielsPoint Select(int index)
            {
                if ((index % 2 == 0) || index >= 16)
                {
                    throw new ArgumentException("invalid index");
                }

                return this.table[index / 2];
            }
        }
    }

}
