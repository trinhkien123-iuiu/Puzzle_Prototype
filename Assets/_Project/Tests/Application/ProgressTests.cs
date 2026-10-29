using NUnit.Framework;
using Puzzle.Application.Progress;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Tests.Application
{
    public class ProgressTests
    {
        private const string LevelId = "test-two-ls";

        [Test]
        public void SavedPuzzle_RoundTripsAPuzzleInProgress()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            PuzzleState state = definition.CreateInitialState();
            state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border);
            state.SetEdge(TestBoards.EdgeBetween(1, 0, 1, 1), EdgeState.Joined);

            SavedPuzzle saved = SavedPuzzle.Capture(LevelId, state);
            bool restored = saved.TryRestore(definition, out PuzzleState loaded);

            Assert.IsTrue(restored);
            Assert.IsTrue(loaded.SameEdgesAs(state));
            Assert.IsFalse(new PuzzleSession(definition, loaded).Snapshot.CanUndo);
        }

        [Test]
        public void SavedPuzzle_ForADifferentBoardSize_IsRejected()
        {
            var saved = new SavedPuzzle { LevelId = LevelId, Edges = "0120" };

            Assert.IsFalse(saved.TryRestore(TestPuzzles.TwoLs(), out PuzzleState state));
            Assert.IsNull(state);
        }

        [Test]
        public void SavedPuzzle_WithCorruptCharacters_IsRejected()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs();
            var saved = new SavedPuzzle { LevelId = LevelId, Edges = new string('9', definition.Board.Edges.Count) };

            Assert.IsFalse(saved.TryRestore(definition, out _));
        }

        [Test]
        public void SavedPuzzle_CannotRemoveAGivenBorder()
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            var allUnknown = new SavedPuzzle { LevelId = LevelId, Edges = new string('0', definition.Board.Edges.Count) };

            allUnknown.TryRestore(definition, out PuzzleState state);

            Assert.AreEqual(EdgeState.Border, state.GetEdge(TestPuzzles.GivenEdge));
        }

        [Test]
        public void MarkCompleted_RecordsLevelOnce_AndDropsItsSavedPuzzle()
        {
            var progress = new PlayerProgress
            {
                InProgress = SavedPuzzle.Capture(LevelId, TestPuzzles.TwoLs().CreateInitialState()),
            };

            progress.MarkCompleted(LevelId);
            progress.MarkCompleted(LevelId);

            Assert.IsTrue(progress.IsCompleted(LevelId));
            Assert.AreEqual(1, progress.CompletedLevelIds.Count);
            Assert.IsFalse(progress.HasPuzzleInProgress);
        }

        [Test]
        public void NewProgress_HasNoPuzzleInProgress()
        {
            Assert.IsFalse(new PlayerProgress().HasPuzzleInProgress);
        }
    }
}
