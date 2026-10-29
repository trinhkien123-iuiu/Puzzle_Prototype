using System;
using Puzzle.Domain.Grid;

namespace Puzzle.Domain.Puzzle
{
    /// <summary>
    /// Nguồn sự thật duy nhất của một ván: trạng thái từng cạnh.
    /// Mọi thứ khác (vùng, lỗi) tính ra từ đây. Chỉ Command ở tầng Application được gọi <see cref="SetEdge"/>.
    /// </summary>
    public sealed class PuzzleState
    {
        // Mảng theo chỉ số cạnh của BoardShape: nhỏ, sao chép rẻ, không cấp phát khi đọc.
        private readonly EdgeState[] _edges;

        public BoardShape Board { get; }

        public PuzzleState(BoardShape board)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            _edges = new EdgeState[board.Edges.Count];
        }

        private PuzzleState(BoardShape board, EdgeState[] edges)
        {
            Board = board;
            _edges = edges;
        }

        public EdgeState GetEdge(Edge edge) => _edges[RequireIndex(edge)];

        public EdgeState GetEdge(int edgeIndex) => _edges[edgeIndex];

        public void SetEdge(Edge edge, EdgeState state)
        {
            _edges[RequireIndex(edge)] = state;
        }

        /// <summary>Bản sao độc lập, cho solver hoặc để so sánh trước/sau trong test command.</summary>
        public PuzzleState Clone() => new PuzzleState(Board, (EdgeState[])_edges.Clone());

        public bool SameEdgesAs(PuzzleState other)
        {
            if (other == null || other.Board != Board) return false;
            for (int i = 0; i < _edges.Length; i++)
            {
                if (_edges[i] != other._edges[i]) return false;
            }
            return true;
        }

        private int RequireIndex(Edge edge)
        {
            int index = Board.IndexOf(edge);
            if (index < 0) throw new ArgumentException($"Edge {edge} is not on this board.");
            return index;
        }
    }
}
