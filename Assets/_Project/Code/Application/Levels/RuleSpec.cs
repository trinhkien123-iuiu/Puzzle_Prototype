using System.Collections.Generic;
using Puzzle.Domain.Shapes;

namespace Puzzle.Application.Levels
{
    /// <summary>
    /// Mô tả một luật của level dưới dạng dữ liệu (id + tham số) để màn hình giải thích luật cho người chơi.
    /// Presentation tra hình ảnh/chữ theo <see cref="RuleId"/>, không cần biết lớp luật cụ thể.
    /// </summary>
    public sealed class RuleSpec
    {
        public string RuleId { get; }
        public IReadOnlyList<Polyomino> Shapes { get; }

        public RuleSpec(string ruleId, IReadOnlyList<Polyomino> shapes)
        {
            RuleId = ruleId;
            Shapes = shapes;
        }
    }
}
