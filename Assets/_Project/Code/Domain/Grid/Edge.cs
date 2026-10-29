using System;

namespace Puzzle.Domain.Grid
{
    /// <summary>
    /// Cạnh chung giữa hai ô kề nhau. Luôn chuẩn hoá để <see cref="A"/> &lt; <see cref="B"/>,
    /// nhờ vậy Edge(a, b) và Edge(b, a) là cùng một khoá trong từ điển.
    /// </summary>
    public readonly struct Edge : IEquatable<Edge>
    {
        public Cell A { get; }
        public Cell B { get; }

        public Edge(Cell first, Cell second)
        {
            if (!first.IsAdjacentTo(second))
            {
                throw new ArgumentException($"Cells {first} and {second} are not adjacent.");
            }

            bool inOrder = first.CompareTo(second) < 0;
            A = inOrder ? first : second;
            B = inOrder ? second : first;
        }

        /// <summary>Cạnh dọc (hai ô nằm cạnh nhau theo hàng ngang).</summary>
        public bool IsVertical => A.Y == B.Y;

        public bool Touches(Cell cell) => A == cell || B == cell;

        public bool Equals(Edge other) => A == other.A && B == other.B;

        public override bool Equals(object obj) => obj is Edge other && Equals(other);

        public override int GetHashCode() => (A.GetHashCode() * 397) ^ B.GetHashCode();

        public static bool operator ==(Edge left, Edge right) => left.Equals(right);
        public static bool operator !=(Edge left, Edge right) => !left.Equals(right);

        public override string ToString() => $"{A}-{B}";
    }
}
