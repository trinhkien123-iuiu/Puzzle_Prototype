using System.Collections.Generic;
using System.Text;
using Puzzle.Application.Levels;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Shapes;
using Puzzle.Presentation.Config;
using UnityEngine;

namespace Puzzle.Presentation.Screens
{
    /// <summary>
    /// Giải thích luật của level cho người chơi: chữ lấy từ <see cref="ClueVisuals"/> theo id luật,
    /// hình minh hoạ lấy từ tham số luật. Không kiểm tra kiểu luật nào.
    /// </summary>
    public sealed class RuleLegendPresenter
    {
        private const float IconPadding = 0.1f;

        public RuleLegendPresenter(RuleLegendView view, IReadOnlyList<RuleSpec> rules, ClueVisuals visuals, PaletteConfig palette)
        {
            var titles = new StringBuilder();
            var descriptions = new StringBuilder();
            var shapes = new List<Polyomino>();
            foreach (RuleSpec rule in rules)
            {
                visuals.TryGet(rule.RuleId, out string title, out string description);
                Append(titles, title, " · ");
                Append(descriptions, description, "\n");
                shapes.AddRange(rule.Shapes);
            }

            view.SetText(titles.ToString(), descriptions.ToString());
            IReadOnlyList<ShapeIconView> icons = view.PrepareIcons(shapes.Count);
            for (int i = 0; i < shapes.Count; i++)
            {
                Sprite block = palette.RegionSprite(i * 2 + 1);
                icons[i].Show(IconCells(shapes[i], icons[i].Size), block != null ? Color.white : palette.RegionColor(i * 2 + 1), block);
            }
        }

        // Co hình vào giữa ô minh hoạ, giữ ô vuông, chừa lề và khe nhỏ giữa các ô.
        private static List<Rect> IconCells(Polyomino shape, Vector2 area)
        {
            int width = 0;
            int height = 0;
            foreach (Cell cell in shape.Cells)
            {
                width = Mathf.Max(width, cell.X + 1);
                height = Mathf.Max(height, cell.Y + 1);
            }

            float usable = Mathf.Min(area.x, area.y) * (1f - 2f * IconPadding);
            float size = usable / Mathf.Max(width, height);
            float gap = size * 0.08f;
            var origin = new Vector2(-width * size / 2f, height * size / 2f);

            var rects = new List<Rect>(shape.Area);
            foreach (Cell cell in shape.Cells)
            {
                var min = new Vector2(origin.x + cell.X * size + gap / 2f, origin.y - (cell.Y + 1) * size + gap / 2f);
                rects.Add(new Rect(min, new Vector2(size - gap, size - gap)));
            }
            return rects;
        }

        private static void Append(StringBuilder builder, string text, string separator)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (builder.Length > 0) builder.Append(separator);
            builder.Append(text);
        }
    }
}
