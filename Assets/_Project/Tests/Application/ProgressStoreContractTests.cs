using NUnit.Framework;
using Puzzle.Application.Progress;

namespace Puzzle.Tests.Application
{
    /// <summary>
    /// Hợp đồng chung cho mọi <see cref="IProgressStore"/>. Cài đặt thật (PlayerPrefs) ở Presentation
    /// sẽ có lớp con riêng chạy cùng bộ test này.
    /// </summary>
    public abstract class ProgressStoreContractTests
    {
        protected abstract IProgressStore CreateStore();

        [Test]
        public void EmptyStore_LoadsEmptyProgress_NotNull()
        {
            PlayerProgress progress = CreateStore().Load();

            Assert.IsNotNull(progress);
            Assert.IsEmpty(progress.CompletedLevelIds);
            Assert.IsFalse(progress.HasPuzzleInProgress);
        }

        [Test]
        public void SaveThenLoad_ReturnsEquivalentProgress()
        {
            IProgressStore store = CreateStore();
            var progress = new PlayerProgress();
            progress.MarkCompleted("level-1");
            progress.InProgress = new SavedPuzzle { LevelId = "level-2", Edges = "0120" };

            store.Save(progress);
            PlayerProgress loaded = store.Load();

            CollectionAssert.AreEqual(new[] { "level-1" }, loaded.CompletedLevelIds);
            Assert.AreEqual("level-2", loaded.InProgress.LevelId);
            Assert.AreEqual("0120", loaded.InProgress.Edges);
        }

        [Test]
        public void ChangingProgressAfterSave_DoesNotChangeWhatWasSaved()
        {
            IProgressStore store = CreateStore();
            var progress = new PlayerProgress();
            store.Save(progress);

            progress.MarkCompleted("level-1");

            Assert.IsEmpty(store.Load().CompletedLevelIds);
        }
    }

    public class InMemoryProgressStoreTests : ProgressStoreContractTests
    {
        protected override IProgressStore CreateStore() => new InMemoryProgressStore();
    }
}
