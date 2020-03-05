using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Z3;

namespace Lattice
{
    using Math;

    public static class Lattice
    {
        private static Vector<BigRational> Example = Vector.Create<BigRational>(
            253337345868468,
            -22694297408,
            -185181437920550,
            -210385418508672,
            -50390170855715,
            -108859710755132,
            -69114274894019,
            -55156597183194,
            -116063811706968,
            -159549169155694,
            -17935017627050,
            -82866953394718
        );

        public static List<Vector<BigInteger>> Enumerate(int dimensions, Vector<BigRational> lower, Vector<BigRational> upper, Matrix<BigRational> basis, Vector<BigRational> offset)
        {
            using var context = new Context();
            using var optimize = context.MkOptimize();
            var variables = new ArithExpr[dimensions];
            var basisInverse = basis.GetInverse();

            for (var i = 0; i < dimensions; ++i)
            {
                variables[i] = context.MkRealConst($"x{i}");
            }

            var constraints = new BoolExpr[2 * dimensions];

            for (var i = 0; i < dimensions; ++i)
            {
                {
                    var face = lower - offset;
                    var normal = Vector.Create(dimensions, j => j == i ? BigRational.One : 0);
                    constraints[2 * i + 0] = InHalfPlane(context, variables, basis, basisInverse, face, normal);
                }

                {
                    var face = upper - offset;
                    var normal = Vector.Create(dimensions, j => j == i ? BigRational.MinusOne : 0);
                    constraints[2 * i + 1] = InHalfPlane(context, variables, basis, basisInverse, face, normal);
                }
            }

            var zeroes = new Expr[dimensions];

            for (var i = 0; i < dimensions; ++i)
            {
                zeroes[i] = context.MkReal(0);
            }

            optimize.Assert(constraints);

            var result = new List<Vector<BigInteger>>();
            Solve(context, optimize, variables, new BigInteger?[dimensions], result, dimensions - 1);

            return result;
        }

        private static void Solve(Context context, Optimize optimize, ArithExpr[] variables, BigInteger?[] values, List<Vector<BigInteger>> result, int index)
        {
            // var index = -1;
            // var (min, max) = (BigInteger.MinValue, BigInteger.MaxValue);

            // for (var i = 0; i < variables.Length; ++i)
            // {
            //     if (values[i] == null)
            //     {
            //         var (currentMin, currentMax) = GetRange(context, optimize, variables[i]);

            //         if (currentMax - currentMin < max - min)
            //         {
            //             index = i;
            //             (min, max) = (currentMin, currentMax);
            //         }
            //     }
            // }

            if (index == -1)
            {
                result.Add(Vector.Create(variables.Length, i => values[i].Value));
                Console.WriteLine(Vector.Create(variables.Length, i => values[i].Value));
            }
            else
            {
                var (min, max) = GetRange(context, optimize, variables[index]);

                Console.WriteLine($"{index}: {min} -> {max}");

                for (var x = min; x <= max; x += 1)
                {
                    // Console.WriteLine($"setting x{index} to {x}");

                    values[index] = x;
                    optimize.Push();
                    optimize.Assert(context.MkEq(variables[index], context.MkInt(x.ToString())));
                    Solve(context, optimize, variables, values, result, index - 1);
                    optimize.Pop();
                }

                values[index] = null;
            }
        }

        private static ValueTuple<BigInteger, BigInteger> GetRange(Context context, Optimize optimize, ArithExpr variable)
        {
            optimize.Push();
            var minHandle = optimize.MkMinimize(variable);
            optimize.Check();
            var min = BigRational.Parse(minHandle.Value.ToString());
            // Console.WriteLine(minHandle.Value.ToString());
            optimize.Pop();

            // Console.WriteLine(min);

            optimize.Push();
            var maxHandle = optimize.MkMaximize(variable);
            optimize.Check();
            var max = BigRational.Parse(maxHandle.Value.ToString());
            // Console.WriteLine(maxHandle.Value.ToString());
            optimize.Pop();

            // Console.WriteLine(max);

            return (BigRational.Ceiling(min), BigRational.Floor(max));
        }

        private static BoolExpr InHalfPlane(Context context, ArithExpr[] variables, Matrix<BigRational> basis, Matrix<BigRational> basisInverse, Vector<BigRational> offset, Vector<BigRational> normal)
        {
            var rhs = Vector.InnerProduct(normal, offset);
            var lhs = normal * basis;
            var lcm = rhs.Denominator;
            var gcd = rhs.Numerator;

            for (var i = 0; i < variables.Length; ++i)
            {
                lcm = lcm * lhs[i].Denominator / BigInteger.GreatestCommonDivisor(lcm, lhs[i].Denominator);
                gcd = BigInteger.GreatestCommonDivisor(gcd, lhs[i].Numerator);
            }

            var scale = new BigRational(lcm, gcd);

            rhs *= scale;
            lhs *= scale;

            var rhsExpr = context.MkInt(rhs.ToString());
            var lhsExprs = new ArithExpr[variables.Length];

            for (var i = 0; i < variables.Length; ++i)
            {
                lhsExprs[i] = variables[i] * context.MkInt(lhs[i].ToString());
            }

            return context.MkAdd(lhsExprs) >= rhsExpr;
        }
    }
}
