using System;
using System.Collections.Generic;

namespace Puzzle.Domain.Grid
{
    /// <summary>
    /// Hình dạng bàn chơi: ô nào tồn tại. Bàn có thể khuyết ô nên không giả định hình chữ nhật.
    /// Bất biến; đánh số sẵn ô và cạnh để trạng thái và union-find dùng mảng thay vì từ điển.
    /// </summary>
    public sealed class BoardShape
    {
        private readonly Cell[] _cells;
        private readonly Edge[] _edges;
        private readonly Dictionary<Cell, int> _cellIndex;
        private readonly Dictionary<Edge, int> _edgeIndex;

        public int Width { get; }
        public int Height { get; }

        /// <summary>Các ô theo thứ tự dòng rồi cột; chỉ số trong danh sách = <see cref="IndexOf(Cell)"/>.</summary>
        public IReadOnlyList<Cell> Cells => _cells;

        /// <summary>Các cạnh nằm giữa hai ô cùng tồn tại (không gồm mép bàn).</summary>
        public IReadOnlyList<Edge> Edges => _edges;

        public BoardShape(IEnumerable<Cell> cells)
        {
            if (cells == null) throw new ArgumentNullException(nameof(cells));

            var sorted = new List<Cell>(new HashSet<Cell>(cells));
            sorted.Sort();
            _cells = sorted.ToArray();
            _cellIndex = new Dictionary<Cell, int>(_cells.Length);
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i].X < 0 || _cells[i].Y < 0)
                {
                    throw new ArgumentException($"Cell {_cells[i]} has a negative coordinate.");
                }
                _cellIndex[_cells[i]] = i;
                Width = Math.Max(Width, _cells[i].X + 1);
                Height = Math.Max(Height, _cells[i].Y + 1);
            }

            _edges = CollectEdges(_cells, _cellIndex);
            _edgeIndex = new Dictionary<Edge, int>(_edges.Length);
            for (int i = 0; i < _edges.Length; i++)
            {
                _edgeIndex[_edges[i]] = i;
            }
        }

        /// <summary>Bàn hình chữ nhật đầy đủ ô.</summary>
        public static BoardShape Rectangle(int width, int height)
        {
            var cells = new List<Cell>(width * height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    cells.Add(new Cell(x, y));
                }
            }
            return new BoardShape(cells);
        }

        public bool Contains(Cell cell) => _cellIndex.ContainsKey(cell);

        public bool Contains(Edge edge) => _edgeIndex.ContainsKey(edge);

        /// <summary>Chỉ số của ô; -1 nếu ô không thuộc bàn.</summary>
        public int IndexOf(Cell cell) => _cellIndex.TryGetValue(cell, out int index) ? index : -1;

        /// <summary>Chỉ số của cạnh; -1 nếu cạnh không thuộc bàn.</summary>
        public int IndexOf(Edge edge) => _edgeIndex.TryGetValue(edge, out int index) ? index : -1;

        // Chỉ nhìn sang phải và xuống dưới để mỗi cạnh được thêm đúng một lần.
        private static Edge[] CollectEdges(Cell[] cells, Dictionary<Cell, int> cellIndex)
        {
            var edges = new List<Edge>(cells.Length * 2);
            foreach (Cell cell in cells)
            {
                Cell right = cell.Offset(1, 0);
                Cell down = cell.Offset(0, 1);
                if (cellIndex.ContainsKey(right)) edges.Add(new Edge(cell, right));
                if (cellIndex.ContainsKey(down)) edges.Add(new Edge(cell, down));
            }
            return edges.ToArray();
        }
    }
}
