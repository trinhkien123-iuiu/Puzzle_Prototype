using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Rules;
using Puzzle.Domain.Shapes;

namespace Puzzle.Application.Levels
{
    /// <summary>
    /// Đọc level dạng văn bản (định dạng ở Docs/design/level-format.md) thành <see cref="LevelDocument"/>.
    /// Văn bản thuần để level diff được trên git và sửa tay được; lỗi định dạng ném
    /// <see cref="FormatException"/> kèm số dòng để importer báo ngay khi nạp asset.
    /// </summary>
    public sealed class LevelLoader
    {
        private readonly RuleRegistry _registry;

        public LevelLoader(RuleRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public LevelDocument Parse(string text, string fallbackId)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var reader = new LevelTextReader(text);
            var draft = new Draft { Id = fallbackId };

            while (reader.TryNextKeyword(out string keyword, out string[] args))
            {
                ReadSection(reader, draft, keyword, args);
            }
            return Build(draft, reader);
        }

        private static void ReadSection(LevelTextReader reader, Draft draft, string keyword, string[] args)
        {
            switch (keyword)
            {
                case "id":
                    draft.Id = reader.RequireArg(args, 0);
                    break;
                case "difficulty":
                    draft.Difficulty = reader.RequireInt(args, 0);
                    break;
                case "shape":
                    string name = reader.RequireArg(args, 0);
                    if (draft.Shapes.ContainsKey(name)) throw reader.Error($"Shape '{name}' is defined twice.");
                    draft.Shapes[name] = Polyomino.Parse(reader.ReadGrid().ToArray());
                    break;
                case "rule":
                    draft.Rules.Add((reader.RequireArg(args, 0), args, reader.LineNumber));
                    break;
                case "tip":
                    draft.Tip = reader.RestOfLine;
                    break;
                case "solution":
                    draft.Solution = reader.ReadGrid();
                    break;
                default:
                    throw reader.Error($"Unknown keyword '{keyword}'.");
            }
        }

        private LevelDocument Build(Draft draft, LevelTextReader reader)
        {
            if (draft.Solution == null || draft.Solution.Count == 0) throw reader.Error("Missing 'solution' section.");
            if (draft.Rules.Count == 0) throw reader.Error("A level needs at least one rule.");

            var rules = new List<IRule>(draft.Rules.Count);
            var specs = new List<RuleSpec>(draft.Rules.Count);
            foreach ((string ruleId, string[] args, int line) in draft.Rules)
            {
                if (!_registry.Contains(ruleId)) throw new FormatException($"Line {line}: unknown rule '{ruleId}'.");
                List<Polyomino> shapes = ShapesFor(draft, args, line);
                rules.Add(_registry.Create(ruleId, new RuleParameters(shapes)));
                specs.Add(new RuleSpec(ruleId, shapes));
            }

            var solution = new SolutionGrid(draft.Solution);
            var puzzle = new PuzzleDefinition(solution.Board, rules, Array.Empty<Edge>(), solution.Borders);
            return new LevelDocument(draft.Id, draft.Difficulty, puzzle, specs, draft.Tip);
        }

        // Tham số sau id luật là tên hình đã khai báo bằng 'shape'.
        private static List<Polyomino> ShapesFor(Draft draft, string[] args, int line)
        {
            var shapes = new List<Polyomino>();
            for (int i = 1; i < args.Length; i++)
            {
                if (!draft.Shapes.TryGetValue(args[i], out Polyomino shape))
                {
                    throw new FormatException($"Line {line}: shape '{args[i]}' is not defined.");
                }
                shapes.Add(shape);
            }
            return shapes;
        }

        private sealed class Draft
        {
            public string Id;
            public int Difficulty = 1;
            public string Tip = string.Empty;
            public readonly Dictionary<string, Polyomino> Shapes = new Dictionary<string, Polyomino>();
            public readonly List<(string RuleId, string[] Args, int Line)> Rules = new List<(string, string[], int)>();
            public List<string> Solution;
        }
    }
}
