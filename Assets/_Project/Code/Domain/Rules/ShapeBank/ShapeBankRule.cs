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
        private readonly int _minArea = int.MaxValue;

        public IReadOnlyList<Polyomino> Shapes => _shapes;

        public ShapeBankRule(IEnumerable<Polyomino> shapes)
        {
            if (shapes == null) throw new ArgumentNullException(nameof(shapes));
            _shapes = new List<Polyomino>(shapes).ToArray();
            if (_shapes.Length == 0) throw new ArgumentException("Shape bank needs at least one shape.");

            foreach (Polyomino shape in _shapes)
            {
                _canonicalBank.Add(new ShapeVariants(shape).Canonical);
                _minArea = Math.Min(_minArea, shape.Area);
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

        // Vùng đã bị ranh giới bao kín thì chỉ có thể nhỏ đi khi cắt thêm, không thể lớn lên.
        // Nên vùng nhỏ hơn hình nhỏ nhất của bank là sai chắc chắn; vùng sai hình mà còn cạnh
        // Unknown bên trong thì người chơi vẫn cắt tiếp được, chỉ là chưa xong.
        private Severity SeverityFor(Region region)
        {
            if (region.Area < _minArea) return Severity.Broken;
            return region.IsSettled ? Severity.Broken : Severity.Incomplete;
        }
    }
}
