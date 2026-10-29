namespace Puzzle.Domain.Rules
{
    /// <summary>
    /// Mức độ lỗi. Tách hai mức để View không tô đỏ khi người chơi đang vẽ dở.
    /// </summary>
    public enum Severity
    {
        /// <summary>Chưa thoả nhưng còn sửa được bằng cách quyết định thêm cạnh Unknown.</summary>
        Incomplete = 0,

        /// <summary>Sai chắc chắn với các cạnh đã đặt; phải gỡ bớt mới đúng được.</summary>
        Broken = 1,
    }
}
