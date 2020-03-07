using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lattice.Math
{
    public static class Matrix
    {
#region create

        public static Matrix<T> Create<T>(T[,] values) => new Matrix<T>(values);

        public static Matrix<T> Create<T>(int rows, int cols, Func<int, int, T> generator)
        {
            if (rows < 1 || cols < 1)
            {
                throw new ArgumentException("Dimensions must be greater than zero.");
            }

            var entries = new T[rows, cols];

            for (var row = 0; row < rows; ++row)
            {
                for (var col = 0; col < cols; ++col)
                {
                    entries[row, col] = generator(row, col);
                }
            }

            return Create(entries);
        }

        public static Matrix<T> CreateZero<T>(int rows, int cols) => Create(rows, cols, (row, col) => LinearType<T>.Zero);
        public static Matrix<T> CreateIdentity<T>(int rows, int cols) => Create(rows, cols, (row, col) => row == col ? LinearType<T>.One : LinearType<T>.Zero);
        public static Matrix<T> CreateDiagonal<T>(Vector<T> vector) => Create(vector.Length, vector.Length, (row, col) => row == col ? vector[row] : LinearType<T>.Zero);
        public static Matrix<T> CreateRows<T>(IEnumerable<Vector<T>> rows) => CreateRows(rows.Cast<IEnumerable<T>>());
        public static Matrix<T> CreateColumns<T>(IEnumerable<Vector<T>> cols) => CreateColumns(cols.Cast<IEnumerable<T>>());

        public static Matrix<T> CreateRows<T>(IEnumerable<IEnumerable<T>> rows)
        {
            if (rows.Count() < 1 || rows.First().Count() < 1)
            {
                throw new ArgumentException("Dimensions must be greater than zero.");
            }

            if (!rows.All(row => row.Count() == rows.First().Count()))
            {
                throw new ArgumentException("All rows must have the same length.");
            }

            var entries = new T[rows.Count(), rows.First().Count()];

            foreach (var (row, values) in rows.Select((x, i) => (i, x)))
            {
                foreach (var (col, value) in values.Select((x, i) => (i, x)))
                {
                    entries[row, col] = value;
                }
            }

            return Create(entries);
        }


        public static Matrix<T> CreateColumns<T>(IEnumerable<IEnumerable<T>> cols)
        {
            if (cols.Count() < 1 || cols.First().Count() < 1)
            {
                throw new ArgumentException("Dimensions must be greater than zero.");
            }

            if (!cols.All(col => col.Count() == cols.First().Count()))
            {
                throw new ArgumentException("All rows must have the same length.");
            }

            var entries = new T[cols.First().Count(), cols.Count()];

            foreach (var (col, values) in cols.Select((x, i) => (i, x)))
            {
                foreach (var (row, value) in values.Select((x, i) => (i, x)))
                {
                    entries[row, col] = value;
                }
            }

            return Create(entries);
        }


#endregion
#region arithmetic

        public static Matrix<T> AbsoluteValue<T>(Matrix<T> value)
        {
            return Create(value.Rows, value.Columns, (row, col) => LinearType<T>.AbsoluteValue(value[row, col]));
        }

        public static Matrix<T> Negate<T>(Matrix<T> value)
        {
            return Create(value.Rows, value.Columns, (row, col) => LinearType<T>.Negate(value[row, col]));
        }

        public static Matrix<T> Add<T>(Matrix<T> left, Matrix<T> right)
        {
            if (left.Rows != right.Rows || left.Columns != right.Columns)
            {
                throw new ArgumentException("Matrix dimensions must be equal.");
            }

            return Create(left.Rows, right.Columns, (row, col) => LinearType<T>.Add(left[row, col], right[row, col]));
        }

        public static Matrix<T> Subtract<T>(Matrix<T> left, Matrix<T> right)
        {
            if (left.Rows != right.Rows || left.Columns != right.Columns)
            {
                throw new ArgumentException("Matrix dimensions must be equal.");
            }

            return Create(left.Rows, right.Columns, (row, col) => LinearType<T>.Add(left[row, col], right[row, col]));
        }

        public static Matrix<T> Multiply<T>(Matrix<T> left, Matrix<T> right)
        {
            if (left.Columns != right.Rows)
            {
                throw new ArgumentException("Matrix dimensions must be compatible.");
            }

            return Create(left.Rows, right.Columns, (row, col) => Vector.InnerProduct(left.GetRow(row), right.GetColumn(col)));
        }

        public static Matrix<T> Multiply<T>(T left, Matrix<T> right)
        {
            return Create(right.Rows, right.Columns, (row, col) => LinearType<T>.Multiply(left, right[row, col]));
        }

        public static Matrix<T> Multiply<T>(Matrix<T> left, T right)
        {
            return Create(left.Rows, left.Columns, (row, col) => LinearType<T>.Multiply(left[row, col], right));
        }

        public static Vector<T> Multiply<T>(Vector<T> left, Matrix<T> right)
        {
            if (left.Length != right.Rows)
            {
                throw new ArgumentException("Vector and matrix dimensions must be compatible.");
            }

            return Vector.Create(right.Columns, col => Vector.InnerProduct(left, right.GetColumn(col)));
        }

        public static Vector<T> Multiply<T>(Matrix<T> left, Vector<T> right)
        {
            if (left.Columns != right.Length)
            {
                throw new ArgumentException("Matrix and vector dimensions must be compatible.");
            }

            return Vector.Create(left.Rows, row => Vector.InnerProduct(left.GetRow(row), right));
        }

        public static Matrix<T> Divide<T>(Matrix<T> left, T right)
        {
            return Create(left.Rows, left.Columns, (row, col) => LinearType<T>.Divide(left[row, col], right));
        }

        public static Matrix<T> Transpose<T>(Matrix<T> value)
        {
            return Create(value.Columns, value.Rows, (row, col) => value[col, row]);
        }

        public static Matrix<T> RowReduce<T>(Matrix<T> value, out int rank, int count = -1)
        {
            if (count == -1)
            {
                count = value.Columns;
            }

            var rows = value.GetRows().ToArray();
            var map = new int[count];
            rank = 0;

            for (var col = 0; col < count && rank < value.Rows; ++col)
            {
                var bestRow = rank;

                for (var row = rank + 1; row < value.Rows; ++row)
                {
                    if (LinearType<T>.Compare(LinearType<T>.AbsoluteValue(rows[row][col]), LinearType<T>.AbsoluteValue(rows[bestRow][col])) > 0)
                    {
                        bestRow = row;
                    }
                }

                if (LinearType<T>.IsZero(rows[bestRow][col]))
                {
                    map[col] = -1;
                    continue;
                }

                (rows[rank], rows[bestRow]) = (rows[bestRow], rows[rank]);
                rows[rank] = Vector.Divide(rows[rank], rows[rank][col]);

                for (var row = rank + 1; row < value.Rows; ++row)
                {
                    rows[row] -= Vector.Multiply(rows[rank], rows[row][col]);
                }

                map[col] = rank;
                ++rank;
            }

            for (var col = count - 1; col >= 0; --col)
            {
                for (var row = map[col] - 1; row >= 0; --row)
                {
                    rows[row] -= Vector.Multiply(rows[map[col]], rows[row][col]);
                }
            }

            return Create(value.Rows, value.Columns, (row, col) => rows[row][col]);
        }

        public static Matrix<T> RowReduce<T>(Matrix<T> value, int count = -1)
        {
            var rank = 0;
            var result = RowReduce(value, out rank, count);

            return result;
        }

        public static Matrix<T> Inverse<T>(Matrix<T> value)
        {
            if (value.Rows != value.Columns)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            var identity = CreateIdentity<T>(value.Rows, value.Columns);
            var intermediate = Create(value.Rows, 2 * value.Columns, (row, col) => col < value.Columns ? value[row, col] : identity[row, col - value.Columns]);

            var rank = 0;
            var result = RowReduce(intermediate, out rank, value.Columns);

            if (rank != value.Rows)
            {
                throw new ArithmeticException("Matrix must be invertible.");
            }

            return Create(value.Rows, value.Columns, (row, col) => result[row, value.Columns + col]);
        }

#endregion
    }

    public readonly struct Matrix<T>
    {
#region op_arithmetic

        public static Matrix<T> operator +(Matrix<T> value) => value;
        public static Matrix<T> operator -(Matrix<T> value) => Matrix.Negate(value);
        public static Matrix<T> operator +(Matrix<T> left, Matrix<T> right) => Matrix.Add(left, right);
        public static Matrix<T> operator -(Matrix<T> left, Matrix<T> right) => Matrix.Subtract(left, right);
        public static Matrix<T> operator *(Matrix<T> left, Matrix<T> right) => Matrix.Multiply(left, right);
        public static Matrix<T> operator *(T left, Matrix<T> right) => Matrix.Multiply(left, right);
        public static Matrix<T> operator *(Matrix<T> left, T right) => Matrix.Multiply(left, right);
        public static Vector<T> operator *(Vector<T> left, Matrix<T> right) => Matrix.Multiply(left, right);
        public static Vector<T> operator *(Matrix<T> left, Vector<T> right) => Matrix.Multiply(left, right);
        public static Matrix<T> operator /(Matrix<T> left, T right) => Matrix.Divide(left, right);

#endregion
#region op_compare

        public static bool operator ==(Matrix<T> left, Matrix<T> right) => left.Equals(right);
        public static bool operator !=(Matrix<T> left, Matrix<T> right) => !left.Equals(right);

#endregion

        public readonly int Rows { get => values.GetLength(0); }
        public readonly int Columns { get => values.GetLength(1); }

        private readonly T[,] values;

        internal Matrix(T[,] values)
        {
            this.values = new T[values.GetLength(0), values.GetLength(1)];
            Array.Copy(values, this.values, values.Length);
        }

        public IEnumerable<Vector<T>> GetRows()
        {
            var values = this.values;
            var rows = this.Rows;
            var cols = this.Columns;

            return Enumerable.Range(0, rows).Select(row => Vector.Create(cols, col => values[row, col]));
        }

        public IEnumerable<Vector<T>> GetColumns()
        {
            var values = this.values;
            var rows = this.Rows;
            var cols = this.Columns;

            return Enumerable.Range(0, cols).Select(col => Vector.Create(rows, row => values[row, col]));
        }

        public Vector<T> GetRow(int row)
        {
            var values = this.values;

            return Vector.Create(this.Columns, col => values[row, col]);
        }

        public Vector<T> GetColumn(int col)
        {
            var values = this.values;

            return Vector.Create(this.Rows, row => values[row, col]);
        }

        public Matrix<T> GetTranspose() => Matrix.Transpose(this);
        public Matrix<T> GetInverse() => Matrix.Inverse(this);
        public Matrix<T> GetRowReduce(out int rank, int count = -1) => Matrix.RowReduce(this, out rank, count);
        public Matrix<T> GetRowReduce(int count = -1) => Matrix.RowReduce(this, count);

        public readonly T this[int row, int col] { get => this.values[row, col]; }

        public bool Equals(Matrix<T> other)
        {
            if (this.Rows != other.Rows || this.Columns != other.Columns)
            {
                return false;
            }

            for (var row = 0; row < this.Rows; ++row)
            {
                for (var col = 0; col < this.Columns; ++col)
                {
                    if (LinearType<T>.Compare(this[row, col], other[row, col]) != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool Equals(object other)
        {
            if (other is Matrix<T> matrix)
            {
                return this.Equals(matrix);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var hash = 0;

            for (var row = 0; row < this.Rows; ++row)
            {
                for (var col = 0; col < this.Columns; ++col)
                {
                    hash = (hash, this[row, col]).GetHashCode();
                }
            }

            return hash;
        }

        public override string ToString()
        {
            var strings = new string[this.Rows, this.Columns];
            var lengths = new int[this.Columns];

            for (var row = 0; row < this.Rows; ++row)
            {
                for (var col = 0; col < this.Columns; ++col)
                {
                    strings[row, col] = this[row, col].ToString();

                    if (strings[row, col].Length > lengths[col])
                    {
                        lengths[col] = strings[row, col].Length;
                    }
                }
            }


            // [[x, x]]
            var result = new StringBuilder();

            for (var row = 0; row < this.Rows; ++row)
            {
                for (var col = 0; col < this.Columns; ++col)
                {
                    result.Append(col == 0 ? row == 0 ? "[[" : " [" : ", ");
                    result.Append(' ', lengths[col] - strings[row, col].Length);
                    result.Append(strings[row, col]);
                }

                result.Append(row == this.Rows - 1 ? "]]" : "]\n");
            }

            return result.ToString();
        }

        public T[,] ToArray()
        {
            var values = new T[this.Rows, this.Columns];
            Array.Copy(this.values, values, this.values.Length);

            return values;
        }
    }
}
