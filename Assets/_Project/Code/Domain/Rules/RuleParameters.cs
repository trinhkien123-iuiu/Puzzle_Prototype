using System;
using System.Collections.Generic;
using Puzzle.Domain.Shapes;

namespace Puzzle.Domain.Rules
{
    /// <summary>
    /// Tham số đọc từ dữ liệu level để tạo một luật qua <see cref="RuleRegistry"/>.
    /// Gom chung một kiểu để registry có một chữ ký hàm tạo duy nhất; luật nào không dùng trường nào thì bỏ qua.
    /// </summary>
    public sealed class RuleParameters
    {
        public static readonly RuleParameters Empty = new RuleParameters(null);

        /// <summary>Danh sách hình (Shape Bank và các luật về hình).</summary>
        public IReadOnlyList<Polyomino> Shapes { get; }

        public RuleParameters(IReadOnlyList<Polyomino> shapes)
        {
            Shapes = shapes ?? Array.Empty<Polyomino>();
        }
    }
}
