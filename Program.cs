using System;
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

            // var upper = (BigRational) BigInteger.Pow(2, 48);
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

            Console.WriteLine("lower:  " + lower);
            Console.WriteLine("offset: " + offset);

            var sw = new Stopwatch();
            sw.Start();

            var results = Lattice.Enumerate(dimensions, lower, upper, basis, offset);

            sw.Stop();

            foreach (var x in results)
            {
                Console.WriteLine(x);
            }

            Console.WriteLine($"total:   {results.Count}");
            Console.WriteLine($"elapsed: {sw.Elapsed}");
        }
    }
}
