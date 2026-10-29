using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Rules;

namespace Puzzle.Domain.Puzzle
{
    /// <summary>
    /// Đề bài bất biến: bàn, luật, ranh giới cho sẵn và lời giải.
    /// Lời giải là bắt buộc để test dữ liệu tự kiểm tra level và để booster Gợi ý có đáp án.
    /// </summary>
    public sealed class PuzzleDefinition
    {
        private readonly IRule[] _rules;
        private readonly Edge[] _givenBorders;
        private readonly Edge[] _solutionBorders;

        public BoardShape Board { get; }
        public IReadOnlyList<IRule> Rules => _rules;

        /// <summary>Ranh giới có sẵn khi bắt đầu ván (người chơi không xoá được).</summary>
        public IReadOnlyList<Edge> GivenBorders => _givenBorders;

        /// <summary>Mọi cạnh là ranh giới trong lời giải; cạnh còn lại là Joined.</summary>
        public IReadOnlyList<Edge> SolutionBorders => _solutionBorders;

        public PuzzleDefinition(BoardShape board, IEnumerable<IRule> rules,
            IEnumerable<Edge> givenBorders, IEnumerable<Edge> solutionBorders)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            _rules = ToArray(rules, nameof(rules));
            _givenBorders = ToArray(givenBorders, nameof(givenBorders));
            _solutionBorders = ToArray(solutionBorders, nameof(solutionBorders));
            RequireOnBoard(_givenBorders);
            RequireOnBoard(_solutionBorders);
        }

        public PuzzleState CreateInitialState()
        {
            var state = new PuzzleState(Board);
            foreach (Edge edge in _givenBorders) state.SetEdge(edge, EdgeState.Border);
            return state;
        }

        /// <summary>Trạng thái đã giải hoàn toàn: không còn cạnh Unknown.</summary>
        public PuzzleState CreateSolutionState()
        {
            var state = new PuzzleState(Board);
            foreach (Edge edge in Board.Edges) state.SetEdge(edge, EdgeState.Joined);
            foreach (Edge edge in _solutionBorders) state.SetEdge(edge, EdgeState.Border);
            return state;
        }

        private void RequireOnBoard(Edge[] edges)
        {
            foreach (Edge edge in edges)
            {
                if (!Board.Contains(edge)) throw new ArgumentException($"Edge {edge} is not on the board.");
            }
        }

        private static T[] ToArray<T>(IEnumerable<T> items, string name)
        {
            if (items == null) throw new ArgumentNullException(name);
            return new List<T>(items).ToArray();
        }
    }
}
