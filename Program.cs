using System;
using System.IO;
using System.Diagnostics;
// using MathNet.Numerics;
// using MathNet.Numerics.LinearAlgebra;
//using MathNet.Symbolics;

using Extreme.Mathematics;

namespace Lattice
{
    static class Program
    {
        static Program()
        {
            NumericsConfiguration.Providers.RegisterGenericProvider();
        }

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
                    basisEntries[col, row] = BigInteger.Parse(line[col]);
                }
            }

            var upper = BigRational.Pow(2, 48);
            var lower = upper - upper / 10;
            var basis = Matrix.Create(basisEntries);
            var offset = Vector.Create(offsetEntries);

            var sw = new Stopwatch();
            sw.Start();

            var results = Lattice.Enumerate2(dimensions, lower, upper, basis, offset);

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
