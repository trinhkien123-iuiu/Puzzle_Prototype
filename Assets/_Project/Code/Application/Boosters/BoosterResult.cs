using System;
using System.Collections.Generic;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;

namespace Puzzle.Application.Boosters
{
    /// <summary>
    /// Kết quả dùng booster: một command để chạy (có thể không có) và các ô cần làm nổi bật.
    /// Tách hai phần vì có booster chỉ báo tin (Kiểm tra) và có booster đổi state (Gợi ý).
    /// </summary>
    public sealed class BoosterResult
    {
        /// <summary>Command cần đưa vào <see cref="IPuzzleCommands.Execute"/>; null nếu booster chỉ báo tin.</summary>
        public ICommand Command { get; }

        public IReadOnlyList<Cell> HighlightedCells { get; }

        public BoosterResult(ICommand command, IReadOnlyList<Cell> highlightedCells)
        {
            Command = command;
            HighlightedCells = highlightedCells ?? Array.Empty<Cell>();
        }
    }
}
