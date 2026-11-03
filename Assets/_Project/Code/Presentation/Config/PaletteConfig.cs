using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Presentation.Config
{
    /// <summary>Bảng màu của màn chơi: màu tô vùng người chơi chọn và màu các trạng thái của bàn.</summary>
    [CreateAssetMenu(menuName = "Puzzle/Config/Palette", fileName = "PaletteConfig")]
    public sealed class PaletteConfig : ScriptableObject
    {
        [SerializeField, Tooltip("Màu nền camera (phía sau mọi thứ).")]
        private Color _background = new Color(0.3f, 0.31f, 0.33f);

        [SerializeField, Tooltip("Các màu người chơi chọn để tô vùng, theo thứ tự trên bảng màu (tối đa 9).")]
        private List<Color> _paints = new List<Color>
        {
            new Color(0.98f, 0.82f, 0.25f), new Color(0.47f, 0.78f, 0.36f), new Color(0.29f, 0.78f, 0.6f),
            new Color(0.96f, 0.55f, 0.24f), new Color(0.93f, 0.42f, 0.55f), new Color(0.24f, 0.78f, 0.86f),
            new Color(0.86f, 0.26f, 0.3f), new Color(0.55f, 0.33f, 0.72f), new Color(0.27f, 0.42f, 0.86f),
        };

        [SerializeField, Tooltip("Ô chưa gộp vào vùng nào.")]
        private Color _unpaintedCell = new Color(0.95f, 0.94f, 0.92f);

        [SerializeField, Tooltip("Đường chấm giữa hai ô chưa quyết định.")]
        private Color _dots = new Color(0.45f, 0.42f, 0.38f, 0.8f);

        [SerializeField, Tooltip("Pha vào màu vùng sai chắc chắn.")]
        private Color _brokenRegion = new Color(0.95f, 0.08f, 0.08f);

        [SerializeField, Range(0f, 1f), Tooltip("Mức pha màu lỗi vào vùng sai.")]
        private float _brokenTint = 0.7f;

        [SerializeField, Tooltip("Ô được booster làm nổi bật.")]
        private Color _highlight = new Color(1f, 0.92f, 0.3f);

        [SerializeField, Tooltip("Ô trên đường đang kéo.")]
        private Color _dragPreview = new Color(1f, 0.97f, 0.8f);

        [SerializeField, Tooltip("Ranh giới kim loại người chơi đặt.")]
        private Color _border = new Color(0.16f, 0.17f, 0.2f);

        [SerializeField, Tooltip("Ranh giới cho sẵn của đề.")]
        private Color _givenBorder = Color.black;

        [SerializeField, Tooltip("Tuỳ chọn: sprite khối theo từng màu tô (cùng thứ tự với màu). Có sprite thì ô vẽ bằng sprite, không tô màu.")]
        private List<Sprite> _paintSprites = new List<Sprite>();

        [SerializeField, Tooltip("Bàn vẽ ô bằng sprite khối. Tắt thì bàn vẽ bằng màu (shader ngọc/kính liền vùng), bảng màu và kho hình vẫn dùng sprite.")]
        private bool _boardUsesSprites = true;

        [SerializeField, Tooltip("Tuỳ chọn: sprite ô trống chưa gộp vào vùng nào.")]
        private Sprite _emptySprite;

        [SerializeField, Tooltip("Tuỳ chọn: sprite ô trên đường đang kéo.")]
        private Sprite _previewSprite;

        public Color Background => _background;
        public Color UnpaintedCell => _unpaintedCell;
        public Color Dots => _dots;
        public Color Highlight => _highlight;
        public Color DragPreview => _dragPreview;
        public Color Border => _border;
        public Color GivenBorder => _givenBorder;
        public IReadOnlyList<Color> Paints => _paints;
        public IReadOnlyList<Sprite> PaintSprites => _paintSprites;
        public Sprite EmptySprite => _emptySprite;
        public bool BoardUsesSprites => _boardUsesSprites && _paintSprites.Count > 0;
        public Sprite PreviewSprite => _previewSprite;

        /// <summary>Sprite khối theo chỉ số màu 1-based; null nếu không có (khi đó vẽ bằng màu).</summary>
        public Sprite PaintSprite(int paint)
        {
            if (paint <= 0 || _paintSprites.Count == 0) return _emptySprite;
            return _paintSprites[(paint - 1) % _paintSprites.Count];
        }

        /// <summary>Sprite theo khoá bất kỳ, cùng cách chọn với <see cref="RegionColor"/>.</summary>
        public Sprite RegionSprite(int key)
        {
            if (_paints.Count == 0 || _paintSprites.Count == 0) return null;
            int index = key % _paints.Count;
            index = index < 0 ? index + _paints.Count : index;
            return _paintSprites[index % _paintSprites.Count];
        }

        /// <summary>Màu tô theo chỉ số 1-based (0 = chưa tô).</summary>
        public Color PaintColor(int paint)
        {
            if (paint <= 0 || _paints.Count == 0) return _unpaintedCell;
            return _paints[(paint - 1) % _paints.Count];
        }

        /// <summary>Màu tự chọn theo một khoá bất kỳ (vùng gộp bằng Gợi ý chưa có màu người chơi chọn).</summary>
        public Color RegionColor(int key)
        {
            if (_paints.Count == 0) return _unpaintedCell;
            int index = key % _paints.Count;
            return _paints[index < 0 ? index + _paints.Count : index];
        }

        public Color Broken(Color regionColor) => Color.Lerp(regionColor, _brokenRegion, _brokenTint);
    }
}
