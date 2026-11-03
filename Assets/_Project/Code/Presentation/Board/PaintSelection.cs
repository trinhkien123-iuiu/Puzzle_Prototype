using System;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Màu người chơi đang chọn để tô vùng (1-based, khớp PuzzleState.GetPaint). Trạng thái giao diện thuần,
    /// không nằm trong PuzzleState vì đổi màu cọ không phải một thao tác cần undo.
    /// </summary>
    public sealed class PaintSelection
    {
        private readonly int _count;

        public int Selected { get; private set; } = 1;

        public event Action<int> Changed;

        public PaintSelection(int count)
        {
            _count = Math.Max(1, count);
        }

        public void Select(int paint)
        {
            int clamped = Math.Min(_count, Math.Max(1, paint));
            if (clamped == Selected) return;
            Selected = clamped;
            Changed?.Invoke(Selected);
        }
    }
}
