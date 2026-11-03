using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Application.Levels;
using Puzzle.Application.Progress;

namespace Puzzle.Tests.Levels
{
    public class LevelSelectorTests
    {
        private sealed class FakeLevels : ILevelList
        {
            private readonly List<string> _ids;
            public FakeLevels(params string[] ids) => _ids = new List<string>(ids);
            public int Count => _ids.Count;
            public string IdAt(int index) => _ids[index];
            public int IndexOf(string levelId) => _ids.IndexOf(levelId);
        }

        [Test]
        public void NewPlayer_StartsAtFirstLevel()
        {
            Assert.AreEqual(0, new LevelSelector(new FakeLevels("a", "b"), new PlayerProgress()).FirstToPlay());
        }

        [Test]
        public void SkipsCompletedLevels()
        {
            var progress = new PlayerProgress();
            progress.MarkCompleted("a");

            Assert.AreEqual(1, new LevelSelector(new FakeLevels("a", "b", "c"), progress).FirstToPlay());
        }

        [Test]
        public void PuzzleInProgress_WinsOverFirstUncompleted()
        {
            var progress = new PlayerProgress { InProgress = new SavedPuzzle { LevelId = "c", Edges = "0" } };

            Assert.AreEqual(2, new LevelSelector(new FakeLevels("a", "b", "c"), progress).FirstToPlay());
        }

        [Test]
        public void InProgressLevelRemovedFromCatalog_FallsBackToFirstUncompleted()
        {
            var progress = new PlayerProgress { InProgress = new SavedPuzzle { LevelId = "gone", Edges = "0" } };

            Assert.AreEqual(0, new LevelSelector(new FakeLevels("a", "b"), progress).FirstToPlay());
        }

        [Test]
        public void AllCompleted_StartsOver()
        {
            var progress = new PlayerProgress();
            progress.MarkCompleted("a");
            progress.MarkCompleted("b");

            Assert.AreEqual(0, new LevelSelector(new FakeLevels("a", "b"), progress).FirstToPlay());
        }
    }
}
