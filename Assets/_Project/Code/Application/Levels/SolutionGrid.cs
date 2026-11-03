using System.Collections.Generic;
using Puzzle.Domain.Grid;

namespace Puzzle.Application.Levels
{
    /// <summary>
    /// Lưới lời giải viết bằng chữ: mỗi ký tự một ô, '.' là không có ô, cùng ký tự liền nhau là cùng vùng.
    /// Từ đó suy ra hình bàn và các cạnh ranh giới (hai ô kề nhau khác ký tự).
    /// </summary>
    internal sealed class SolutionGrid
    {
        public const char NoCell = '.';

        public BoardShape Board { get; }
        public List<Edge> Borders { get; } = new List<Edge>();

        public SolutionGrid(IReadOnlyList<string> rows)
        {
            var cells = new List<Cell>();
            for (int y = 0; y < rows.Count; y++)
            {
                for (int x = 0; x < rows[y].Length; x++)
                {
                    if (rows[y][x] != NoCell) cells.Add(new Cell(x, y));
                }
            }
            Board = new BoardShape(cells);

            foreach (Edge edge in Board.Edges)
            {
                if (rows[edge.A.Y][edge.A.X] != rows[edge.B.Y][edge.B.X]) Borders.Add(edge);
            }
        }
    }
}
