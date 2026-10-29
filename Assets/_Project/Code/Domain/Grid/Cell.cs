using System;

namespace Puzzle.Domain.Grid
{
    /// <summary>
    /// Toạ độ một ô trên lưới. X tăng sang phải, Y tăng xuống dưới,
    /// giống thứ tự đọc của bản vẽ ASCII để level viết tay khỏi phải đảo trục.
    /// </summary>
    public readonly struct Cell : IEquatable<Cell>, IComparable<Cell>
    {
        public int X { get; }
        public int Y { get; }

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Hai ô có chung một cạnh (không tính chéo).</summary>
        public bool IsAdjacentTo(Cell other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y) == 1;
        }

        public Cell Offset(int dx, int dy)
        {
            return new Cell(X + dx, Y + dy);
        }

        /// <summary>
        /// Thứ tự theo dòng rồi theo cột. Dùng để chuẩn hoá cạnh (a &lt; b)
        /// và để so sánh hình, nên phải ổn định và toàn phần.
        /// </summary>
        public int CompareTo(Cell other)
        {
            int byRow = Y.CompareTo(other.Y);
            return byRow != 0 ? byRow : X.CompareTo(other.X);
        }

        public bool Equals(Cell other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Cell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X * 397) ^ Y;
        }

        public static bool operator ==(Cell left, Cell right) => left.Equals(right);
        public static bool operator !=(Cell left, Cell right) => !left.Equals(right);

        public override string ToString() => $"({X},{Y})";
    }
}
