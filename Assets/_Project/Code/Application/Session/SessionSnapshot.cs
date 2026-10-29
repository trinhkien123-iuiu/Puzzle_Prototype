using System.Collections.Generic;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Rules;

namespace Puzzle.Application.Session
{
    /// <summary>
    /// Ảnh chụp chỉ đọc sau mỗi thay đổi: View vẽ từ đây, không hỏi lại session.
    /// <see cref="State"/> là bản sao riêng, nên lỡ có ai sửa nó cũng không ảnh hưởng ván đang chơi.
    /// </summary>
    public sealed class SessionSnapshot
    {
        public PuzzleState State { get; }
        public RegionMap Regions { get; }
        public IReadOnlyList<RuleViolation> Violations { get; }

        /// <summary>Không còn lỗi nào của mọi luật.</summary>
        public bool IsSolved { get; }

        /// <summary>Có lỗi chắc chắn sai: View tô đỏ; chỉ có lỗi Incomplete thì không.</summary>
        public bool HasBroken { get; }

        public bool CanUndo { get; }
        public bool CanRedo { get; }

        public SessionSnapshot(PuzzleState state, RegionMap regions, IReadOnlyList<RuleViolation> violations,
            bool canUndo, bool canRedo)
        {
            State = state;
            Regions = regions;
            Violations = violations;
            IsSolved = violations.Count == 0;
            HasBroken = PuzzleValidator.HasBroken(violations);
            CanUndo = canUndo;
            CanRedo = canRedo;
        }
    }
}
