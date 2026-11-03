using System;
using System.Collections.Generic;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Shapes;

namespace Puzzle.Domain.Rules.ShapeBank
{
    /// <summary>
    /// Luật Shape Bank: mọi vùng phải là một trong các hình cho sẵn, được xoay và lật.
    /// </summary>
    public sealed class ShapeBankRule : IRule
    {
        public const string Id = "shape-bank";

        // Lưu dạng chính tắc của bank một lần, để mỗi lần kiểm tra chỉ là một phép tra HashSet.
        private readonly HashSet<Polyomino> _canonicalBank = new HashSet<Polyomino>();
        private readonly Polyomino[] _shapes;
        private readonly int _maxArea;

        public IReadOnlyList<Polyomino> Shapes => _shapes;

        public ShapeBankRule(IEnumerable<Polyomino> shapes)
        {
            if (shapes == null) throw new ArgumentNullException(nameof(shapes));
            _shapes = new List<Polyomino>(shapes).ToArray();
            if (_shapes.Length == 0) throw new ArgumentException("Shape bank needs at least one shape.");

            foreach (Polyomino shape in _shapes)
            {
                _canonicalBank.Add(new ShapeVariants(shape).Canonical);
                _maxArea = Math.Max(_maxArea, shape.Area);
            }
        }

        public void Check(in RuleContext ctx, List<RuleViolation> violations)
        {
            foreach (Region region in ctx.Regions.Regions)
            {
                if (_canonicalBank.Contains(new ShapeVariants(region.Shape).Canonical)) continue;
                violations.Add(new RuleViolation(Id, SeverityFor(region), region.Id, region.Cells));
            }
        }

        // Vùng chỉ có thể lớn thêm (gộp thêm ô), không tự nhỏ đi. Nên vùng đã to bằng hình lớn nhất
        // mà vẫn sai hình, hoặc vùng không còn cạnh mở để gộp, là sai chắc chắn (phải gỡ bớt).
        // Vùng nhỏ hơn mà còn cạnh mở thì người chơi vẫn đang gộp dở, chỉ là chưa xong.
        private Severity SeverityFor(Region region)
        {
            if (region.Area >= _maxArea) return Severity.Broken;
            return region.IsClosed ? Severity.Broken : Severity.Incomplete;
        }
    }
}
