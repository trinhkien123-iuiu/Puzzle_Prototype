using System;
using System.Collections.Generic;
using Puzzle.Presentation.Pooling;
using UnityEngine;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Bàn chơi trên màn hình. Câm: sinh ô/cạnh từ prefab qua pool rồi đặt vị trí, màu theo lệnh của BoardPresenter.
    /// Không đọc input, không biết luật.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField, Tooltip("Lớp chứa ô (vẽ dưới).")]
        private RectTransform _cellLayer;

        [SerializeField, Tooltip("Lớp chứa cạnh (vẽ trên ô).")]
        private RectTransform _edgeLayer;

        [SerializeField, Tooltip("Lớp chứa viền ô (vẽ dưới cùng); để trống thì bàn không có viền ngoài.")]
        private RectTransform _outlineLayer;

        [SerializeField] private CellView _cellPrefab;
        [SerializeField] private CellView _outlinePrefab;
        [SerializeField] private EdgeView _edgePrefab;

        private readonly List<CellView> _cells = new List<CellView>();
        private readonly List<CellView> _outlines = new List<CellView>();
        private readonly List<EdgeView> _edges = new List<EdgeView>();
        private ComponentPool<CellView> _cellPool;
        private ComponentPool<CellView> _outlinePool;
        private ComponentPool<EdgeView> _edgePool;

        private bool HasOutline => _outlineLayer != null && _outlinePrefab != null;

        /// <summary>Khung bàn đổi kích thước (xoay máy, safe area đổi): cần dựng lại bố cục.</summary>
        public event Action Resized;

        /// <summary>Bố cục đang dùng để vẽ; BoardInput dùng chung để đổi điểm chạm ra ô/cạnh.</summary>
        public BoardLayout Layout { get; private set; }

        public Vector2 AvailableSize => ((RectTransform)transform).rect.size;

        /// <summary>Hệ số đơn vị canvas → pixel màn hình, để kiểm tra cỡ ô theo dp.</summary>
        public float CanvasScale
        {
            get
            {
                var canvas = GetComponentInParent<Canvas>();
                return canvas != null ? canvas.scaleFactor : 1f;
            }
        }

        public void Build(BoardLayout layout, int cellCount, int edgeCount)
        {
            _cellPool ??= new ComponentPool<CellView>(_cellPrefab, _cellLayer);
            _edgePool ??= new ComponentPool<EdgeView>(_edgePrefab, _edgeLayer);
            _cellPool.ReleaseAll();
            _edgePool.ReleaseAll();
            _cells.Clear();
            _edges.Clear();
            _outlines.Clear();

            for (int i = 0; i < cellCount; i++) _cells.Add(_cellPool.Get());
            for (int i = 0; i < edgeCount; i++) _edges.Add(_edgePool.Get());
            if (HasOutline)
            {
                _outlinePool ??= new ComponentPool<CellView>(_outlinePrefab, _outlineLayer);
                _outlinePool.ReleaseAll();
                for (int i = 0; i < cellCount; i++) _outlines.Add(_outlinePool.Get());
            }
            Layout = layout;
        }

        public void ShowCell(int index, Rect rect, Color color, Sprite sprite, Rect uv, int edgeSides, int edgeCorners)
        {
            _cells[index].Place(rect);
            _cells[index].SetColor(color);
            _cells[index].SetSprite(sprite);
            _cells[index].SetUv(uv);
            _cells[index].SetEdgeMasks(edgeSides, edgeCorners);
        }

        /// <summary>Tấm nền tối to hơn ô một chút; các tấm chồng lên nhau nên chỉ lộ ra thành viền quanh mép bàn.</summary>
        public void ShowCellOutline(int index, Rect rect, Color color)
        {
            if (!HasOutline) return;
            _outlines[index].Place(rect);
            _outlines[index].SetColor(color);
        }

        public void ShowEdge(int index, Rect rect, bool vertical, EdgeLook look, Color color)
        {
            _edges[index].Place(rect, vertical);
            _edges[index].Show(look, color);
        }

        private void OnRectTransformDimensionsChange() => Resized?.Invoke();
    }
}
