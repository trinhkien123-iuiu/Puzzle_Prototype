using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Rules;
using Puzzle.Domain.Rules.ShapeBank;
using Puzzle.Domain.Shapes;

namespace Puzzle.Tests.Rules
{
    public class ShapeBankRuleTests
    {
        // Ví dụ tự vẽ: bàn 4x2 chia thành hai hình L (một cái đã xoay/lật).
        private static readonly string[] TwoLs =
        {
            "AAAB",
            "ABBB",
        };

        private static readonly Polyomino L = Polyomino.Parse(
            "#.",
            "#.",
            "##");

        private readonly List<RuleViolation> _violations = new List<RuleViolation>();

        private static ShapeBankRule LBank() => new ShapeBankRule(new[] { L });

        private void Check(IRule rule, PuzzleState state)
        {
            _violations.Clear();
            rule.Check(new RuleContext(state, new RegionMap(state)), _violations);
        }

        [Test]
        public void Solution_WithRotatedAndMirroredShapes_HasNoViolations()
        {
            Check(LBank(), TestBoards.StateOf(TwoLs));

            Assert.IsEmpty(_violations);
        }

        [Test]
        public void SettledWrongShapes_AreBroken()
        {
            PuzzleState state = TestBoards.StateOf(
                "AABB",
                "AABB");

            Check(LBank(), state);

            Assert.AreEqual(2, _violations.Count);
            foreach (RuleViolation violation in _violations)
            {
                Assert.AreEqual(ShapeBankRule.Id, violation.RuleId);
                Assert.AreEqual(Severity.Broken, violation.Severity);
                Assert.AreEqual(4, violation.Cells.Count);
            }
        }

        [Test]
        public void FreshBoard_IsIncomplete_NotBroken()
        {
            var state = new PuzzleState(BoardShape.Rectangle(4, 2));

            Check(LBank(), state);

            Assert.AreEqual(1, _violations.Count);
            Assert.AreEqual(Severity.Incomplete, _violations[0].Severity);
        }

        [Test]
        public void EnclosedRegionSmallerThanEveryShape_IsBroken_EvenWithUnknownsElsewhere()
        {
            var state = new PuzzleState(BoardShape.Rectangle(4, 2));
            state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border);
            state.SetEdge(TestBoards.EdgeBetween(0, 0, 0, 1), EdgeState.Border);

            Check(LBank(), state);

            Assert.AreEqual(2, _violations.Count);
            RuleViolation corner = _violations.Find(v => v.Cells.Count == 1);
            Assert.AreEqual(Severity.Broken, corner.Severity);
            RuleViolation rest = _violations.Find(v => v.Cells.Count == 7);
            Assert.AreEqual(Severity.Incomplete, rest.Severity);
        }

        [Test]
        public void CorrectRegions_WithUndecidedInnerEdges_StillPass()
        {
            // Vùng đã đúng hình thì cạnh bên trong để Unknown cũng không sao.
            PuzzleState state = TestBoards.StateOf(TwoLs);
            state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Unknown);

            Check(LBank(), state);

            Assert.IsEmpty(_violations);
        }

        [Test]
        public void BankWithSeveralShapes_AcceptsAnyOfThem()
        {
            var bank = new ShapeBankRule(new[] { Polyomino.Parse("##"), Polyomino.Parse("###") });
            PuzzleState state = TestBoards.StateOf(
                "AAB",
                "CCB");

            Check(bank, state);

            Assert.IsEmpty(_violations);
        }

        [Test]
        public void Validator_AcceptsDefinitionSolution_AndRejectsInitialState()
        {
            var definition = new PuzzleDefinition(
                TestBoards.ShapeOf(TwoLs),
                new IRule[] { RuleRegistry.CreateDefault().Create(ShapeBankRule.Id, new RuleParameters(new[] { L })) },
                new Edge[0],
                TestBoards.BordersOf(TwoLs));
            var validator = new PuzzleValidator(definition.Rules);

            PuzzleState solved = definition.CreateSolutionState();
            PuzzleState fresh = definition.CreateInitialState();

            Assert.IsTrue(validator.Validate(new RuleContext(solved, new RegionMap(solved)), _violations));
            Assert.IsFalse(validator.Validate(new RuleContext(fresh, new RegionMap(fresh)), _violations));
            Assert.IsFalse(PuzzleValidator.HasBroken(_violations));
        }

        [Test]
        public void Registry_RejectsUnknownRuleId()
        {
            Assert.Throws<KeyNotFoundException>(() => RuleRegistry.CreateDefault().Create("no-such-rule", null));
        }
    }
}
