using NUnit.Framework;
using Puzzle.Domain.Shapes;

namespace Puzzle.Tests.Shapes
{
    public class ShapeVariantsTests
    {
        private static readonly Polyomino L = Polyomino.Parse(
            "#.",
            "#.",
            "##");

        private static readonly Polyomino J = Polyomino.Parse(
            ".#",
            ".#",
            "##");

        private static readonly Polyomino T = Polyomino.Parse(
            "###",
            ".#.");

        [Test]
        public void Polyomino_IgnoresPosition()
        {
            Polyomino shifted = Polyomino.Parse(
                "....",
                "..#.",
                "..#.",
                "..##");

            Assert.AreEqual(L, shifted);
            Assert.AreEqual(L.GetHashCode(), shifted.GetHashCode());
        }

        [Test]
        public void FourRotations_ReturnToOriginal()
        {
            Assert.AreEqual(L, L.Rotate90().Rotate90().Rotate90().Rotate90());
            Assert.AreNotEqual(L, L.Rotate90());
        }

        [Test]
        public void Mirror_OfL_IsJ()
        {
            Assert.AreEqual(J, L.Mirror());
        }

        [TestCase(new[] { "#" }, 1, TestName = "Monomino has 1 variant")]
        [TestCase(new[] { "##", "##" }, 1, TestName = "Square tetromino has 1 variant")]
        [TestCase(new[] { "####" }, 2, TestName = "Straight tetromino has 2 variants")]
        [TestCase(new[] { "###", ".#." }, 4, TestName = "T tetromino has 4 variants")]
        [TestCase(new[] { ".##", "##." }, 4, TestName = "S tetromino has 4 variants")]
        [TestCase(new[] { "#.", "#.", "##" }, 8, TestName = "L tetromino has 8 variants")]
        [TestCase(new[] { ".##", "##.", ".#." }, 8, TestName = "F pentomino has 8 variants")]
        public void VariantCount_MatchesSymmetry(string[] rows, int expected)
        {
            var variants = new ShapeVariants(Polyomino.Parse(rows));

            Assert.AreEqual(expected, variants.All.Count);
        }

        [Test]
        public void AllVariants_ShareTheSameCanonicalForm()
        {
            Polyomino canonical = new ShapeVariants(L).Canonical;

            foreach (Polyomino variant in new ShapeVariants(L).All)
            {
                Assert.AreEqual(canonical, new ShapeVariants(variant).Canonical);
            }
        }

        [Test]
        public void Matches_AcceptsRotatedAndMirrored_RejectsOtherShapes()
        {
            var variants = new ShapeVariants(L);

            Assert.IsTrue(variants.Matches(L.Rotate90()));
            Assert.IsTrue(variants.Matches(J));
            Assert.IsFalse(variants.Matches(T));
            Assert.AreNotEqual(variants.Canonical, new ShapeVariants(T).Canonical);
        }
    }
}
