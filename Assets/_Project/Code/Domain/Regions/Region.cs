using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Shapes;

namespace Puzzle.Domain.Regions
{
    /// <summary>
    /// Một vùng: tập ô liên thông không bị ranh giới chia cắt. Chỉ đọc, do <see cref="RegionMap"/> tạo ra.
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
        /// Số cạnh Unknown nằm bên trong vùng. Lớn hơn 0 nghĩa là người chơi còn có thể
        /// cắt vùng này ra, nên luật nên coi lỗi về hình/diện tích là "chưa xong" thay vì "sai".
        /// </summary>
        public int UnknownInnerEdges { get; }

        public bool IsSettled => UnknownInnerEdges == 0;

        /// <summary>Hình của vùng; tính khi cần lần đầu vì không phải luật nào cũng dùng.</summary>
        public Polyomino Shape => _shape ??= new Polyomino(_cells);

        internal Region(int id, Cell[] cells, int unknownInnerEdges)
        {
            Id = id;
            _cells = cells;
            UnknownInnerEdges = unknownInnerEdges;
        }
    }
}
