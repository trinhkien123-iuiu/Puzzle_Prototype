using System.Collections.Generic;
using Puzzle.Application.Progress;

namespace Puzzle.Tests.Application
{
    /// <summary>
    /// Kho tiến độ giả cho test. Sao chép sâu khi lưu/đọc để giống một kho thật
    /// (dữ liệu đã lưu không đổi theo object người gọi giữ).
    /// </summary>
    internal sealed class InMemoryProgressStore : IProgressStore
    {
        private PlayerProgress _saved;

        public PlayerProgress Load() => _saved == null ? new PlayerProgress() : Copy(_saved);

        public void Save(PlayerProgress progress) => _saved = Copy(progress);

        private static PlayerProgress Copy(PlayerProgress source)
        {
            return new PlayerProgress
            {
                CompletedLevelIds = new List<string>(source.CompletedLevelIds),
                InProgress = source.InProgress == null
                    ? new SavedPuzzle()
                    : new SavedPuzzle { LevelId = source.InProgress.LevelId, Edges = source.InProgress.Edges },
            };
        }
    }
}
