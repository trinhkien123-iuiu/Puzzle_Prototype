using System.Collections.Generic;
using Puzzle.Presentation.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Screens
{
    /// <summary>Khung giải thích luật phía trên bàn: tên luật, một câu mô tả và hàng hình minh hoạ. Câm.</summary>
    public sealed class RuleLegendView : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;

        [SerializeField, Tooltip("Hàng chứa hình minh hoạ (có HorizontalLayoutGroup).")]
        private RectTransform _iconRow;

        [SerializeField] private ShapeIconView _iconPrefab;

        private ComponentPool<ShapeIconView> _pool;

        public void SetText(string title, string description)
        {
            _title.text = title;
            _description.text = description;
        }

        /// <summary>Lấy đủ <paramref name="count"/> ô hình minh hoạ; presenter tự vẽ nội dung từng ô.</summary>
        public IReadOnlyList<ShapeIconView> PrepareIcons(int count)
        {
            _pool ??= new ComponentPool<ShapeIconView>(_iconPrefab, _iconRow);
            _pool.ReleaseAll();
            var icons = new List<ShapeIconView>(count);
            for (int i = 0; i < count; i++) icons.Add(_pool.Get());

            // Layout group chỉ cập nhật cuối frame; ép ngay để kích thước icon đúng khi presenter vẽ.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_iconRow);
            return icons;
        }
    }
}
