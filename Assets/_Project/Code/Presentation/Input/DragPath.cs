using System.Collections.Generic;
using Puzzle.Domain.Grid;

namespace Puzzle.Presentation.Input
{
    /// <summary>
    /// Đường ngón tay đi qua các ô khi kéo. Ngón lướt nhanh có thể nhảy cóc vài ô giữa hai frame,
    /// nên tự điền các ô ở giữa để đường luôn liền mạch; kéo lùi về ô trước thì bỏ ô cuối (cho phép "rút lại").
    /// </summary>
    public sealed class DragPath
    {
        private readonly List<Cell> _cells = new List<Cell>();

        public IReadOnlyList<Cell> Cells => _cells;

        public void Start(Cell cell)
        {
            _cells.Clear();
            _cells.Add(cell);
        }

        public void Clear() => _cells.Clear();

        /// <summary>Nối đường tới ô mới. Trả true nếu đường thay đổi.</summary>
        public bool ExtendTo(Cell target)
        {
            if (_cells.Count == 0)
            {
                Start(target);
                return true;
            }

            bool changed = false;
            while (_cells[_cells.Count - 1] != target)
            {
                StepTowards(target);
                changed = true;
            }
            return changed;
        }

        // Đi từng ô một: theo trục X trước rồi Y, để mỗi bước luôn là hai ô kề nhau.
        private void StepTowards(Cell target)
        {
            Cell last = _cells[_cells.Count - 1];
            int dx = System.Math.Sign(target.X - last.X);
            Cell next = dx != 0 ? last.Offset(dx, 0) : last.Offset(0, System.Math.Sign(target.Y - last.Y));

            bool isBacktrack = _cells.Count >= 2 && _cells[_cells.Count - 2] == next;
            if (isBacktrack) _cells.RemoveAt(_cells.Count - 1);
            else _cells.Add(next);
        }
    }
}
