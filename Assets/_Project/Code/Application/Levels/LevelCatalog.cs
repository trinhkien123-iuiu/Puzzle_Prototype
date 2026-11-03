using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Application.Levels
{
    /// <summary>Thứ tự level người chơi đi qua. Đổi thứ tự = kéo thả trong Inspector, không sửa code.</summary>
    [CreateAssetMenu(menuName = "Puzzle/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject, ILevelList
    {
        [SerializeField, Tooltip("Level theo thứ tự chơi.")]
        private List<LevelAsset> _levels = new List<LevelAsset>();

        public IReadOnlyList<LevelAsset> Levels => _levels;

        public int Count => _levels.Count;

        public string IdAt(int index) => _levels[index] != null ? _levels[index].Id : null;

        /// <summary>Vị trí level theo id; -1 nếu không có.</summary>
        public int IndexOf(string levelId)
        {
            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levels[i] != null && _levels[i].Id == levelId) return i;
            }
            return -1;
        }

        /// <summary>Chỉ menu Editor dùng để điền danh sách khi dựng greybox.</summary>
        public void ReplaceLevels(IEnumerable<LevelAsset> levels)
        {
            _levels = new List<LevelAsset>(levels);
        }
    }
}
