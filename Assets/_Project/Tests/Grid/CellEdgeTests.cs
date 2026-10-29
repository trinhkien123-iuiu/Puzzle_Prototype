using System;
using NUnit.Framework;
using Puzzle.Domain.Grid;

namespace Puzzle.Tests.Grid
{
    public class CellEdgeTests
    {
        [Test]
        public void Cells_WithSameCoordinates_AreEqual()
        {
            Assert.AreEqual(new Cell(2, 3), new Cell(2, 3));
            Assert.AreEqual(new Cell(2, 3).GetHashCode(), new Cell(2, 3).GetHashCode());
            Assert.AreNotEqual(new Cell(2, 3), new Cell(3, 2));
        }

        [Test]
        public void Cells_CompareByRowThenColumn()
        {
            Assert.Less(new Cell(5, 0).CompareTo(new Cell(0, 1)), 0);
            Assert.Less(new Cell(0, 1).CompareTo(new Cell(1, 1)), 0);
            Assert.AreEqual(0, new Cell(1, 1).CompareTo(new Cell(1, 1)));
        }

        [Test]
        public void Adjacency_IgnoresDiagonals()
        {
            var cell = new Cell(1, 1);
            Assert.IsTrue(cell.IsAdjacentTo(new Cell(1, 0)));
            Assert.IsTrue(cell.IsAdjacentTo(new Cell(2, 1)));
            Assert.IsFalse(cell.IsAdjacentTo(new Cell(2, 2)));
            Assert.IsFalse(cell.IsAdjacentTo(cell));
        }

        [Test]
        public void Edge_IsNormalized_RegardlessOfArgumentOrder()
        {
            var a = new Cell(1, 0);
            var b = new Cell(0, 0);
            var forward = new Edge(a, b);
            var backward = new Edge(b, a);

            Assert.AreEqual(forward, backward);
            Assert.AreEqual(forward.GetHashCode(), backward.GetHashCode());
            Assert.AreEqual(b, forward.A);
            Assert.AreEqual(a, forward.B);
        }

        [Test]
        public void Edge_BetweenNonAdjacentCells_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Edge(new Cell(0, 0), new Cell(1, 1)));
            Assert.Throws<ArgumentException>(() => new Edge(new Cell(0, 0), new Cell(0, 0)));
        }

        [Test]
        public void Edge_ReportsOrientation()
        {
            Assert.IsTrue(TestBoards.EdgeBetween(0, 0, 1, 0).IsVertical);
            Assert.IsFalse(TestBoards.EdgeBetween(0, 0, 0, 1).IsVertical);
        }

        [Test]
        public void RectangleBoard_HasExpectedCellsAndEdges()
        {
            BoardShape board = BoardShape.Rectangle(3, 2);

            Assert.AreEqual(6, board.Cells.Count);
            Assert.AreEqual(7, board.Edges.Count); // 2 cạnh dọc × 2 hàng + 3 cạnh ngang
            Assert.AreEqual(3, board.Width);
            Assert.AreEqual(2, board.Height);
        }

        [Test]
        public void BoardWithHole_HasNoEdgesTouchingTheHole()
        {
            BoardShape board = TestBoards.ShapeOf(
                "...",
                ". .",
                "...");

            Assert.AreEqual(8, board.Cells.Count);
            Assert.IsFalse(board.Contains(new Cell(1, 1)));
            Assert.AreEqual(8, board.Edges.Count);
            foreach (Edge edge in board.Edges)
            {
                Assert.IsFalse(edge.Touches(new Cell(1, 1)));
            }
        }
    }
}
