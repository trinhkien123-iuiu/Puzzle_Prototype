namespace Puzzle.Domain.Grid
{
    /// <summary>
    /// Trạng thái người chơi đặt cho một cạnh.
    /// Unknown vẫn nối hai ô khi tính vùng (người chơi chia bàn bằng cách vẽ ranh giới),
    /// nhưng luật dùng nó để phân biệt "chưa xong" với "sai chắc chắn".
    /// </summary>
    public enum EdgeState
    {
        Unknown = 0,
        Border = 1,
        Joined = 2,
    }
}
