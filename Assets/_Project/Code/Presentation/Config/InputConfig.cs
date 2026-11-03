using UnityEngine;

namespace Puzzle.Presentation.Config
{
    /// <summary>Ngưỡng cử chỉ chạm. Ngưỡng bắt đầu kéo dùng pixelDragThreshold của EventSystem.</summary>
    [CreateAssetMenu(menuName = "Puzzle/Config/Input", fileName = "InputConfig")]
    public sealed class InputConfig : ScriptableObject
    {
        [SerializeField, Tooltip("Giữ ngón bao lâu (giây) thì tính là nhấn giữ (xoá vùng).")]
        private float _longPressSeconds = 0.45f;

        [SerializeField, Range(0.1f, 0.5f), Tooltip("Chạm cách cạnh trong khoảng này (tỉ lệ cỡ ô) thì tính là chạm cạnh.")]
        private float _edgeHitRatio = 0.3f;

        public float LongPressSeconds => _longPressSeconds;
        public float EdgeHitRatio => _edgeHitRatio;
    }
}
