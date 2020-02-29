using System;
using System.Collections.Generic;

namespace Lattice.Math
{
    public interface ILinearType<T>
    {
        T Zero { get; }
        T One { get; }
        T MinusOne { get; }

        bool IsZero(T value);
        int Sign(T value);
        int Compare(T left, T right);

        T Min(T left, T right);
        T Max(T left, T right);

        T AbsoluteValue(T value);
        T Negate(T value);
        T Add(T left, T right);
        T Subtract(T left, T right);
        T Multiply(T left, T right);
        T Divide(T left, T right);
    }

    public static class LinearType<T>
    {
        private static ILinearType<T> _type = null;
        public static ILinearType<T> Type { get => _type = _type ?? LinearTypes.GetLinearType<T>(); }

        public static T Zero { get => Type.Zero; }
        public static T One { get => Type.One; }
        public static T MinusOne { get => Type.MinusOne; }

        public static bool IsZero(T value) => Type.IsZero(value);
        public static int Sign(T value) => Type.Sign(value);
        public static int Compare(T left, T right) => Type.Compare(left, right);

        public static T Min(T left, T right) => Type.Min(left, right);
        public static T Max(T left, T right) => Type.Max(left, right);

        public static T AbsoluteValue(T value) => Type.AbsoluteValue(value);
        public static T Negate(T value) => Type.Negate(value);
        public static T Add(T left, T right) => Type.Add(left, right);
        public static T Subtract(T left, T right) => Type.Subtract(left, right);
        public static T Multiply(T left, T right) => Type.Multiply(left, right);
        public static T Divide(T left, T right) => Type.Divide(left, right);
    }

    public static class LinearTypes
    {
        private static readonly Dictionary<Type, dynamic> types = new Dictionary<Type, dynamic>();

        static LinearTypes()
        {
            RegisterLinearType(new SingleLinearType());
            RegisterLinearType(new DoubleLinearType());
            RegisterLinearType(new DecimalLinearType());
            RegisterLinearType(new BigRationalLinearType());
        }

        public static ILinearType<T> GetLinearType<T>()
        {
            dynamic type;

            if (!types.TryGetValue(typeof(T), out type))
            {
                throw new ArgumentException($"No registered LinearType for type {typeof(T).FullName}");
            }

            return (ILinearType<T>) type;
        }

        public static void RegisterLinearType<T>(ILinearType<T> type)
        {
            types[typeof(T)] = type;
        }
    }

    internal class SingleLinearType : ILinearType<Single>
    {
        public Single Zero { get => 0.0f; }
        public Single One { get => 1.0f; }
        public Single MinusOne { get => -1.0f; }

        public bool IsZero(Single value) => value == 0.0f;
        public int Sign(Single value) => System.Math.Sign(value);
        public int Compare(Single left, Single right) => System.Math.Sign(left - right);

        public Single Min(Single left, Single right) => System.Math.Min(left, right);
        public Single Max(Single left, Single right) => System.Math.Max(left, right);

        public Single AbsoluteValue(Single value) => System.Math.Abs(value);
        public Single Negate(Single value) => -value;
        public Single Add(Single left, Single right) => left + right;
        public Single Subtract(Single left, Single right) => left - right;
        public Single Multiply(Single left, Single right) => left * right;
        public Single Divide(Single left, Single right) => left / right;
    }

    internal class DoubleLinearType : ILinearType<Double>
    {
        public Double Zero { get => 0.0; }
        public Double One { get => 1.0; }
        public Double MinusOne { get => -1.0; }

        public bool IsZero(Double value) => value == 0.0;
        public int Sign(Double value) => System.Math.Sign(value);
        public int Compare(Double left, Double right) => System.Math.Sign(left - right);

        public Double Min(Double left, Double right) => System.Math.Min(left, right);
        public Double Max(Double left, Double right) => System.Math.Max(left, right);

        public Double AbsoluteValue(Double value) => System.Math.Abs(value);
        public Double Negate(Double value) => -value;
        public Double Add(Double left, Double right) => left + right;
        public Double Subtract(Double left, Double right) => left - right;
        public Double Multiply(Double left, Double right) => left * right;
        public Double Divide(Double left, Double right) => left / right;
    }

    internal class DecimalLinearType : ILinearType<Decimal>
    {
        public Decimal Zero { get => 0.0m; }
        public Decimal One { get => 1.0m; }
        public Decimal MinusOne { get => -1.0m; }

        public bool IsZero(Decimal value) => value == 0.0m;
        public int Sign(Decimal value) => System.Math.Sign(value);
        public int Compare(Decimal left, Decimal right) => System.Math.Sign(left - right);

        public Decimal Min(Decimal left, Decimal right) => System.Math.Min(left, right);
        public Decimal Max(Decimal left, Decimal right) => System.Math.Max(left, right);

        public Decimal AbsoluteValue(Decimal value) => System.Math.Abs(value);
        public Decimal Negate(Decimal value) => -value;
        public Decimal Add(Decimal left, Decimal right) => left + right;
        public Decimal Subtract(Decimal left, Decimal right) => left - right;
        public Decimal Multiply(Decimal left, Decimal right) => left * right;
        public Decimal Divide(Decimal left, Decimal right) => left / right;
    }

    internal class BigRationalLinearType : ILinearType<BigRational>
    {
        public BigRational Zero { get => BigRational.Zero; }
        public BigRational One { get => BigRational.One; }
        public BigRational MinusOne { get => BigRational.MinusOne; }

        public bool IsZero(BigRational value) => value.IsZero;
        public int Sign(BigRational value) => value.Sign;
        public int Compare(BigRational left, BigRational right) => BigRational.Compare(left, right);

        public BigRational AbsoluteValue(BigRational value) => BigRational.AbsoluteValue(value);
        public BigRational Min(BigRational left, BigRational right) => BigRational.Min(left, right);
        public BigRational Max(BigRational left, BigRational right) => BigRational.Max(left, right);

        public BigRational Negate(BigRational value) => -value;
        public BigRational Add(BigRational left, BigRational right) => left + right;
        public BigRational Subtract(BigRational left, BigRational right) => left - right;
        public BigRational Multiply(BigRational left, BigRational right) => left * right;
        public BigRational Divide(BigRational left, BigRational right) => left / right;
    }
}
