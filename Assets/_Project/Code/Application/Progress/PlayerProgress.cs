using System;
using System.Collections.Generic;

namespace Puzzle.Application.Progress
{
    /// <summary>
    /// Toàn bộ tiến độ người chơi cần lưu: level đã qua và ván đang chơi dở.
    /// Dữ liệu thuần, field public để serialize được; quy tắc lưu khi nào nằm ở nơi gọi <see cref="IProgressStore"/>.
    /// </summary>
    [Serializable]
    public sealed class PlayerProgress
    {
        public List<string> CompletedLevelIds = new List<string>();

        /// <summary>Ván dở. Serializer của Unity không giữ null, nên kiểm tra bằng <see cref="HasPuzzleInProgress"/>.</summary>
        public SavedPuzzle InProgress = new SavedPuzzle();

        public bool HasPuzzleInProgress => InProgress != null && !InProgress.IsEmpty;

        public bool IsCompleted(string levelId) => CompletedLevelIds.Contains(levelId);

        /// <summary>Ghi nhận qua màn và bỏ ván dở của level đó (nếu có).</summary>
        public void MarkCompleted(string levelId)
        {
            if (!IsCompleted(levelId)) CompletedLevelIds.Add(levelId);
            if (HasPuzzleInProgress && InProgress.LevelId == levelId) InProgress = new SavedPuzzle();
        }
    }
}
