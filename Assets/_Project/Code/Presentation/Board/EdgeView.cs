using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Board
{
    /// <summary>Nét một cạnh giữa hai ô. Câm: chỉ đặt vị trí, màu và kiểu nét.</summary>
    public sealed class EdgeView : MonoBehaviour
    {
        [SerializeField, Tooltip("Nét liền: ranh giới, hoặc lấp khe giữa hai ô cùng vùng.")]
        private Image _line;

        [SerializeField, Tooltip("Nét chấm (Image kiểu Tiled): khe giữa hai ô chưa gộp.")]
        private Image _dots;

        public void Place(Rect rect, bool vertical)
        {
            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = rect.center;

            // Hoạ tiết chấm chạy theo chiều ngang của sprite, nên cạnh dọc xoay 90° thay vì cần sprite thứ hai.
            rectTransform.localRotation = vertical ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
            rectTransform.sizeDelta = vertical ? new Vector2(rect.height, rect.width) : rect.size;
        }

        public void Show(EdgeLook look, Color color)
        {
            _line.enabled = look == EdgeLook.Solid;
            _dots.enabled = look == EdgeLook.Dotted;
            _line.color = color;
            _dots.color = color;
        }
    }
}
