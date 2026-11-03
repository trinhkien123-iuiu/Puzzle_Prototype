using UnityEngine;

namespace Puzzle.Presentation.Screens
{
    /// <summary>
    /// Co RectTransform vào vùng an toàn của màn hình (tránh tai thỏ, thanh điều hướng).
    /// Kiểm tra mỗi frame vì safe area đổi khi xoay máy hoặc bật/tắt thanh điều hướng; chỉ so sánh, không cấp phát.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect _applied;
        private Vector2Int _screen;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != _applied || Screen.width != _screen.x || Screen.height != _screen.y) Apply();
        }

        private void Apply()
        {
            _applied = Screen.safeArea;
            _screen = new Vector2Int(Screen.width, Screen.height);
            if (_screen.x <= 0 || _screen.y <= 0) return;

            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = new Vector2(_applied.xMin / _screen.x, _applied.yMin / _screen.y);
            rectTransform.anchorMax = new Vector2(_applied.xMax / _screen.x, _applied.yMax / _screen.y);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
