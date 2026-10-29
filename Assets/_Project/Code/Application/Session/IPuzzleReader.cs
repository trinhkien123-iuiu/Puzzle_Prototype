using System;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Session
{
    /// <summary>Phần chỉ đọc của một ván, cho View/Presenter và booster.</summary>
    public interface IPuzzleReader
    {
        PuzzleDefinition Definition { get; }
        SessionSnapshot Snapshot { get; }

        /// <summary>Phát đúng một lần sau mỗi command, undo hoặc redo có làm đổi state.</summary>
        event Action<SessionSnapshot> Changed;
    }
}
