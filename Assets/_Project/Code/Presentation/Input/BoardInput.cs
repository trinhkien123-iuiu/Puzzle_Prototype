using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Config;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Puzzle.Presentation.Input
{
    /// <summary>
    /// Đổi cử chỉ một ngón trên bàn thành ý định: kéo qua ô, chạm cạnh, nhấn giữ ô.
    /// Dùng sự kiện con trỏ của EventSystem nên chạm (mobile) và chuột (Editor) đi chung một đường.
    /// Không vẽ, không biết luật, không tạo command.
    /// </summary>
    public sealed class BoardInput : MonoBehaviour,
        IPointerDownHandler, IBeginDragHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        private readonly DragPath _path = new DragPath();
        private BoardView _board;
        private InputConfig _config;
        private int _pointerId = NoPointer;
        private float _pressTime;
        private Vector2 _pressPoint;
        private bool _dragging;
        private bool _held;

        /// <summary>Nhấc ngón sau khi kéo qua ít nhất hai ô: cả đường kéo.</summary>
        public event Action<IReadOnlyList<Cell>> CellsDragged;

        /// <summary>Đường kéo thay đổi (để vẽ xem trước); danh sách rỗng khi kết thúc.</summary>
        public event Action<IReadOnlyList<Cell>> DragPreviewChanged;

        public event Action<Edge> EdgeTapped;
        public event Action<Cell> CellHeld;

        public void Construct(BoardView board, InputConfig config)
        {
            _board = board;
            _config = config;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pointerId != NoPointer || !TryLocalPoint(eventData, out Vector2 point)) return;
            _pointerId = eventData.pointerId;
            _pressTime = Time.unscaledTime;
            _pressPoint = point;
            _dragging = false;
            _held = false;
            _path.Clear();
            if (_board.Layout.TryGetCell(point, out Cell cell)) _path.Start(cell);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == _pointerId && !_held) _dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId || !_dragging) return;
            if (!TryLocalPoint(eventData, out Vector2 point)) return;
            if (_board.Layout.TryGetCell(point, out Cell cell) && _path.ExtendTo(cell))
            {
                DragPreviewChanged?.Invoke(_path.Cells);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _pointerId) return;
            _pointerId = NoPointer;
            if (_held) return;

            if (_dragging)
            {
                if (_path.Cells.Count >= 2) CellsDragged?.Invoke(_path.Cells);
                DragPreviewChanged?.Invoke(Array.Empty<Cell>());
            }
            else if (_board.Layout.TryGetEdge(_pressPoint, _config.EdgeHitRatio, out Edge edge))
            {
                EdgeTapped?.Invoke(edge);
            }
        }

        // Nhấn giữ phải bắn khi ngón còn đặt trên màn hình, nên kiểm tra theo thời gian mỗi frame.
        private void Update()
        {
            if (_pointerId == NoPointer || _dragging || _held) return;
            if (Time.unscaledTime - _pressTime < _config.LongPressSeconds) return;

            _held = true;
            if (_board.Layout.TryGetCell(_pressPoint, out Cell cell)) CellHeld?.Invoke(cell);
        }

        private bool TryLocalPoint(PointerEventData eventData, out Vector2 point)
        {
            point = default;
            if (_board == null || _board.Layout == null) return false;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_board.transform, eventData.position, eventData.pressEventCamera, out point);
        }
    }
}
