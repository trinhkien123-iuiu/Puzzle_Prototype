using UnityEngine;

namespace Puzzle.Application.Levels
{
    /// <summary>
    /// Một level trong project. Được importer tạo từ file <c>.pzl</c> (văn bản) nên nội dung sửa ở file đó,
    /// không sửa trong Inspector; asset chỉ để <see cref="LevelCatalog"/> tham chiếu được.
    /// </summary>
    public sealed class LevelAsset : ScriptableObject
    {
        [SerializeField, Tooltip("Id duy nhất, dùng làm khoá lưu tiến độ.")]
        private string _id;

        [SerializeField, Tooltip("Độ khó 1..5, chỉ để sắp xếp và hiển thị.")]
        private int _difficulty = 1;

        [SerializeField, TextArea(6, 30), Tooltip("Nội dung file .pzl (chỉ đọc; sửa file gốc).")]
        private string _source;

        public string Id => _id;
        public int Difficulty => _difficulty;
        public string Source => _source;

        /// <summary>Chỉ importer gọi khi tạo asset từ file .pzl.</summary>
        public void Configure(string id, int difficulty, string source)
        {
            _id = id;
            _difficulty = difficulty;
            _source = source;
        }
    }
}
