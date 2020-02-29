using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Lattice.Math
{
    public readonly struct BigRational : IComparable<BigRational>, IEquatable<BigRational>
    {
        public static BigRational Zero { get; private set; } = new BigRational(BigInteger.Zero, BigInteger.One, false);
        public static BigRational One { get; private set; } = new BigRational(BigInteger.One, BigInteger.One, false);
        public static BigRational MinusOne { get; private set; } = new BigRational(BigInteger.MinusOne, BigInteger.One, false);

#region op_castto

        public static implicit operator BigRational(BigInteger value) => new BigRational(value);
        public static implicit operator BigRational(sbyte value) => new BigRational(value);
        public static implicit operator BigRational(short value) => new BigRational(value);
        public static implicit operator BigRational(int value) => new BigRational(value);
        public static implicit operator BigRational(long value) => new BigRational(value);
        public static implicit operator BigRational(byte value) => new BigRational(value);
        public static implicit operator BigRational(ushort value) => new BigRational(value);
        public static implicit operator BigRational(uint value) => new BigRational(value);
        public static implicit operator BigRational(ulong value) => new BigRational(value);

#endregion
#region op_castaway

        public static explicit operator BigInteger(BigRational value) => value.Numerator / value.Denominator;
        public static explicit operator sbyte(BigRational value) => (sbyte) (BigInteger) value;
        public static explicit operator short(BigRational value) => (short) (BigInteger) value;
        public static explicit operator int(BigRational value) => (int) (BigInteger) value;
        public static explicit operator long(BigRational value) => (long) (BigInteger) value;
        public static explicit operator byte(BigRational value) => (byte) (BigInteger) value;
        public static explicit operator ushort(BigRational value) => (ushort) (BigInteger) value;
        public static explicit operator uint(BigRational value) => (uint) (BigInteger) value;
        public static explicit operator ulong(BigRational value) => (ulong) (BigInteger) value;

#endregion
#region op_arithmetic

        public static BigRational operator +(BigRational value) => value;
        public static BigRational operator -(BigRational value) => Negate(value);
        public static BigRational operator +(BigRational left, BigRational right) => Add(left, right);
        public static BigRational operator -(BigRational left, BigRational right) => Subtract(left, right);
        public static BigRational operator *(BigRational left, BigRational right) => Multiply(left, right);
        public static BigRational operator /(BigRational left, BigRational right) => Divide(left, right);

#endregion
#region op_compare

        public static bool operator ==(BigRational left, BigRational right)
        {
            return Compare(left, right) == 0;
        }

        public static bool operator !=(BigRational left, BigRational right)
        {
            return Compare(left, right) != 0;
        }

        public static bool operator <(BigRational left, BigRational right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator <=(BigRational left, BigRational right)
        {
            return Compare(left, right) <= 0;
        }

        public static bool operator >(BigRational left, BigRational right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(BigRational left, BigRational right)
        {
            return Compare(left, right) >= 0;
        }

#endregion
#region arithmetic

        public static BigRational AbsoluteValue(BigRational value)
        {
            return new BigRational(BigInteger.Abs(value.Numerator), value.Denominator, false);
        }

        public static BigRational Negate(BigRational value)
        {
            return new BigRational(-value.Numerator, value.Denominator, false);
        }

        public static BigRational Add(BigRational left, BigRational right)
        {
            return new BigRational(left.Numerator * right.Denominator + right.Numerator * left.Denominator, left.Denominator * right.Denominator);
        }

        public static BigRational Subtract(BigRational left, BigRational right)
        {
            return new BigRational(left.Numerator * right.Denominator - right.Numerator * left.Denominator, left.Denominator * right.Denominator);
        }

        public static BigRational Multiply(BigRational left, BigRational right)
        {
            return new BigRational(left.Numerator * right.Numerator, left.Denominator * right.Denominator);
        }

        public static BigRational Divide(BigRational left, BigRational right)
        {
            return new BigRational(left.Numerator * right.Denominator, left.Denominator * right.Numerator);
        }

        public static BigInteger Floor(BigRational value)
        {
            if (value.Sign < 0)
            {
                return value.Numerator / value.Denominator - 1;
            }
            else
            {
                return value.Numerator / value.Denominator;
            }
        }

        public static BigInteger Ceiling(BigRational value)
        {
            return -Floor(-value);
        }

        public static BigRational Min(BigRational left, BigRational right)
        {
            return left < right ? left : right;
        }

        public static BigRational Max(BigRational left, BigRational right)
        {
            return left < right ? right : left;
        }

        public static int Compare(BigRational left, BigRational right)
        {
            return BigInteger.Compare(left.Numerator * right.Denominator, right.Numerator * left.Denominator);
        }

#endregion
#region parse

        private static readonly Regex parseRegex = new Regex(@"([0-9]+)\s*(/\s*([0-9]+))?");

        public static bool TryParse(string s, out BigRational result)
        {
            var match = parseRegex.Match(s ?? "");

            if (match.Success && match.Groups[3].Success)
            {
                var numerator = BigInteger.Zero;
                var denominator = BigInteger.Zero;
                var success = true;

                success &= BigInteger.TryParse(match.Groups[1].Value, out numerator);
                success &= BigInteger.TryParse(match.Groups[1].Value, out denominator);

                if (success)
                {
                    result = new BigRational(numerator, denominator);
                    return true;
                }
            }
            else if (match.Success)
            {
                var value = BigInteger.Zero;
                var success = BigInteger.TryParse(match.Groups[1].Value, out value);

                if (success)
                {
                    result = new BigRational(value);
                    return true;
                }
            }

            result = Zero;
            return false;
        }

        public static BigRational Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var result = Zero;
            var success = TryParse(value, out result);

            if (!success)
            {
                throw new FormatException("The value could not be parsed.");
            }

            return result;
        }

#endregion

        public readonly bool IsZero { get => this.Numerator.IsZero; }
        public readonly bool IsOne { get => this.Numerator.IsOne && this.Denominator.IsOne; }
        public readonly bool IsInteger { get => this.Denominator.IsOne; }
        public readonly int Sign { get => this.Numerator.Sign; }

        public readonly BigInteger Numerator { get; }
        public readonly BigInteger Denominator { get; }

        private BigRational(BigInteger numerator, BigInteger denominator, bool x)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero)
            {
                throw new ArgumentException("Denominator cannot be zero");
            }

            if (denominator.Sign < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            var gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
            this.Numerator = numerator / gcd;
            this.Denominator = denominator / gcd;
        }

        public BigRational(BigInteger numerator) : this(numerator, BigInteger.One, false)
        {
        }

        public int CompareTo(BigRational other)
        {
            return Compare(this, other);
        }

        public bool Equals(BigRational other)
        {
            return Compare(this, other) == 0;
        }

        public override bool Equals(object other)
        {
            if (other is BigRational rational)
            {
                return this.Equals(rational);
            }
            else if (other is BigInteger integer)
            {
                return this.Equals((BigRational) integer);
            }
            else if (other is sbyte sb)
            {
                return this.Equals((BigRational) sb);
            }
            else if (other is short ss)
            {
                return this.Equals((BigRational) ss);
            }
            else if (other is int si)
            {
                return this.Equals((BigRational) si);
            }
            else if (other is long sl)
            {
                return this.Equals((BigRational) sl);
            }
            else if (other is byte ub)
            {
                return this.Equals((BigRational) ub);
            }
            else if (other is ushort us)
            {
                return this.Equals((BigRational) us);
            }
            else if (other is uint ui)
            {
                return this.Equals((BigRational) ui);
            }
            else if (other is ulong ul)
            {
                return this.Equals((BigRational) ul);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (this.Numerator, this.Denominator).GetHashCode();
        }

        public override string ToString()
        {
            if (this.Denominator.IsOne)
            {
                return this.Numerator.ToString();
            }
            else
            {
                return $"{this.Numerator.ToString()}/{this.Denominator.ToString()}";
            }
        }
    }
}
