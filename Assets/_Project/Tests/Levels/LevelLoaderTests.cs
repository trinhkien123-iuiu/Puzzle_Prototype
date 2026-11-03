using System;
using NUnit.Framework;
using Puzzle.Application.Levels;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Rules;

namespace Puzzle.Tests.Levels
{
    public class LevelLoaderTests
    {
        private const string TwoLs = @"
// ví dụ tự vẽ
id: sample-01
difficulty: 2
tip: Kéo qua các ô để gộp: mỗi vùng là một hình L.

shape L
#.
#.
##

rule shape-bank L

solution
AAAB
ABBB
";

        private static LevelLoader Loader() => new LevelLoader(RuleRegistry.CreateDefault());

        [Test]
        public void ValidText_ProducesBoardRulesAndSolution()
        {
            LevelDocument level = Loader().Parse(TwoLs, "fallback");

            Assert.AreEqual("sample-01", level.Id);
            Assert.AreEqual(2, level.Difficulty);
            Assert.AreEqual("Kéo qua các ô để gộp: mỗi vùng là một hình L.", level.Tip);
            Assert.AreEqual(8, level.Puzzle.Board.Cells.Count);
            Assert.AreEqual(1, level.Puzzle.Rules.Count);
            Assert.AreEqual(EdgeState.Border, level.Puzzle.SolutionStateOf(TestBoards.EdgeBetween(2, 0, 3, 0)));
        }

        [Test]
        public void RuleSpecs_ListRuleIdsAndShapesForTheLegend()
        {
            LevelDocument level = Loader().Parse(TwoLs, "fallback");

            Assert.AreEqual(1, level.RuleSpecs.Count);
            Assert.AreEqual("shape-bank", level.RuleSpecs[0].RuleId);
            Assert.AreEqual(1, level.RuleSpecs[0].Shapes.Count);
            Assert.AreEqual(4, level.RuleSpecs[0].Shapes[0].Area);
        }

        [Test]
        public void ParsedSolution_SatisfiesItsRules()
        {
            PuzzleDefinition puzzle = Loader().Parse(TwoLs, "fallback").Puzzle;
            PuzzleState solved = puzzle.CreateSolutionState();

            bool ok = new PuzzleValidator(puzzle.Rules).Validate(
                new RuleContext(solved, new RegionMap(solved)), new System.Collections.Generic.List<RuleViolation>());

            Assert.IsTrue(ok);
        }

        [Test]
        public void DotInSolution_IsAHole()
        {
            string text = "shape I\n##\nrule shape-bank I\nsolution\nAA.\n.BB\n";

            LevelDocument level = Loader().Parse(text, "holes");

            Assert.AreEqual("holes", level.Id);
            Assert.AreEqual(4, level.Puzzle.Board.Cells.Count);
            Assert.IsFalse(level.Puzzle.Board.Contains(new Cell(2, 0)));
        }

        [TestCase("rule shape-bank L\nsolution\nAA\n", "shape 'L' is not defined", TestName = "Undefined shape")]
        [TestCase("shape L\n#\nrule no-such-rule L\nsolution\nAA\n", "unknown rule", TestName = "Unknown rule")]
        [TestCase("shape L\n#\nrule shape-bank L\n", "Missing 'solution'", TestName = "Missing solution")]
        [TestCase("shape L\n#\nsolution\nAA\n", "at least one rule", TestName = "No rule")]
        [TestCase("AA\nsolution\nAA\n", "Line 1", TestName = "Grid without keyword")]
        [TestCase("difficulty: hard\n", "not a number", TestName = "Bad number")]
        public void BrokenText_ThrowsFormatExceptionWithReason(string text, string expected)
        {
            var error = Assert.Throws<FormatException>(() => Loader().Parse(text, "broken"));

            StringAssert.Contains(expected, error.Message);
        }
    }
}
