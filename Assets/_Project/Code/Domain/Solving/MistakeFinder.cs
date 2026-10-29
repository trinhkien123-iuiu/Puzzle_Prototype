using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Domain.Solving
{
    /// <summary>
    /// So trạng thái với lời giải có sẵn để tìm cạnh người chơi đã đặt sai.
    /// Cạnh Unknown không bao giờ là lỗi: người chơi chưa quyết định thì chưa sai.
    /// </summary>
    public sealed class MistakeFinder
    {
        private readonly PuzzleDefinition _definition;

        public MistakeFinder(PuzzleDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>Cạnh đã đặt (Border/Joined) ngược với lời giải.</summary>
        public bool IsMistake(PuzzleState state, Edge edge)
        {
            EdgeState current = state.GetEdge(edge);
            return current != EdgeState.Unknown && current != _definition.SolutionStateOf(edge);
        }

        /// <summary>Xoá rồi điền <paramref name="mistakes"/> theo thứ tự cạnh của bàn.</summary>
        public void Collect(PuzzleState state, List<Edge> mistakes)
        {
            mistakes.Clear();
            foreach (Edge edge in state.Board.Edges)
            {
                if (IsMistake(state, edge)) mistakes.Add(edge);
            }
        }
    }
}
