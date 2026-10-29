using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;

namespace Puzzle.Domain.Shapes
{
    /// <summary>
    /// Một hình ghép từ các ô vuông, luôn ở dạng chuẩn: dịch về góc (0,0) và sắp xếp ô.
    /// Nhờ vậy hai hình chỉ khác vị trí trên bàn sẽ bằng nhau, và có thể so thứ tự để chọn dạng chính tắc.
    /// Bất biến.
    /// </summary>
    public sealed class Polyomino : IEquatable<Polyomino>, IComparable<Polyomino>
    {
        private readonly Cell[] _cells;
        private readonly int _hash;

        /// <summary>Các ô đã dịch về (0,0), theo thứ tự dòng rồi cột.</summary>
        public IReadOnlyList<Cell> Cells => _cells;

        public int Area => _cells.Length;

        public Polyomino(IEnumerable<Cell> cells)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            _cells = Normalize(cells);
            if (_cells.Length == 0) throw new ArgumentException("A polyomino needs at least one cell.");
            _hash = ComputeHash(_cells);
        }

        /// <summary>
        /// Đọc hình từ các dòng ASCII, '#' là ô có mặt, ký tự khác là trống.
        /// Dùng cho level viết tay và test.
        /// </summary>
        public static Polyomino Parse(params string[] rows)
        {
            var cells = new List<Cell>();
            for (int y = 0; y < rows.Length; y++)
            {
                for (int x = 0; x < rows[y].Length; x++)
                {
                    if (rows[y][x] == '#') cells.Add(new Cell(x, y));
                }
            }
            return new Polyomino(cells);
        }

        /// <summary>Xoay 90° theo chiều kim đồng hồ (trên màn hình, Y hướng xuống).</summary>
        public Polyomino Rotate90()
        {
            var rotated = new Cell[_cells.Length];
            for (int i = 0; i < _cells.Length; i++)
            {
                rotated[i] = new Cell(-_cells[i].Y, _cells[i].X);
            }
            return new Polyomino(rotated);
        }

        /// <summary>Lật gương qua trục dọc.</summary>
        public Polyomino Mirror()
        {
            var mirrored = new Cell[_cells.Length];
            for (int i = 0; i < _cells.Length; i++)
            {
                mirrored[i] = new Cell(-_cells[i].X, _cells[i].Y);
            }
            return new Polyomino(mirrored);
        }

        public bool Equals(Polyomino other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other is null || other._hash != _hash || other._cells.Length != _cells.Length) return false;
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] != other._cells[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Thứ tự toàn phần: diện tích trước, rồi so từng ô. Chỉ cần ổn định để
        /// <see cref="ShapeVariants"/> chọn ra cùng một dạng chính tắc cho mọi biến thể.
        /// </summary>
        public int CompareTo(Polyomino other)
        {
            if (other is null) return 1;
            int byArea = Area.CompareTo(other.Area);
            if (byArea != 0) return byArea;
            for (int i = 0; i < _cells.Length; i++)
            {
                int byCell = _cells[i].CompareTo(other._cells[i]);
                if (byCell != 0) return byCell;
            }
            return 0;
        }

        public override bool Equals(object obj) => obj is Polyomino other && Equals(other);

        public override int GetHashCode() => _hash;

        public override string ToString() => string.Join(" ", _cells);

        private static Cell[] Normalize(IEnumerable<Cell> cells)
        {
            var unique = new List<Cell>(new HashSet<Cell>(cells));
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            foreach (Cell cell in unique)
            {
                minX = Math.Min(minX, cell.X);
                minY = Math.Min(minY, cell.Y);
            }
            for (int i = 0; i < unique.Count; i++)
            {
                unique[i] = unique[i].Offset(-minX, -minY);
            }
            unique.Sort();
            return unique.ToArray();
        }

        private static int ComputeHash(Cell[] cells)
        {
            int hash = cells.Length;
            foreach (Cell cell in cells)
            {
                hash = (hash * 31) ^ cell.GetHashCode();
            }
            return hash;
        }
    }
}
