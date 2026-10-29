using System;
using Puzzle.Application.Commands;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Solving;

namespace Puzzle.Application.Boosters
{
    /// <summary>
    /// Gợi ý: đưa một cạnh về đúng lời giải có sẵn trong level (sửa cạnh sai trước, rồi mới thêm ranh giới còn thiếu).
    /// </summary>
    public sealed class HintBooster : IBooster
    {
        public bool CanUse(IPuzzleReader puzzle)
        {
            return !puzzle.Snapshot.IsSolved && TryFind(puzzle, out _, out _);
        }

        public BoosterResult Use(IPuzzleReader puzzle)
        {
            if (!TryFind(puzzle, out Edge edge, out EdgeState target))
            {
                throw new InvalidOperationException("Hint booster used when there is nothing to hint.");
            }
            var command = new SetEdgeCommand(puzzle.Definition, edge, target);
            return new BoosterResult(command, new[] { edge.A, edge.B });
        }

        // Tạo HintFinder mỗi lần vì booster sống qua nhiều level; chi phí nhỏ và chỉ chạy khi người chơi bấm.
        private static bool TryFind(IPuzzleReader puzzle, out Edge edge, out EdgeState target)
        {
            return new HintFinder(puzzle.Definition).TryFind(puzzle.Snapshot.State, out edge, out target);
        }
    }
}
