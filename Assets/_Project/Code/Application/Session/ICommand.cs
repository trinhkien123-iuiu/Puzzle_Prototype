using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Session
{
    /// <summary>
    /// Một thao tác của người chơi (hoặc booster) trên trạng thái puzzle.
    /// Hợp đồng: gọi <see cref="Undo"/> ngay sau <see cref="Do"/> phải trả state về đúng như cũ,
    /// và gọi lại <see cref="Do"/> sau <see cref="Undo"/> (redo) phải cho cùng kết quả.
    /// </summary>
    public interface ICommand
    {
        void Do(PuzzleState state);
        void Undo(PuzzleState state);
    }
}
