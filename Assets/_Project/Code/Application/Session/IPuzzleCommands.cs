namespace Puzzle.Application.Session
{
    /// <summary>Phần thay đổi một ván, cho Input và Presenter của booster.</summary>
    public interface IPuzzleCommands
    {
        /// <summary>Chạy command; trả false (và không ghi lịch sử) khi command không đổi gì.</summary>
        bool Execute(ICommand command);

        bool Undo();
        bool Redo();
    }
}
