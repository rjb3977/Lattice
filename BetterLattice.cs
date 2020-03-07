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
            Search(dimensions, basis, transform, transformOffset, A, b, solutions, new Stack<BigInteger>());

            return solutions;
        }

        private static void Search(int dimensions, Matrix<BigRational> basis, Matrix<BigRational> transform, Vector<BigRational> transformOffset, Matrix<BigRational> A, Vector<BigRational> b, List<Vector<BigInteger>> solutions, Stack<BigInteger> set)
        {
            if (set.Count == dimensions)
            {
                Console.WriteLine("asdf");
                solutions.Add(Vector.Create(set.AsEnumerable()));
            }
            else
            {
                var n = set.Count;

                // Console.WriteLine("A:\n" + A);
                // Console.WriteLine("b: " + b);
                // Console.WriteLine("c: " + transform.GetRow(n));
                // Console.WriteLine("result:");
                // Console.WriteLine(Solve(A, b, transform.GetRow(n)));

                var lower = transformOffset - transform * Solve(A, b, -transform.GetRow(n));
                var upper = transformOffset - transform * Solve(A, b, transform.GetRow(n));

                // Console.WriteLine(lower);
                // Console.WriteLine(upper);

                var min = BigRational.Ceiling(lower[n]);
                var max = BigRational.Floor(upper[n]);

                Console.WriteLine($"{set.Count}: {min} -> {max}");

                for (var x = min; x <= max; ++x)
                {
                    var A2 = Matrix.Create(A.Rows + 1, A.Columns, (row, col) => row < A.Rows ? A[row, col] : transform[n, col]);
                    var b2 = Vector.Create(b.Length + 1, row => row < b.Length ? b[row] : transformOffset[n] - x);

                    set.Push(x);
                    Search(dimensions, basis, transform, transformOffset, A2, b2, solutions, set);
                    set.Pop();
                }
            }
        }

        public static Vector<BigRational> Solve(Matrix<BigRational> A, Vector<BigRational> b, Vector<BigRational> c)
        {
            // permutation matrices on A
            Matrix<BigRational> B;
            Matrix<BigRational> N;

            {
                var BColumns = new List<int>();
                var NColumns = new List<int>();

                for (var i = 0; i < A.Columns; ++i)
                {
                    BColumns.Add(i);
                    var rank = 0;
                    Matrix.Create(A.Rows, BColumns.Count, (row, col) => A[row, BColumns[col]]).GetRowReduce(out rank);

                    if (rank != BColumns.Count)
                    {
                        BColumns.Remove(i);
                        NColumns.Add(i);
                    }
                }

                B = Matrix.Create<BigRational>(A.Columns, BColumns.Count, (row, col) => row == BColumns[col] ? 1 : 0);
                N = Matrix.Create<BigRational>(A.Columns, NColumns.Count, (row, col) => row == NColumns[col] ? 1 : 0);
            }

            var x = (A * B).GetInverse() * b;

            while(true)
            {
                var λ = (A * B).GetTranspose().GetInverse() * (c * B);
                var s = (c * N) - (A * N).GetTranspose() * λ;
                var validEntering = Enumerable.Range(0, A.Columns - A.Rows).Where(i => s[i].Sign < 0);

                // Console.WriteLine("B:\n" + (A * B));
                // Console.WriteLine("N:\n" + (A * N));
                // Console.WriteLine("λ: " + λ);
                // Console.WriteLine("s: " + s);
                // Console.WriteLine("x: " + x);

                if (!validEntering.Any())
                {
                    break;
                }

                var (_, q) = validEntering.Min(i => (s[i], i));
                var d = (A * B).GetInverse() * (A * N.GetColumn(q));

                // Console.WriteLine("q: " + q);
                // Console.WriteLine("d: " + d);

                if (d.All(x => x.Sign <= 0))
                {
                    throw new ArithmeticException("Solution is unbounded");
                }

                var (xq, p) = x.Select((x, i) => (x: x, i: i)).Where(a => d[a.i].Sign > 0).Min(a => (a.x / d[a.i], a.i));

                // Console.WriteLine("q: " + p);

                x -= xq * d;

                // Console.WriteLine("X: " + x);

                x += xq * Vector.CreateBasis<BigRational>(A.Rows, p);

                // Console.WriteLine("X: " + x);

                (B, N) = (Matrix.Create(B.Rows, B.Columns, (row, col) => col == p ? N[row, q] : B[row, col]), Matrix.Create(N.Rows, N.Columns, (row, col) => col == q ? B[row, p] : N[row, col]));

                // Console.WriteLine();
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
            constraints = Matrix.Create(constraints).GetRowReduce(count).ToArray();

            if (count != dimensions)
            {
                throw new ArithmeticException("Unable to reduce constraint matrix appropriately");
            }

            b = Vector.Create(dimensions, row => constraints[row, 3 * dimensions]);

            for (var row = dimensions; row < dimensions * 2; ++row)
            {
                if (constraints[row, 3 * dimensions].Sign < 0)
                {
                    for (var col = 0; col < 3 * dimensions + 1; ++col)
                    {
                        constraints[row, col] = -constraints[row, col];
                    }
                }
            }

            transform = Matrix.Create(dimensions, 2 * dimensions, (row, col) => constraints[row, dimensions + col]);
            transformOffset = Vector.Create(dimensions, row => constraints[row, 3 * dimensions]);

            A = Matrix.Create(dimensions, 2 * dimensions, (row, col) => constraints[dimensions + row, dimensions + col]);
            b = Vector.Create(dimensions, row => constraints[dimensions + row, 3 * dimensions]);

            var v = Vector.Create(70368744177664, 0, 0, 70368744177664, 70368744177664, 0, 0, 70368744177664, 70368744177664, 0, 70368744177664, 0, 70368744177664, 0, 70368744177664, 0, 0, 70368744177664, 70368744177664, 0, 0, 70368744177664, 70368744177664, 0, 0, 70368744177664, 0, 70368744177664, 0, 2199023255552, 0, 17592186044416, 0, 17592186044416);



            // Console.WriteLine(A);
            // Console.WriteLine(b);
            // Console.WriteLine(transform.GetRow(0));
            // Console.WriteLine(transformOffset[0]);
        }
    }
}
