using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Lattice
{
    using Math;

    public static class BetterLattice
    {
        public static List<Vector<BigInteger>> Enumerate(int dimensions, Matrix<BigRational> basis, Vector<BigRational> lower, Vector<BigRational> upper)
        {
            // step 1:  convert to standard form
            //              add slack / surplus variables
            //              substitute original variables

            Matrix<BigRational> transform;
            Vector<BigRational> transformOffset;
            Matrix<BigRational> A;
            Vector<BigRational> b;

            Setup(dimensions, basis, lower, upper, out transform, out transformOffset, out A, out b);

            var solutions = new List<Vector<BigInteger>>();
            Search(dimensions, basis, transform, transformOffset, A, b, solutions, 0);

            return solutions;
        }

        private static void Search(int dimensions, Matrix<BigRational> basis, Matrix<BigRational> transform, Vector<BigRational> transformOffset, Matrix<BigRational> A, Vector<BigRational> b, List<Vector<BigInteger>> solutions, int current)
        {

        }

        private static Vector<BigRational> Solve(Matrix<BigRational> transform, Vector<BigRational> transformOffset, Matrix<BigRational> A, Vector<BigRational> b, Vector<BigRational> c)
        {
            // permutation matrices on A
            var B = Matrix.Create<BigRational>(A.Columns, A.Rows, (row, col) => row < A.Columns - A.Rows ? 0 : row - (A.Columns - A.Rows) == col ? 1 : 0);
            var N = Matrix.Create<BigRational>(A.Columns, A.Columns - A.Rows, (row, col) => row < A.Columns - A.Rows ? row == col ? 1 : 0 : 0);

            var x = (A * B).GetInverse() * b;

            while(true)
            {
                var λ = (A * B).GetTranspose().GetInverse() * (c * B);
                var s = (c * N) - (A * N).GetTranspose() * λ;
                var validEntering = Enumerable.Range(0, A.Columns - A.Rows).Where(i => s[i].Sign < 0);

                if (!validEntering.Any())
                {
                    break;
                }

                var (_, q) = validEntering.Min(i => (s[i], i));
                var d = (A * B).GetInverse() * A.GetColumn(q);

                if (d.All(x => x.Sign <= 0))
                {
                    throw new ArithmeticException("Solution is unbounded");
                }

                var (xq, p) = x.Select((x, i) => (x: x, i: i)).Where(a => d[a.i].Sign > 0).Min(a => (a.x / d[a.i], a.i));

                x -= xq * (d - Vector.CreateBasis<BigRational>(A.Rows, p));
                (B, N) = (Matrix.Create(B.Rows, B.Columns, (row, col) => col == p ? N[row, q] : B[row, col]), Matrix.Create(N.Rows, N.Columns, (row, col) => col == q ? B[row, p] : N[row, col]));
            }

            return x * B.GetTranspose();
        }

        private static void Setup(int dimensions, Matrix<BigRational> basis, Vector<BigRational> lower, Vector<BigRational> upper, out Matrix<BigRational> transform, out Vector<BigRational> transformOffset, out Matrix<BigRational> A, out Vector<BigRational> b)
        {
            var constraints = new BigRational[2 * dimensions, 3 * dimensions + 1];

            for (var row = 0; row < 2 * dimensions; ++row)
            {
                for (var col = 0; col < 3 * dimensions + 1; ++col)
                {
                    constraints[row, col] = 0;
                }
            }

            for (var row = 0; row < dimensions; ++row)
            {
                // lhs <= upper
                // lhs + s = upper
                constraints[2 * row + 0, 2 * row + 0 + dimensions] = 1;
                constraints[2 * row + 0, 3 * dimensions] = upper[row];

                // lhs >= lower
                // lhs - s = lower
                constraints[2 * row + 1, 2 * row + 1 + dimensions] = -1;
                constraints[2 * row + 1, 3 * dimensions] = lower[row];

                for (var col = 0; col < dimensions; ++col)
                {
                    constraints[2 * row + 0, col] = basis[row, col];
                    constraints[2 * row + 1, col] = basis[row, col];
                }
            }

            var count = dimensions;
            constraints = Matrix.Create(constraints).GetRowReduce(ref count).ToArray();

            if (count != dimensions)
            {
                throw new ArithmeticException("Unable to reduce constraint matrix appropriately");
            }

            for (var row = dimensions; row < dimensions * 2; ++row)
            {
                if (constraints[row, 3 * dimensions].Sign < 0)
                {
                    for (var col = 0; col < dimensions * 3 + 1; ++col)
                    {
                        constraints[row, col] = -constraints[row, col];
                    }
                }
            }

            transform = Matrix.Create(dimensions, 2 * dimensions, (row, col) => -constraints[row, dimensions + col]);
            transformOffset = Vector.Create(dimensions, row => -constraints[row, 3 * dimensions]);

            A = Matrix.Create(dimensions, 2 * dimensions, (row, col) => constraints[dimensions + row, dimensions + col]);
            b = Vector.Create(dimensions, row => constraints[row, 3 * dimensions]);
        }
    }
}
