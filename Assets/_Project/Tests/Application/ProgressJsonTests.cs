#if UNITY_2017_1_OR_NEWER
using NUnit.Framework;
using Puzzle.Application.Progress;
using UnityEngine;

namespace Puzzle.Tests.Application
{
    /// <summary>
    /// Kiểm tra dữ liệu tiến độ đi qua được JsonUtility, serializer mà kho PlayerPrefs sẽ dùng.
    /// Chỉ chạy trong Unity (ngoài Unity không có JsonUtility).
    /// </summary>
    public class ProgressJsonTests
    {
        [Test]
        public void PlayerProgress_RoundTripsThroughJsonUtility()
        {
            var progress = new PlayerProgress();
            progress.MarkCompleted("level-1");
            progress.InProgress = new SavedPuzzle { LevelId = "level-2", Edges = "0120" };

            string json = JsonUtility.ToJson(progress);
            var loaded = JsonUtility.FromJson<PlayerProgress>(json);

            CollectionAssert.AreEqual(new[] { "level-1" }, loaded.CompletedLevelIds);
            Assert.AreEqual("level-2", loaded.InProgress.LevelId);
            Assert.AreEqual("0120", loaded.InProgress.Edges);
        }

        [Test]
        public void EmptyProgress_StaysWithoutPuzzleInProgress_AfterJson()
        {
            var loaded = JsonUtility.FromJson<PlayerProgress>(JsonUtility.ToJson(new PlayerProgress()));

            Assert.IsFalse(loaded.HasPuzzleInProgress);
        }
    }
}
#endif
