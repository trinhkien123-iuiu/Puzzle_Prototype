using System.Collections.Generic;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Pooling;
using UnityEngine;

namespace Puzzle.Presentation.Screens
{
    /// <summary>Hình thu nhỏ của một polyomino trong khung luật. Câm: vẽ các ô theo toạ độ được đưa vào.</summary>
    public sealed class ShapeIconView : MonoBehaviour
    {
        [SerializeField, Tooltip("Lớp chứa các ô của hình.")]
        private RectTransform _cellLayer;

        [SerializeField] private CellView _cellPrefab;

        private ComponentPool<CellView> _pool;

        public Vector2 Size => _cellLayer.rect.size;

        /// <param name="sprite">Sprite khối cho mỗi ô; null thì vẽ bằng màu.</param>
        public void Show(IReadOnlyList<Rect> cells, Color color, Sprite sprite = null)
        {
            _pool ??= new ComponentPool<CellView>(_cellPrefab, _cellLayer);
            _pool.ReleaseAll();
            foreach (Rect rect in cells)
            {
                CellView cell = _pool.Get();
                cell.Place(rect);
                cell.SetColor(color);
                cell.SetSprite(sprite);
            }
        }
    }
}
