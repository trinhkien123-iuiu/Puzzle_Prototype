using System;
using System.Collections.Generic;
using Puzzle.Domain.Rules;

namespace Puzzle.Domain.Puzzle
{
    /// <summary>
    /// Chạy lần lượt các luật của một puzzle và gom lỗi. Không chứa luật nào,
    /// nên thêm luật mới không bao giờ phải sửa lớp này.
    /// </summary>
    public sealed class PuzzleValidator
    {
        private readonly IReadOnlyList<IRule> _rules;

        public PuzzleValidator(IReadOnlyList<IRule> rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>
        /// Xoá rồi điền <paramref name="violations"/> bằng lỗi của mọi luật.
        /// Truyền list vào thay vì trả list mới để người gọi tái sử dụng, không cấp phát mỗi lần đặt cạnh.
        /// </summary>
        /// <returns>true khi không có lỗi nào, tức puzzle đã giải xong.</returns>
        public bool Validate(in RuleContext ctx, List<RuleViolation> violations)
        {
            if (violations == null) throw new ArgumentNullException(nameof(violations));
            violations.Clear();
            for (int i = 0; i < _rules.Count; i++)
            {
                _rules[i].Check(ctx, violations);
            }
            return violations.Count == 0;
        }

        /// <summary>Có lỗi Broken không: dùng để quyết định tô đỏ hay chỉ nhắc nhẹ.</summary>
        public static bool HasBroken(IReadOnlyList<RuleViolation> violations)
        {
            for (int i = 0; i < violations.Count; i++)
            {
                if (violations[i].Severity == Severity.Broken) return true;
            }
            return false;
        }
    }
}
