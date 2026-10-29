using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Rules;
using Puzzle.Domain.Rules.ShapeBank;
using Puzzle.Domain.Shapes;

namespace Puzzle.Tests
{
    /// <summary>Puzzle nhỏ tự vẽ dùng chung cho test tầng Application.</summary>
    internal static class TestPuzzles
    {
        /// <summary>Lời giải của <see cref="TwoLs"/>: bàn 4x2 chia thành hai hình L.</summary>
        public static readonly string[] TwoLsSolution =
        {
            "AAAB",
            "ABBB",
        };

        /// <summary>Ranh giới cho sẵn trong đề <see cref="TwoLs"/> (giữa ô (2,0) và (3,0)).</summary>
        public static readonly Edge GivenEdge = TestBoards.EdgeBetween(2, 0, 3, 0);

        public static PuzzleDefinition TwoLs(bool withGivenBorder = false)
        {
            Polyomino l = Polyomino.Parse(
                "#.",
                "#.",
                "##");
            IRule bank = new ShapeBankRule(new[] { l });
            var given = withGivenBorder ? new[] { GivenEdge } : new Edge[0];
            return new PuzzleDefinition(
                TestBoards.ShapeOf(TwoLsSolution),
                new[] { bank },
                given,
                TestBoards.BordersOf(TwoLsSolution));
        }

        public static List<Cell> Cells(params (int X, int Y)[] points)
        {
            var cells = new List<Cell>(points.Length);
            foreach ((int x, int y) in points) cells.Add(new Cell(x, y));
            return cells;
        }
    }
}
