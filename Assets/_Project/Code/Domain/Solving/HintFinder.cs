using System;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Domain.Solving
{
    /// <summary>
    /// Chọn một cạnh để gợi ý dựa trên lời giải có sẵn trong level.
    /// Ưu tiên sửa cạnh đặt sai trước, vì gợi ý thêm ranh giới mới trên nền một chỗ sai chỉ làm người chơi rối hơn.
    /// </summary>
    public sealed class HintFinder
    {
        private readonly PuzzleDefinition _definition;
        private readonly MistakeFinder _mistakes;

        public HintFinder(PuzzleDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _mistakes = new MistakeFinder(definition);
        }

        /// <summary>
        /// Tìm cạnh tiếp theo nên đổi và trạng thái đúng của nó.
        /// Trả false khi không còn gì để gợi ý (mọi ranh giới của lời giải đã có và không cạnh nào sai).
        /// </summary>
        public bool TryFind(PuzzleState state, out Edge edge, out EdgeState target)
        {
            if (TryFindFirst(state, missingBorder: false, out edge) ||
                TryFindFirst(state, missingBorder: true, out edge))
            {
                target = _definition.SolutionStateOf(edge);
                return true;
            }
            target = EdgeState.Unknown;
            return false;
        }

        // Cạnh Unknown mà lời giải là Joined thì không cần gợi ý: Unknown vốn đã nối hai ô.
        private bool TryFindFirst(PuzzleState state, bool missingBorder, out Edge found)
        {
            foreach (Edge edge in state.Board.Edges)
            {
                bool matches = missingBorder
                    ? state.GetEdge(edge) == EdgeState.Unknown && _definition.SolutionStateOf(edge) == EdgeState.Border
                    : _mistakes.IsMistake(state, edge);
                if (!matches) continue;
                found = edge;
                return true;
            }
            found = default;
            return false;
        }
    }
}
