namespace Puzzle.Presentation.Board
{
    /// <summary>Cách vẽ một cạnh.</summary>
    public enum EdgeLook
    {
        /// <summary>Không vẽ gì.</summary>
        Hidden,

        /// <summary>Nét liền (ranh giới, hoặc lấp khe giữa hai ô cùng vùng).</summary>
        Solid,

        /// <summary>Đường chấm giữa hai ô chưa gộp.</summary>
        Dotted,
    }
}
