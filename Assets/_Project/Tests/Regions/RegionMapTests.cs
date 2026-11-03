using NUnit.Framework;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;

namespace Puzzle.Tests.Regions
{
    public class RegionMapTests
    {
        [Test]
        public void FreshBoard_IsOneOpenRegionPerCell()
        {
            var state = new PuzzleState(BoardShape.Rectangle(3, 3));

            var map = new RegionMap(state);

            Assert.AreEqual(9, map.Count);
            Assert.AreEqual(2, map.RegionOf(new Cell(0, 0)).OpenSides);
            Assert.AreEqual(4, map.RegionOf(new Cell(1, 1)).OpenSides);
            foreach (Region region in map.Regions) Assert.IsFalse(region.IsClosed);
        }

        [Test]
        public void Borders_SplitBoardIntoLabelledRegions()
        {
            PuzzleState state = TestBoards.StateOf(
                "AAB",
                "CAB",
                "CCB");

            var map = new RegionMap(state);

            Assert.AreEqual(3, map.Count);
            Assert.AreEqual(map.RegionIdOf(new Cell(0, 0)), map.RegionIdOf(new Cell(1, 1)));
            Assert.AreNotEqual(map.RegionIdOf(new Cell(0, 0)), map.RegionIdOf(new Cell(0, 1)));
            Assert.AreEqual(3, map.RegionOf(new Cell(2, 2)).Area);
            foreach (Region region in map.Regions) Assert.IsTrue(region.IsClosed);
        }

        [Test]
        public void SameLabelSplitByOtherRegion_IsTwoRegions()
        {
            // Hai chữ A không liền nhau nên phải là hai vùng khác nhau.
            PuzzleState state = TestBoards.StateOf("ABA");

            var map = new RegionMap(state);

            Assert.AreEqual(3, map.Count);
        }

        [Test]
        public void UnknownEdge_SeparatesCells_AndCountsAsOpenSide()
        {
            PuzzleState state = TestBoards.StateOf(
                "AB",
                "AB");
            state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Unknown);

            var map = new RegionMap(state);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(1, map.Regions[0].OpenSides);
            Assert.AreEqual(1, map.Regions[1].OpenSides);
        }

        [Test]
        public void BorderOnly_DoesNotMerge_JoinedDoes()
        {
            var state = new PuzzleState(BoardShape.Rectangle(2, 1));
            state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border);
            Assert.AreEqual(2, new RegionMap(state).Count);

            state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Joined);
            Assert.AreEqual(1, new RegionMap(state).Count);
        }

        [Test]
        public void HoleInBoard_CanSeparateRegionsWithoutBorders()
        {
            var state = new PuzzleState(TestBoards.ShapeOf(". ."));

            var map = new RegionMap(state);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(-1, map.RegionIdOf(new Cell(1, 0)));
            Assert.IsNull(map.RegionOf(new Cell(1, 0)));
        }

        [Test]
        public void RegionIds_FollowReadingOrder()
        {
            PuzzleState state = TestBoards.StateOf(
                "BA",
                "CC");

            var map = new RegionMap(state);

            Assert.AreEqual(0, map.RegionIdOf(new Cell(0, 0)));
            Assert.AreEqual(1, map.RegionIdOf(new Cell(1, 0)));
            Assert.AreEqual(2, map.RegionIdOf(new Cell(0, 1)));
        }

        [Test]
        public void RegionShape_IsItsPolyomino()
        {
            PuzzleState state = TestBoards.StateOf(
                "AB",
                "AA");

            var map = new RegionMap(state);

            Assert.AreEqual(3, map.RegionOf(new Cell(0, 0)).Shape.Area);
            Assert.AreEqual(1, map.RegionOf(new Cell(1, 0)).Shape.Area);
        }
    }
}
