using System;
using Puzzle.Application.Progress;

namespace Puzzle.Application.Levels
{
    /// <summary>Chọn level để mở khi vào game: ván đang dở, nếu không thì level đầu tiên chưa qua.</summary>
    public sealed class LevelSelector
    {
        private readonly ILevelList _levels;
        private readonly PlayerProgress _progress;

        public LevelSelector(ILevelList levels, PlayerProgress progress)
        {
            _levels = levels ?? throw new ArgumentNullException(nameof(levels));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public int FirstToPlay()
        {
            if (_progress.HasPuzzleInProgress)
            {
                int saved = _levels.IndexOf(_progress.InProgress.LevelId);
                if (saved >= 0) return saved;
            }
            for (int i = 0; i < _levels.Count; i++)
            {
                if (!_progress.IsCompleted(_levels.IdAt(i))) return i;
            }
            return 0;
        }
    }
}
