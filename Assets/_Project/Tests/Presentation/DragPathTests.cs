using NUnit.Framework;
using Puzzle.Domain.Grid;
using Puzzle.Presentation.Input;

namespace Puzzle.Tests.Presentation
{
    public class DragPathTests
    {
        [Test]
        public void AdjacentSteps_AreAppended()
        {
            var path = new DragPath();
            path.Start(new Cell(0, 0));

            Assert.IsTrue(path.ExtendTo(new Cell(1, 0)));
            Assert.IsTrue(path.ExtendTo(new Cell(1, 1)));

            CollectionAssert.AreEqual(TestPuzzles.Cells((0, 0), (1, 0), (1, 1)), path.Cells);
        }

        [Test]
        public void SameCell_DoesNotChangePath()
        {
            var path = new DragPath();
            path.Start(new Cell(0, 0));

            Assert.IsFalse(path.ExtendTo(new Cell(0, 0)));
        }

        [Test]
        public void FastFingerJump_FillsCellsInBetween()
        {
            var path = new DragPath();
            path.Start(new Cell(0, 0));

            path.ExtendTo(new Cell(2, 1));

            CollectionAssert.AreEqual(TestPuzzles.Cells((0, 0), (1, 0), (2, 0), (2, 1)), path.Cells);
        }

        [Test]
        public void DraggingBack_RetractsThePath()
        {
            var path = new DragPath();
            path.Start(new Cell(0, 0));
            path.ExtendTo(new Cell(2, 0));

            path.ExtendTo(new Cell(1, 0));

            CollectionAssert.AreEqual(TestPuzzles.Cells((0, 0), (1, 0)), path.Cells);
        }
    }
}
