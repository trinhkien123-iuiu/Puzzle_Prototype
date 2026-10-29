using System.Collections.Generic;

namespace Puzzle.Domain.Rules
{
    /// <summary>
    /// Một luật puzzle. Chỉ đọc trạng thái, không bao giờ sửa.
    /// Hợp đồng chung: không ném exception với trạng thái dở dang, chỉ thêm lỗi vào
    /// <paramref name="violations"/> (không xoá), và chạy nhanh (không tự giải puzzle).
    /// </summary>
    public interface IRule
    {
        void Check(in RuleContext ctx, List<RuleViolation> violations);
    }
}
