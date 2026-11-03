using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Shapes;

namespace Puzzle.Domain.Regions
{
    /// <summary>
    /// Một vùng: tập ô người chơi đã gộp với nhau (nối bằng cạnh Joined). Ô chưa gộp là vùng một ô.
    /// Chỉ đọc, do <see cref="RegionMap"/> tạo ra.
    /// </summary>
    public sealed class Region
    {
        private readonly Cell[] _cells;
        private Polyomino _shape;

        public int Id { get; }

        /// <summary>Các ô của vùng theo thứ tự dòng rồi cột.</summary>
        public IReadOnlyList<Cell> Cells => _cells;

        public int Area => _cells.Length;

        /// <summary>
        /// Số cạnh Unknown trên đường bao của vùng. Lớn hơn 0 nghĩa là vùng còn có thể gộp thêm ô,
        /// nên luật nên coi vùng còn nhỏ hoặc thiếu hình là "chưa xong" thay vì "sai".
        /// </summary>
        public int OpenSides { get; }

        /// <summary>Đường bao toàn là ranh giới hoặc mép bàn: vùng không thể lớn thêm.</summary>
        public bool IsClosed => OpenSides == 0;

        /// <summary>Hình của vùng; tính khi cần lần đầu vì không phải luật nào cũng dùng.</summary>
        public Polyomino Shape => _shape ??= new Polyomino(_cells);

        internal Region(int id, Cell[] cells, int openSides)
        {
            Id = id;
            _cells = cells;
            OpenSides = openSides;
        }
    }
}
