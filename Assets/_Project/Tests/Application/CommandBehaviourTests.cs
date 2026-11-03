using NUnit.Framework;
using Puzzle.Application.Commands;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Tests.Application
{
    public class CommandBehaviourTests
    {
        [Test]
        public void MergeDrag_JoinsEachConsecutivePair_AndSkipsGaps()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs();
            PuzzleState state = definition.CreateInitialState();

            // (1,0) → (3,0) không kề nhau: ngón lướt quá nhanh, cặp này bị bỏ qua.
            new MergeCellsCommand(definition, TestPuzzles.Cells((0, 0), (1, 0), (3, 0), (3, 1)), 5).Do(state);

            Assert.AreEqual(EdgeState.Joined, state.GetEdge(TestBoards.EdgeBetween(0, 0, 1, 0)));
            Assert.AreEqual(EdgeState.Joined, state.GetEdge(TestBoards.EdgeBetween(3, 0, 3, 1)));
            Assert.AreEqual(EdgeState.Unknown, state.GetEdge(TestBoards.EdgeBetween(1, 0, 2, 0)));
            Assert.AreEqual(5, state.GetPaint(new Cell(0, 0)));
            Assert.AreEqual(PuzzleState.NoPaint, state.GetPaint(new Cell(2, 0)));
        }

        [Test]
        public void MergeIntoPaintedRegion_RepaintsTheWholeRegion()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs();
            PuzzleState state = definition.CreateInitialState();
            new MergeCellsCommand(definition, TestPuzzles.Cells((0, 0), (1, 0)), 2).Do(state);

            new MergeCellsCommand(definition, TestPuzzles.Cells((1, 0), (2, 0)), 7).Do(state);

            foreach (Cell cell in TestPuzzles.Cells((0, 0), (1, 0), (2, 0))) Assert.AreEqual(7, state.GetPaint(cell));
        }

        [Test]
        public void ResetBoard_ClearsEverythingButGivenBorders()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            PuzzleState state = definition.CreateSolutionState();
            state.SetPaint(new Cell(0, 0), 3);

            new ResetBoardCommand(definition).Do(state);

            Assert.IsTrue(state.SameAs(definition.CreateInitialState()));
        }

        [Test]
        public void ToggleBorder_SetsThenClears()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs();
            PuzzleState state = definition.CreateInitialState();
            Edge edge = TestBoards.EdgeBetween(0, 0, 1, 0);

            SetEdgeCommand.ToggleBorder(definition, state, edge).Do(state);
            Assert.AreEqual(EdgeState.Border, state.GetEdge(edge));

            SetEdgeCommand.ToggleBorder(definition, state, edge).Do(state);
            Assert.AreEqual(EdgeState.Unknown, state.GetEdge(edge));
        }

        [Test]
        public void ClearRegion_ResetsInsideAndOutline_KeepsGivenBorder()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            PuzzleState state = definition.CreateSolutionState();

            // Ô (0,0) thuộc vùng A: mọi cạnh chạm vùng A về Unknown, trừ cạnh cho sẵn.
            new ClearRegionCommand(definition, new Cell(0, 0)).Do(state);

            Assert.AreEqual(EdgeState.Unknown, state.GetEdge(TestBoards.EdgeBetween(0, 0, 1, 0)));
            Assert.AreEqual(EdgeState.Unknown, state.GetEdge(TestBoards.EdgeBetween(1, 0, 1, 1)));
            Assert.AreEqual(EdgeState.Border, state.GetEdge(TestPuzzles.GivenEdge));
            Assert.AreEqual(EdgeState.Joined, state.GetEdge(TestBoards.EdgeBetween(2, 1, 3, 1)));
        }
    }
}
