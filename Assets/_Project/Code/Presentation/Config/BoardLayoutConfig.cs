using UnityEngine;

namespace Puzzle.Presentation.Config
{
    /// <summary>Kích thước và bố cục bàn chơi. Đơn vị là đơn vị canvas (canvas tham chiếu 1080 rộng).</summary>
    [CreateAssetMenu(menuName = "Puzzle/Config/Board Layout", fileName = "BoardLayoutConfig")]
    public sealed class BoardLayoutConfig : ScriptableObject
    {
        [SerializeField, Tooltip("Cỡ ô lớn nhất, để bàn nhỏ không phình to quá mức.")]
        private float _maxCellSize = 200f;

        [SerializeField, Tooltip("Khe giữa hai ô; cạnh Joined lấp khe bằng màu vùng.")]
        private float _cellGap = 8f;

        [SerializeField, Tooltip("Độ dày nét ranh giới.")]
        private float _borderThickness = 14f;

        [SerializeField, Tooltip("Cỡ ô tối thiểu theo dp (Android ~48dp). Nhỏ hơn thì cảnh báo trong Console.")]
        private float _minCellDp = 48f;

        public float MaxCellSize => _maxCellSize;
        public float CellGap => _cellGap;
        public float BorderThickness => _borderThickness;
        public float MinCellDp => _minCellDp;
    }
}
