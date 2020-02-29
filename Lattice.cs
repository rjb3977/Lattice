using System;
using System.Collections.Generic;
using Extreme.Mathematics;
using Microsoft.Z3;
using System.Linq;

namespace Lattice
{
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

        public static List<Vector<BigInteger>> Enumerate(int dimensions, BigRational lower, BigRational upper, Matrix<BigRational> basis, Vector<BigRational> offset)
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
                    var face = Vector.Create(dimensions, j => lower) - offset;
                    var normal = Vector.Create(dimensions, j => j == i ? BigRational.One : 0);
                    constraints[2 * i + 0] = InHalfPlane(context, variables, basis, basisInverse, face, normal);
                }

                {
                    var face = Vector.Create(dimensions, j => upper) - offset;
                    var normal = Vector.Create(dimensions, j => j == i ? BigRational.MinusOne : 0);
                    constraints[2 * i + 1] = InHalfPlane(context, variables, basis, basisInverse, face, normal);
                }
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
            }
            else
            {
                BigInteger min, max;

                try
                {
                    (min, max) = GetRange(context, optimize, variables[index]);
                }
                catch
                {
                    // Console.WriteLine(index);

                    // foreach (var assertion in optimize.Assertions)
                    // {
                    //     Console.WriteLine(assertion);
                    // }

                    throw;
                }

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
            var min = ParseBigRational(minHandle.Value.ToString());
            Console.WriteLine("min: " + min.ToString());
            optimize.Pop();

            optimize.Push();
            var maxHandle = optimize.MkMaximize(variable);
            optimize.Check();
            Console.WriteLine("max: " + maxHandle.Value.ToString());
            var max = ParseBigRational(maxHandle.Value.ToString());
            Console.WriteLine("max: " + max.ToString());
            optimize.Pop();

            Console.WriteLine("floor(max): " + BigRational.Floor(max));

            return (BigRational.Ceiling(min).Numerator, BigRational.Floor(max).Numerator);
        }

        private static BigRational ParseBigRational(string x)
        {
            var split = x.Split("/");

            // Console.WriteLine(x);

            if (split.Length == 1)
            {
                return new BigRational(BigInteger.Parse(split[0]));
            }
            else
            {
                return new BigRational(BigInteger.Parse(split[0]), BigInteger.Parse(split[1]));
            }
        }

        private static BoolExpr InHalfPlane(Context context, ArithExpr[] variables, Matrix<BigRational> basis, Matrix<BigRational> basisInverse, Vector<BigRational> offset, Vector<BigRational> normal)
        {
            var rhs = Vector.DotProduct(normal, offset);
            var lhs = normal * basis;
            var lcm = rhs.Denominator;
            var gcd = rhs.Numerator;

            for (var i = 0; i < variables.Length; ++i)
            {
                lcm = BigInteger.LeastCommonMultiple(lcm, lhs[i].Denominator);
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
