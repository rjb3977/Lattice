﻿using System;
using System.Numerics;
using System.IO;
using System.Diagnostics;

namespace Lattice
{
    using Math;

    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: dotnet run input");
                return;
            }

            var text = File.ReadAllLines(args[0]);
            var dimensions = int.Parse(text[0]);
            var basisEntries = new BigRational[dimensions, dimensions];
            var offsetEntries = new BigRational[dimensions];
            var line = text[dimensions + 2].Split(",");

            for (var col = 0; col < dimensions; ++col)
            {
                offsetEntries[col] = BigInteger.Parse(line[col]);
            }

            for (var row = 0; row < dimensions; ++row)
            {
                line = text[row + 1].Split(",");

                for (var col = 0; col < dimensions; ++col)
                {
                    basisEntries[row, col] = BigInteger.Parse(line[col]);
                }
            }

            // Console.ReadKey();

            // var upper = Vector.Create<BigRational>(12, i => BigInteger.Pow(2, 48));
            // var lower = upper - upper / 10;
            // var lower = BigRational.Zero;
            // var upper = new BigRational(50048832364193);
            var basis = Matrix.Create(basisEntries).GetTranspose();
            var offset = Vector.Create(offsetEntries);

            // var lower = Vector.CreateZero<BigRational>(17);
            // var upper = Vector.Create<BigRational>(
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 46,
            //     1 << 41,
            //     1 << 44,
            //     1 << 44
            // );

            Console.ReadKey();

            var lower = Vector.Create<BigRational>(
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                211106232532992,
                279275953455104,
                263882790666240,
                263882790666240
            );
            var upper = Vector.Create<BigRational>(dimensions, x => 1L << 48);

            // Console.WriteLine("lower:  " + lower);
            // Console.WriteLine("offset: " + offset);

            var sw = new Stopwatch();
            sw.Start();

            // var results = Lattice.Enumerate(dimensions, lower, upper, basis, offset);
            var results = BetterLattice.Enumerate(dimensions, basis, lower - offset, upper - offset);

            sw.Stop();

            foreach (var x in results)
            {
                Console.WriteLine(x);
            }

            Console.WriteLine($"total:       {results.Count}");
            Console.WriteLine($"elapsed:     {sw.Elapsed}");
            Console.WriteLine($"solve calls: {Lattice.SolveCalls}");
        }

        static void Main2(string[] args)
        {
            var A = Matrix.Create(new BigRational[,] { { 3, 2, 1, 1, 0 }, { 2, 5, 3, 0, 1 } });
            var b = Vector.Create(new BigRational[] { 10, 15 });
            var c = Vector.Create(new BigRational[] { -2, -3, -4, 0, 0 });

            Console.WriteLine(BetterLattice.Solve(A, b, c));
        }
    }
}
