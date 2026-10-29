using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Tests
{
    /// <summary>
    /// Dựng bàn và trạng thái từ hình vẽ chữ để test đọc được bằng mắt.
    /// Mỗi ký tự là một ô; cùng chữ = cùng vùng; khoảng trắng = không có ô.
    /// Cạnh giữa hai chữ khác nhau là Border, cùng chữ là Joined.
    /// </summary>
    internal static class TestBoards
    {
        public static BoardShape ShapeOf(params string[] rows)
        {
            var cells = new List<Cell>();
            for (int y = 0; y < rows.Length; y++)
            {
                for (int x = 0; x < rows[y].Length; x++)
                {
                    if (rows[y][x] != ' ') cells.Add(new Cell(x, y));
                }
            }
            return new BoardShape(cells);
        }

        public static PuzzleState StateOf(params string[] rows)
        {
            BoardShape board = ShapeOf(rows);
            var state = new PuzzleState(board);
            foreach (Edge edge in board.Edges)
            {
                bool sameLabel = LabelAt(rows, edge.A) == LabelAt(rows, edge.B);
                state.SetEdge(edge, sameLabel ? EdgeState.Joined : EdgeState.Border);
            }
            return state;
        }

        /// <summary>Danh sách cạnh Border trong hình vẽ, dùng làm lời giải cho PuzzleDefinition.</summary>
        public static List<Edge> BordersOf(params string[] rows)
        {
            var borders = new List<Edge>();
            foreach (Edge edge in ShapeOf(rows).Edges)
            {
                if (LabelAt(rows, edge.A) != LabelAt(rows, edge.B)) borders.Add(edge);
            }
            return borders;
        }

        public static Edge EdgeBetween(int ax, int ay, int bx, int by)
        {
            return new Edge(new Cell(ax, ay), new Cell(bx, by));
        }

        private static char LabelAt(string[] rows, Cell cell) => rows[cell.Y][cell.X];
    }
}
