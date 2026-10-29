using System;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;

namespace Puzzle.Domain.Rules
{
    /// <summary>
    /// Mọi thứ một luật được phép đọc. Luật cần thêm dữ liệu thì thêm vào đây,
    /// không để luật tự đi lấy, nhờ vậy luật dễ test và không phụ thuộc lẫn nhau.
    /// </summary>
    public readonly struct RuleContext
    {
        public BoardShape Board => State.Board;
        public PuzzleState State { get; }
        public RegionMap Regions { get; }

        public RuleContext(PuzzleState state, RegionMap regions)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            Regions = regions ?? throw new ArgumentNullException(nameof(regions));
        }
    }
}
