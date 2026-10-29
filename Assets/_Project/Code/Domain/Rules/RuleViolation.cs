using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;

namespace Puzzle.Domain.Rules
{
    /// <summary>
    /// Một lỗi do luật báo: luật nào, nặng tới đâu, ở vùng/ô/cạnh nào.
    /// Dùng id dạng chuỗi thay vì tham chiếu lớp luật để Presentation tra hình ảnh
    /// theo dữ liệu (ClueVisuals) mà không cần kiểm tra kiểu luật.
    /// </summary>
    public sealed class RuleViolation
    {
        private static readonly Cell[] NoCells = Array.Empty<Cell>();
        private static readonly Edge[] NoEdges = Array.Empty<Edge>();

        public string RuleId { get; }
        public Severity Severity { get; }

        /// <summary>Id vùng trong <see cref="Regions.RegionMap"/>; -1 nếu lỗi không gắn với một vùng.</summary>
        public int RegionId { get; }

        public IReadOnlyList<Cell> Cells { get; }
        public IReadOnlyList<Edge> Edges { get; }

        public RuleViolation(string ruleId, Severity severity, int regionId,
            IReadOnlyList<Cell> cells = null, IReadOnlyList<Edge> edges = null)
        {
            if (string.IsNullOrEmpty(ruleId)) throw new ArgumentException("Rule id is required.", nameof(ruleId));
            RuleId = ruleId;
            Severity = severity;
            RegionId = regionId;
            Cells = cells ?? NoCells;
            Edges = edges ?? NoEdges;
        }

        public override string ToString() => $"{RuleId} {Severity} region {RegionId}";
    }
}
