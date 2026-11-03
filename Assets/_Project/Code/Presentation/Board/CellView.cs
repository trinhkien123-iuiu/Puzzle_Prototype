using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Board
{
    /// <summary>Một ô trên bàn. Câm: chỉ đặt vị trí, màu và phần hoạ tiết kính.</summary>
    public sealed class CellView : MonoBehaviour
    {
        [SerializeField, Tooltip("Hình nền ô, đổi màu theo vùng. RawImage thì nhận thêm toạ độ hoạ tiết để kính liền qua các ô.")]
        private Graphic _fill;

        [SerializeField, Tooltip("Tuỳ chọn: gửi mặt nạ mép vùng cho shader kính.")]
        private GlassCellMesh _glass;

        public void Place(Rect rect)
        {
            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = rect.center;
            rectTransform.sizeDelta = rect.size;
        }

        public void SetColor(Color color) => _fill.color = color;

        /// <summary>Đổi sprite của ô (khối màu, ô trống); null thì giữ sprite sẵn có của prefab.</summary>
        public void SetSprite(Sprite sprite)
        {
            if (sprite != null && _fill is Image image) image.sprite = sprite;
        }

        /// <summary>Phần hoạ tiết ô này hiện; các ô cạnh nhau lấy phần nối tiếp nên vân kính chạy liền cả vùng.</summary>
        public void SetUv(Rect uv)
        {
            if (_fill is RawImage raw) raw.uvRect = uv;
        }

        /// <summary>Cạnh và góc lõm của ô nằm trên mép vùng (bit như GlassCellMesh.SetMasks).</summary>
        public void SetEdgeMasks(int sides, int corners)
        {
            if (_glass != null) _glass.SetMasks(sides, corners);
        }
    }
}
