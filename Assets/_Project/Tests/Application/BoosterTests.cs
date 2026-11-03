using NUnit.Framework;
using Puzzle.Application.Boosters;
using Puzzle.Application.Commands;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Tests.Application
{
    public class BoosterTests
    {
        private PuzzleDefinition _definition;
        private PuzzleSession _session;

        [SetUp]
        public void SetUp()
        {
            _definition = TestPuzzles.TwoLs();
            _session = new PuzzleSession(_definition);
        }

        [Test]
        public void Hint_FixesAMistakeBeforeAddingNewBorders()
        {
            Edge wrong = TestBoards.EdgeBetween(0, 0, 1, 0); // lời giải: Joined
            _session.Execute(new SetEdgeCommand(_definition, wrong, EdgeState.Border));
            var hint = new HintBooster();

            BoosterResult result = hint.Use(_session);
            _session.Execute(result.Command);

            Assert.AreEqual(EdgeState.Joined, _session.Snapshot.State.GetEdge(wrong));
            CollectionAssert.AreEquivalent(new[] { wrong.A, wrong.B }, result.HighlightedCells);
        }

        [Test]
        public void RepeatedHints_SolveThePuzzle_ThenHintIsUnavailable()
        {
            var hint = new HintBooster();
            int guard = _definition.Board.Edges.Count;

            while (hint.CanUse(_session) && guard-- > 0)
            {
                Assert.IsTrue(_session.Execute(hint.Use(_session).Command));
            }

            Assert.IsTrue(_session.Snapshot.IsSolved);
            Assert.IsFalse(hint.CanUse(_session));
        }

        [Test]
        public void HintCommand_IsUndoable()
        {
            _session.Execute(new HintBooster().Use(_session).Command);

            _session.Undo();

            Assert.IsTrue(_session.Snapshot.State.SameAs(_definition.CreateInitialState()));
        }

        [Test]
        public void Check_HighlightsOnlyTheRegionWithAMistake_AndChangesNothing()
        {
            _session = new PuzzleSession(_definition, _definition.CreateSolutionState());
            // Ranh giới thừa cắt vùng A làm đôi (ô (0,0) và (1,0) cùng vùng trong lời giải).
            _session.Execute(new SetEdgeCommand(_definition, TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border));
            PuzzleState before = _session.Snapshot.State;

            BoosterResult result = new CheckBooster().Use(_session);

            Assert.IsNull(result.Command);
            CollectionAssert.AreEquivalent(
                TestPuzzles.Cells((0, 0), (1, 0), (2, 0), (0, 1)),
                result.HighlightedCells);
            Assert.AreSame(before, _session.Snapshot.State);
        }

        [Test]
        public void Check_WithNoMistakes_HighlightsNothing()
        {
            _session.Execute(new SetEdgeCommand(_definition, _definition.SolutionBorders[0], EdgeState.Border));

            BoosterResult result = new CheckBooster().Use(_session);

            Assert.IsEmpty(result.HighlightedCells);
        }

        [Test]
        public void Boosters_AreUnavailableOnceSolved()
        {
            var solved = new PuzzleSession(_definition, _definition.CreateSolutionState());

            Assert.IsFalse(new HintBooster().CanUse(solved));
            Assert.IsFalse(new CheckBooster().CanUse(solved));
        }
    }
}
