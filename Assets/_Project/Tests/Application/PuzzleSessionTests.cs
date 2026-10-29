using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Application.Commands;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Tests.Application
{
    public class PuzzleSessionTests
    {
        private PuzzleDefinition _definition;
        private PuzzleSession _session;
        private List<SessionSnapshot> _published;

        [SetUp]
        public void SetUp()
        {
            _definition = TestPuzzles.TwoLs(withGivenBorder: true);
            _session = new PuzzleSession(_definition);
            _published = new List<SessionSnapshot>();
            _session.Changed += _published.Add;
        }

        [Test]
        public void NewSession_StartsFromGivenBorders_Unsolved()
        {
            Assert.AreEqual(EdgeState.Border, _session.Snapshot.State.GetEdge(TestPuzzles.GivenEdge));
            Assert.IsFalse(_session.Snapshot.IsSolved);
            Assert.IsFalse(_session.Snapshot.CanUndo);
        }

        [Test]
        public void Execute_PublishesExactlyOneSnapshot()
        {
            bool changed = _session.Execute(Border(0, 0, 1, 0));

            Assert.IsTrue(changed);
            Assert.AreEqual(1, _published.Count);
            Assert.AreSame(_session.Snapshot, _published[0]);
            Assert.IsTrue(_published[0].CanUndo);
        }

        [Test]
        public void CommandThatChangesNothing_IsIgnored()
        {
            // Chạm vào cạnh cho sẵn: không đổi state, không vào lịch sử, không phát sự kiện.
            bool changed = _session.Execute(SetEdgeCommand.ToggleBorder(_definition, _session.Snapshot.State, TestPuzzles.GivenEdge));

            Assert.IsFalse(changed);
            Assert.IsEmpty(_published);
            Assert.IsFalse(_session.Snapshot.CanUndo);
        }

        [Test]
        public void UndoRedo_RestoreStates_AndPublish()
        {
            Edge edge = TestBoards.EdgeBetween(0, 0, 1, 0);
            _session.Execute(Border(0, 0, 1, 0));

            Assert.IsTrue(_session.Undo());
            Assert.AreEqual(EdgeState.Unknown, _session.Snapshot.State.GetEdge(edge));
            Assert.IsTrue(_session.Snapshot.CanRedo);

            Assert.IsTrue(_session.Redo());
            Assert.AreEqual(EdgeState.Border, _session.Snapshot.State.GetEdge(edge));
            Assert.AreEqual(3, _published.Count);
        }

        [Test]
        public void UndoOnce_RevertsAWholeDrag()
        {
            _session.Execute(new MergeCellsCommand(_definition, TestPuzzles.Cells((0, 0), (1, 0), (1, 1), (2, 1))));

            _session.Undo();

            Assert.IsTrue(_session.Snapshot.State.SameEdgesAs(_definition.CreateInitialState()));
            Assert.IsFalse(_session.Snapshot.CanUndo);
        }

        [Test]
        public void NewCommand_DropsRedoBranch()
        {
            _session.Execute(Border(0, 0, 1, 0));
            _session.Undo();

            _session.Execute(Border(0, 0, 0, 1));

            Assert.IsFalse(_session.Snapshot.CanRedo);
            Assert.IsFalse(_session.Redo());
        }

        [Test]
        public void UndoRedo_WithEmptyHistory_ReturnFalse_WithoutPublishing()
        {
            Assert.IsFalse(_session.Undo());
            Assert.IsFalse(_session.Redo());
            Assert.IsEmpty(_published);
        }

        [Test]
        public void DrawingEverySolutionBorder_SolvesThePuzzle()
        {
            foreach (Edge edge in _definition.SolutionBorders)
            {
                _session.Execute(new SetEdgeCommand(_definition, edge, EdgeState.Border));
            }

            Assert.IsTrue(_session.Snapshot.IsSolved);
            Assert.IsEmpty(_session.Snapshot.Violations);
        }

        [Test]
        public void SnapshotState_IsACopy()
        {
            _session.Snapshot.State.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border);

            _session.Execute(Border(0, 0, 0, 1));

            Assert.AreEqual(EdgeState.Unknown, _session.Snapshot.State.GetEdge(TestBoards.EdgeBetween(0, 0, 1, 0)));
        }

        [Test]
        public void Session_CanStartFromRestoredState()
        {
            var session = new PuzzleSession(_definition, _definition.CreateSolutionState());

            Assert.IsTrue(session.Snapshot.IsSolved);
            Assert.IsFalse(session.Snapshot.CanUndo);
        }

        private SetEdgeCommand Border(int ax, int ay, int bx, int by)
        {
            return new SetEdgeCommand(_definition, TestBoards.EdgeBetween(ax, ay, bx, by), EdgeState.Border);
        }
    }
}
