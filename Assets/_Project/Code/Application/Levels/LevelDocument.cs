using System.Collections.Generic;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Levels
{
    /// <summary>Kết quả đọc một file level: thông tin hiển thị và đề bài.</summary>
    public sealed class LevelDocument
    {
        public string Id { get; }
        public int Difficulty { get; }
        public PuzzleDefinition Puzzle { get; }

        /// <summary>Luật của level ở dạng dữ liệu, cùng thứ tự với <see cref="PuzzleDefinition.Rules"/>.</summary>
        public IReadOnlyList<RuleSpec> RuleSpecs { get; }

        /// <summary>Câu hướng dẫn hiện trên băng-rôn khi vào level (trống nếu level không có).</summary>
        public string Tip { get; }

        public LevelDocument(string id, int difficulty, PuzzleDefinition puzzle, IReadOnlyList<RuleSpec> ruleSpecs,
            string tip)
        {
            Tip = tip ?? string.Empty;
            Id = id;
            Difficulty = difficulty;
            Puzzle = puzzle;
            RuleSpecs = ruleSpecs;
        }
    }
}
