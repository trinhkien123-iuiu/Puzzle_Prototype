using System;
using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Application.Commands;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Tests.Application
{
    /// <summary>
    /// Hợp đồng chung cho MỌI command (ARCHITECTURE.md mục 0b, chữ L).
    /// Thêm command mới → thêm một dòng vào <see cref="Factories"/>.
    /// </summary>
    public class CommandContractTests
    {
        private static readonly Dictionary<string, Func<PuzzleDefinition, ICommand>> Factories =
            new Dictionary<string, Func<PuzzleDefinition, ICommand>>
            {
                ["merge-drag"] = d => new MergeCellsCommand(d,
                    TestPuzzles.Cells((0, 0), (1, 0), (2, 0), (3, 0), (3, 1))),
                ["merge-across-given"] = d => new MergeCellsCommand(d, TestPuzzles.Cells((2, 0), (3, 0))),
                ["set-border"] = d => new SetEdgeCommand(d, TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border),
                ["set-joined"] = d => new SetEdgeCommand(d, TestBoards.EdgeBetween(1, 1, 2, 1), EdgeState.Joined),
                ["clear-region"] = d => new ClearRegionCommand(d, new Cell(1, 1)),
            };

        private static IEnumerable<string> Commands() => Factories.Keys;

        private static IEnumerable<string> StartStates()
        {
            yield return "fresh";
            yield return "solved";
            yield return "mixed";
        }

        [Test, Combinatorial]
        public void UndoAfterDo_RestoresStateExactly(
            [ValueSource(nameof(Commands))] string command,
            [ValueSource(nameof(StartStates))] string start)
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            PuzzleState state = StartState(definition, start);
            PuzzleState original = state.Clone();
            ICommand cmd = Factories[command](definition);

            cmd.Do(state);
            cmd.Undo(state);

            Assert.IsTrue(state.SameEdgesAs(original));
        }

        [Test, Combinatorial]
        public void RedoAfterUndo_GivesSameResultAsFirstDo(
            [ValueSource(nameof(Commands))] string command,
            [ValueSource(nameof(StartStates))] string start)
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            PuzzleState state = StartState(definition, start);
            ICommand cmd = Factories[command](definition);

            cmd.Do(state);
            PuzzleState afterFirstDo = state.Clone();
            cmd.Undo(state);
            cmd.Do(state);

            Assert.IsTrue(state.SameEdgesAs(afterFirstDo));
        }

        [Test, Combinatorial]
        public void Commands_NeverChangeGivenBorders(
            [ValueSource(nameof(Commands))] string command,
            [ValueSource(nameof(StartStates))] string start)
        {
            PuzzleDefinition definition = TestPuzzles.TwoLs(withGivenBorder: true);
            PuzzleState state = StartState(definition, start);

            Factories[command](definition).Do(state);

            Assert.AreEqual(EdgeState.Border, state.GetEdge(TestPuzzles.GivenEdge));
        }

        private static PuzzleState StartState(PuzzleDefinition definition, string name)
        {
            if (name == "solved") return definition.CreateSolutionState();
            PuzzleState state = definition.CreateInitialState();
            if (name == "mixed")
            {
                state.SetEdge(TestBoards.EdgeBetween(0, 0, 1, 0), EdgeState.Border);
                state.SetEdge(TestBoards.EdgeBetween(1, 0, 1, 1), EdgeState.Joined);
            }
            return state;
        }
    }
}
