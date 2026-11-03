namespace Puzzle.Presentation.Screens
{
    /// <summary>Thông tin tĩnh của level cho HUD (không đổi trong lúc chơi).</summary>
    public readonly struct HudInfo
    {
        public string Title { get; }
        public int Difficulty { get; }

        /// <summary>Câu hướng dẫn của level; trống thì HUD dùng câu mặc định.</summary>
        public string Tip { get; }

        public HudInfo(string title, int difficulty, string tip)
        {
            Title = title;
            Difficulty = difficulty;
            Tip = tip;
        }
    }
}
