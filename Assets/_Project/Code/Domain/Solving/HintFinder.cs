using System;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Domain.Solving
{
    /// <summary>
    /// Chọn một cạnh để gợi ý dựa trên lời giải có sẵn trong level.
    /// Ưu tiên sửa cạnh đặt sai trước, vì gợi ý gộp thêm trên nền một chỗ sai chỉ làm người chơi rối hơn.
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
        /// Trả false khi không còn gì để gợi ý (mọi cặp ô cần gộp đã gộp và không cạnh nào sai).
        /// </summary>
        public bool TryFind(PuzzleState state, out Edge edge, out EdgeState target)
        {
            if (TryFindFirst(state, missingJoin: false, out edge) ||
                TryFindFirst(state, missingJoin: true, out edge))
            {
                target = _definition.SolutionStateOf(edge);
                return true;
            }
            target = EdgeState.Unknown;
            return false;
        }

        // Cạnh Unknown mà lời giải là Border thì không cần gợi ý: ranh giới chỉ là đánh dấu phụ,
        // Unknown vốn đã tách hai ô. Chỉ cặp ô cần gộp mới là bước còn thiếu.
        private bool TryFindFirst(PuzzleState state, bool missingJoin, out Edge found)
        {
            foreach (Edge edge in state.Board.Edges)
            {
                bool matches = missingJoin
                    ? state.GetEdge(edge) == EdgeState.Unknown && _definition.SolutionStateOf(edge) == EdgeState.Joined
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
