using System;

namespace Ristretto
{
    /// <summary>
    /// A pre-computed point on the $\mathbb P^3$ model of the curve, represented as $(Y+X, Y-X, Z, 2dXY)$ in "Niels coordinates".
    /// </summary>
    public class ProjectiveNielsPoint
    {
        static readonly ProjectiveNielsPoint IDENTITY = new ProjectiveNielsPoint(FieldElement.ONE, FieldElement.ONE,
                FieldElement.ONE, FieldElement.ZERO);

        public FieldElement YPlusX { get; set; }
        public FieldElement YMinusX { get; set; }
        public FieldElement Z { get; set; }
        public FieldElement T2D { get; set; }

        public ProjectiveNielsPoint(FieldElement YPlusX, FieldElement YMinusX, FieldElement Z, FieldElement T2D)
        {
            this.YPlusX = YPlusX;
            this.YMinusX = YMinusX;
            this.Z = Z;
            this.T2D = T2D;
        }


        /// <summary>
        /// Constant-time selection between two ProjectiveNielsPoints.
        /// </summary>
        /// <param name="that">the other point.</param>
        /// <param name="b">must be 0 or 1, otherwise results are undefined.</param>
        /// <returns>a copy of this if b == 0, or a copy of that if b == 1.</returns>
        public ProjectiveNielsPoint CtSelect(ProjectiveNielsPoint that, int b)
        {
            return new ProjectiveNielsPoint(this.YPlusX.CtSelect(that.YPlusX, b), this.YMinusX.CtSelect(that.YMinusX, b),
                    this.Z.CtSelect(that.Z, b), this.T2D.CtSelect(that.T2D, b));
        }

        /// <summary>
        /// Point negation
        /// </summary>
        /// <returns>-P</returns>
        public ProjectiveNielsPoint Negate()
        {
            return new ProjectiveNielsPoint(this.YMinusX, this.YPlusX, this.Z, this.T2D.Negate());
        }

        /// <summary>
        /// Construct a lookup table of [P, [2] P, [3] P, [4] P, [5] P, [6] P, [7] P, [8] P].
        /// </summary>
        /// <param name="P">the point to calculate multiples for.</param>
        /// <returns>the lookup table.</returns>
        public static LookupTable BuildLookupTable(EdwardsPoint P)
        {
            ProjectiveNielsPoint[] points = new ProjectiveNielsPoint[8];
            points[0] = P.ToProjectiveNiels();
            for (int i = 0; i < 7; i++)
            {
                points[i + 1] = P.Add(points[i]).ToExtended().ToProjectiveNiels();
            }
            return new ProjectiveNielsPoint.LookupTable(points);
        }

        public class LookupTable
        {
            private readonly ProjectiveNielsPoint[] table;

            public LookupTable(ProjectiveNielsPoint[] table)
            {
                this.table = table;
            }

            /// <summary>
            /// Given -8 <= x <= 8, return [x]P in constant time.
            /// </summary>
            /// <param name="index">the index.</param>
            /// <returns>the pre-computed point.</returns>
            public ProjectiveNielsPoint Select(int index)
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
                ProjectiveNielsPoint t = ProjectiveNielsPoint.IDENTITY;
                for (int i = 1; i < 9; i++)
                {
                    t = t.CtSelect(this.table[i - 1], ConstantTime.Equal(xabs, i));
                }

                // -|x| P
                ProjectiveNielsPoint tminus = t.Negate();
                // [x]P
                return t.CtSelect(tminus, xNegative);
            }
        }

        /// <summary>
        /// Construct a lookup table of [P, [3]P, [5]P, [7]P, [9]P, [11]P, [13]P,        [15] P]
        /// </summary>
        /// <param name="P">the point to calculate multiples for.</param>
        /// <returns>the lookup table.</returns>
        public static NafLookupTable BuildNafLookupTable(EdwardsPoint P)
        {
            ProjectiveNielsPoint[] points = new ProjectiveNielsPoint[8];
            points[0] = P.ToProjectiveNiels();
            EdwardsPoint P2 = P.Double();
            for (int i = 0; i < 7; i++)
            {
                points[i + 1] = P2.Add(points[i]).ToExtended().ToProjectiveNiels();
            }
            return new ProjectiveNielsPoint.NafLookupTable(points);
        }

        public class NafLookupTable
        {
            private readonly ProjectiveNielsPoint[] table;

            public NafLookupTable(ProjectiveNielsPoint[] table)
            {
                this.table = table;
            }

            /// <summary>
            /// Given public, odd x with 0 < x < 2^4, return [x]A.
            /// </summary>
            /// <param name="x">the index.</param>
            /// <returns>the pre-computed point.</returns>
            public ProjectiveNielsPoint Select(int x)
            {
                if ((x % 2 == 0) || x >= 16)
                {
                    throw new ArgumentException("invalid x");
                }

                return this.table[x / 2];
            }
        }
    }
}
