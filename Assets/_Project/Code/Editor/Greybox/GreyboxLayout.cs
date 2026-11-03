namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Bố cục dọc của màn chơi (đơn vị canvas, canvas tham chiếu rộng 1080), đo từ mép safe area.
    /// Trên xuống: tiêu đề · băng-rôn hướng dẫn · kho hình · bàn · bảng màu · nút. Một chỗ để chỉnh cho mọi prefab.
    /// </summary>
    internal static class GreyboxLayout
    {
        public const float ReferenceWidth = 1080f;
        public const float ReferenceHeight = 1920f;
        public const float Margin = 30f;

        public const float TitleTop = 0f;
        public const float TitleHeight = 100f;
        public const float BannerTop = 105f;
        public const float BannerHeight = 190f;
        public const float LegendTop = 305f;
        public const float LegendHeight = 330f;
        public const float BoardTop = 650f;

        public const float ButtonsBottom = 20f;
        public const float ButtonsHeight = 150f;
        public const float PaletteBottom = 190f;
        public const float PaletteHeight = 130f;
        public const float BoardBottom = 335f;
    }
}
