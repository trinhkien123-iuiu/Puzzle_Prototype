using System;
using Puzzle.Domain.Grid;
using UnityEngine;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Phép đổi toạ độ giữa lưới và mặt phẳng của khung bàn (gốc ở tâm khung, Y hướng lên như uGUI).
    /// Không phải MonoBehaviour nên test được; vẽ và nhận chạm dùng chung một phép đổi để luôn khớp nhau.
    /// </summary>
    public sealed class BoardLayout
    {
        private readonly BoardShape _board;

        public float CellSize { get; }
        public Vector2 Size => new Vector2(_board.Width * CellSize, _board.Height * CellSize);

        /// <param name="available">Kích thước khung chứa bàn.</param>
        /// <param name="maxCellSize">Cỡ ô lớn nhất, để bàn nhỏ không phình quá to.</param>
        public BoardLayout(BoardShape board, Vector2 available, float maxCellSize)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            float fitWidth = available.x / Mathf.Max(1, board.Width);
            float fitHeight = available.y / Mathf.Max(1, board.Height);
            CellSize = Mathf.Max(1f, Mathf.Min(maxCellSize, Mathf.Min(fitWidth, fitHeight)));
        }

        public Vector2 CellCenter(Cell cell)
        {
            Vector2 size = Size;
            return new Vector2(-size.x / 2f + (cell.X + 0.5f) * CellSize, size.y / 2f - (cell.Y + 0.5f) * CellSize);
        }

        /// <summary>Hình chữ nhật của ô (tâm, kích thước), thu nhỏ đi <paramref name="gap"/> để lộ khe giữa các ô.</summary>
        public Rect CellRect(Cell cell, float gap)
        {
            float side = CellSize - gap;
            Vector2 center = CellCenter(cell);
            return new Rect(center.x - side / 2f, center.y - side / 2f, side, side);
        }

        /// <summary>
        /// Hình chữ nhật nét cạnh nằm giữa hai ô, dày <paramref name="thickness"/>, dài bằng cạnh ô cộng
        /// <paramref name="extraLength"/> (dương để các nét ranh giới nối liền ở góc, âm để chỉ lấp khe giữa hai ô).
        /// </summary>
        public Rect EdgeRect(Edge edge, float thickness, float extraLength)
        {
            Vector2 center = (CellCenter(edge.A) + CellCenter(edge.B)) / 2f;
            float length = CellSize + extraLength;
            Vector2 size = edge.IsVertical ? new Vector2(thickness, length) : new Vector2(length, thickness);
            return new Rect(center - size / 2f, size);
        }

        public bool TryGetCell(Vector2 point, out Cell cell)
        {
            Vector2 grid = ToGrid(point);
            cell = new Cell(Mathf.FloorToInt(grid.x), Mathf.FloorToInt(grid.y));
            return grid.x >= 0f && grid.y >= 0f && _board.Contains(cell);
        }

        /// <summary>
        /// Cạnh gần điểm chạm nếu điểm nằm trong dải <paramref name="hitRatio"/> (tỉ lệ cỡ ô) quanh cạnh đó.
        /// Cạnh mỏng hơn ngón tay nhiều, nên phải cho chạm "gần đúng" thay vì đòi chạm trúng nét.
        /// </summary>
        public bool TryGetEdge(Vector2 point, float hitRatio, out Edge edge)
        {
            edge = default;
            if (!TryGetCell(point, out Cell cell)) return false;

            Vector2 grid = ToGrid(point);
            float u = grid.x - cell.X;
            float v = grid.y - cell.Y;
            float[] distances = { u, 1f - u, v, 1f - v };
            int side = 0;
            for (int i = 1; i < distances.Length; i++)
            {
                if (distances[i] < distances[side]) side = i;
            }
            if (distances[side] > hitRatio) return false;

            Cell neighbour = side switch
            {
                0 => cell.Offset(-1, 0),
                1 => cell.Offset(1, 0),
                2 => cell.Offset(0, -1),
                _ => cell.Offset(0, 1),
            };
            if (!_board.Contains(neighbour)) return false;
            edge = new Edge(cell, neighbour);
            return true;
        }

        // Toạ độ lưới liên tục: (0,0) là góc trên trái của ô (0,0), Y hướng xuống.
        private Vector2 ToGrid(Vector2 point)
        {
            Vector2 size = Size;
            return new Vector2((point.x + size.x / 2f) / CellSize, (size.y / 2f - point.y) / CellSize);
        }
    }
}
