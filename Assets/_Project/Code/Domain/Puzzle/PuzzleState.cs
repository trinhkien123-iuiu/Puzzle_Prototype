using System;
using Puzzle.Domain.Grid;

namespace Puzzle.Domain.Puzzle
{
    /// <summary>
    /// Nguồn sự thật duy nhất của một ván: trạng thái từng cạnh và màu tô từng ô.
    /// Mọi thứ khác (vùng, lỗi) tính ra từ đây. Chỉ Command ở tầng Application được gọi các hàm Set.
    /// </summary>
    public sealed class PuzzleState
    {
        /// <summary>Ô chưa tô màu.</summary>
        public const int NoPaint = 0;

        // Mảng theo chỉ số cạnh/ô của BoardShape: nhỏ, sao chép rẻ, không cấp phát khi đọc.
        private readonly EdgeState[] _edges;
        private readonly int[] _paint;

        public BoardShape Board { get; }

        public PuzzleState(BoardShape board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            _edges = new EdgeState[board.Edges.Count];
            _paint = new int[board.Cells.Count];
        }

        private PuzzleState(BoardShape board, EdgeState[] edges, int[] paint)
        {
            Board = board;
            _edges = edges;
            _paint = paint;
        }

        public EdgeState GetEdge(Edge edge) => _edges[RequireIndex(edge)];

        public EdgeState GetEdge(int edgeIndex) => _edges[edgeIndex];

        public void SetEdge(Edge edge, EdgeState state)
        {
            _edges[RequireIndex(edge)] = state;
        }

        /// <summary>
        /// Màu người chơi tô cho ô (chỉ số bảng màu, <see cref="NoPaint"/> = chưa tô).
        /// Màu chỉ để nhìn, luật không đọc; lưu ở đây để undo/lưu ván đi chung một đường với cạnh.
        /// </summary>
        public int GetPaint(Cell cell) => _paint[RequireIndex(cell)];

        public int GetPaint(int cellIndex) => _paint[cellIndex];

        public void SetPaint(Cell cell, int paint)
        {
            _paint[RequireIndex(cell)] = paint;
        }

        /// <summary>Bản sao độc lập, cho solver hoặc để so sánh trước/sau trong test command.</summary>
        public PuzzleState Clone() => new PuzzleState(Board, (EdgeState[])_edges.Clone(), (int[])_paint.Clone());

        /// <summary>Cùng bàn, cùng mọi cạnh và mọi màu tô.</summary>
        public bool SameAs(PuzzleState other)
        {
            if (other == null || other.Board != Board) return false;
            for (int i = 0; i < _edges.Length; i++)
            {
                if (_edges[i] != other._edges[i]) return false;
            }
            for (int i = 0; i < _paint.Length; i++)
            {
                if (_paint[i] != other._paint[i]) return false;
            }
            return true;
        }

        private int RequireIndex(Edge edge)
        {
            int index = Board.IndexOf(edge);
            if (index < 0) throw new ArgumentException($"Edge {edge} is not on this board.");
            return index;
        }

        private int RequireIndex(Cell cell)
        {
            int index = Board.IndexOf(cell);
            if (index < 0) throw new ArgumentException($"Cell {cell} is not on this board.");
            return index;
        }
    }
}
