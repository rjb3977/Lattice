using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Lattice.Math
{
    public static class Vector
    {
#region create

        public static Vector<T> Create<T>(IEnumerable<T> values) => new Vector<T>(values.ToArray());
        public static Vector<T> Create<T>(int length, Func<int, T> generator) => Create(Enumerable.Range(0, length).Select(generator));

        public static Vector<T> CreateZero<T>(int length)
        {
            if (length < 1)
            {
                throw new ArgumentException("Vector length must be greater than zero");
            }

            return Create(length, i => LinearType<T>.Zero);
        }

        public static Vector<T> CreateBasis<T>(int length, int index)
        {
            if (index < 0 || index >= length)
            {
                throw new ArgumentException("Basis index must be in [0, length)");
            }

            return Create(length, i => i == index ? LinearType<T>.One : LinearType<T>.Zero);
        }

#endregion
#region arithmetic

        public static Vector<T> AbsoluteValue<T>(Vector<T> value)
        {
            return Create(value.Select(LinearType<T>.AbsoluteValue));
        }

        public static Vector<T> Negate<T>(Vector<T> value)
        {
            return Create(value.Select(LinearType<T>.Negate));
        }

        public static Vector<T> Add<T>(Vector<T> left, Vector<T> right)
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("Vector dimensions must be equal.");
            }

            return Create(Enumerable.Zip(left, right, LinearType<T>.Add));
        }

        public static Vector<T> Subtract<T>(Vector<T> left, Vector<T> right)
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("Vector dimensions must be equal.");
            }

            return Create(Enumerable.Zip(left, right, LinearType<T>.Subtract));
        }

        public static Vector<T> Multiply<T>(T left, Vector<T> right)
        {
            return Create(right.Select(x => LinearType<T>.Multiply(left, x)));
        }

        public static Vector<T> Multiply<T>(Vector<T> left, T right)
        {
            return Create(left.Select(x => LinearType<T>.Multiply(x, right)));
        }

        public static Vector<T> Divide<T>(Vector<T> left, T right)
        {
            return Create(left.Select(x => LinearType<T>.Divide(x, right)));
        }

        public static T DotProduct<T>(Vector<T> left, Vector<T> right)
        {
            if (left.Length != right.Length)
            {
                throw new ArgumentException("Vector dimensions must be equal.");
            }

            return Enumerable.Zip(left, right, LinearType<T>.Multiply).Aggregate(LinearType<T>.Zero, LinearType<T>.Add);
        }

#endregion
    }

    public readonly struct Vector<T> : IEnumerable<T>, IEquatable<Vector<T>>
    {
#region op_arithmetic

        public static Vector<T> operator +(Vector<T> value) => value;
        public static Vector<T> operator -(Vector<T> value) => Vector.Negate(value);
        public static Vector<T> operator +(Vector<T> left, Vector<T> right) => Vector.Add(left, right);
        public static Vector<T> operator -(Vector<T> left, Vector<T> right) => Vector.Subtract(left, right);
        public static Vector<T> operator *(T left, Vector<T> right) => Vector.Multiply(left, right);
        public static Vector<T> operator *(Vector<T> left, T right) => Vector.Multiply(left, right);
        public static T operator *(Vector<T> left, Vector<T> right) => Vector.DotProduct(left, right);
        public static Vector<T> operator /(Vector<T> left, T right) => Vector.Divide(left, right);

#endregion
#region op_compare

        public static bool operator ==(Vector<T> left, Vector<T> right) => left.Equals(right);
        public static bool operator !=(Vector<T> left, Vector<T> right) => !left.Equals(right);

#endregion

        public readonly bool IsZero { get => values.All(LinearType<T>.IsZero); }
        public readonly int Length { get => values.Length; }

        private readonly T[] values;

        internal Vector(params T[] values)
        {
            this.values = new T[values.LongLength];
            Array.Copy(values, this.values, values.LongLength);
        }

        public T GetComponent(int index)
        {
            return this.values[index];
        }

        public readonly T this[int index] { get => this.values[index]; }

        public IEnumerator<T> GetEnumerator() => (values as IEnumerable<T>).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (values as IEnumerable).GetEnumerator();

        public bool Equals(Vector<T> other)
        {
            if (this.Length != other.Length)
            {
                return false;
            }

            for (var i = 0; i < other.Length; ++i)
            {
                if (LinearType<T>.Compare(this[i], other[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object other)
        {
            if (other is Vector<T> vector)
            {
                return this.Equals(vector);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var hash = 0;

            for (var i = 0; i < this.Length; ++i)
            {
                hash = (hash, this[i]).GetHashCode();
            }

            return hash;
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", this.values)}]";
        }
    }
}
