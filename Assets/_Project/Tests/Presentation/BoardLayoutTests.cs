#if UNITY_2017_1_OR_NEWER
using NUnit.Framework;
using Puzzle.Domain.Grid;
using Puzzle.Presentation.Board;
using UnityEngine;

namespace Puzzle.Tests.Presentation
{
    public class BoardLayoutTests
    {
        // Bàn 4x2 trong khung 400x400: ô 100, bàn chiếm x ∈ [-200, 200], y ∈ [-100, 100].
        private static BoardLayout Layout() => new BoardLayout(BoardShape.Rectangle(4, 2), new Vector2(400, 400), 500);

        [Test]
        public void CellSize_FitsTheTighterAxis_AndRespectsMaximum()
        {
            Assert.AreEqual(100f, Layout().CellSize);
            Assert.AreEqual(60f, new BoardLayout(BoardShape.Rectangle(4, 2), new Vector2(400, 400), 60).CellSize);
        }

        [Test]
        public void CellCenter_RowZeroIsAtTheTop()
        {
            Assert.AreEqual(new Vector2(-150, 50), Layout().CellCenter(new Cell(0, 0)));
            Assert.AreEqual(new Vector2(150, -50), Layout().CellCenter(new Cell(3, 1)));
        }

        [Test]
        public void TryGetCell_MapsPointsBackToCells()
        {
            Assert.IsTrue(Layout().TryGetCell(new Vector2(-150, 50), out Cell cell));
            Assert.AreEqual(new Cell(0, 0), cell);
            Assert.IsFalse(Layout().TryGetCell(new Vector2(-250, 0), out _));
        }

        [Test]
        public void TapNearSharedSide_HitsThatEdge()
        {
            // x = -105 nằm trong ô (0,0), cách cạnh phải 5 đơn vị (0.05 cỡ ô).
            Assert.IsTrue(Layout().TryGetEdge(new Vector2(-105, 50), 0.25f, out Edge edge));
            Assert.AreEqual(TestBoards.EdgeBetween(0, 0, 1, 0), edge);
        }

        [Test]
        public void TapInCellCentre_OrOnBoardOutline_HitsNoEdge()
        {
            Assert.IsFalse(Layout().TryGetEdge(new Vector2(-150, 50), 0.25f, out _));
            Assert.IsFalse(Layout().TryGetEdge(new Vector2(-195, 50), 0.25f, out _));
        }

        [Test]
        public void HoleInBoard_IsNotACell()
        {
            BoardShape board = TestBoards.ShapeOf(". .");
            var layout = new BoardLayout(board, new Vector2(300, 100), 500);

            Assert.IsFalse(layout.TryGetCell(new Vector2(0, 0), out _));
        }
    }
}
#endif
