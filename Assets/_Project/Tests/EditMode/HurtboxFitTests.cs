using GraceRun.Bosses;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class HurtboxFitTests
    {
        private static bool[] Grid(int w, int h, params RectInt[] filled)
        {
            var grid = new bool[w * h];
            foreach (var r in filled)
                for (var y = r.yMin; y < r.yMax; y++)
                for (var x = r.xMin; x < r.xMax; x++)
                    grid[y * w + x] = true;
            return grid;
        }

        [Test]
        public void EmptyImageGivesEmptyBox()
        {
            Assert.AreEqual(0, HurtboxFit.BodyBox(new bool[100], 10, 10, 0.3f).width);
        }

        [Test]
        public void SolidBodyIsItsBoundingBox()
        {
            var box = HurtboxFit.BodyBox(Grid(20, 20, new RectInt(4, 2, 8, 10)), 20, 20, 0.3f);
            Assert.AreEqual(new RectInt(4, 2, 8, 10), box);
        }

        [Test]
        public void ThinHorizontalBladeIsIgnored()
        {
            var grid = Grid(30, 20, new RectInt(4, 2, 8, 12), new RectInt(12, 8, 16, 1));
            var box = HurtboxFit.BodyBox(grid, 30, 20, 0.3f);
            Assert.AreEqual(4, box.xMin);
            Assert.AreEqual(12, box.xMax);
        }

        [Test]
        public void OpeningRemovesThinPartsAndKeepsMasses()
        {
            var mask = Grid(40, 20, new RectInt(2, 2, 14, 14), new RectInt(16, 8, 20, 2));
            var opened = HurtboxFit.Open(mask, 40, 20, 3);
            Assert.IsTrue(opened[8 * 40 + 8], "mass stays");
            Assert.IsFalse(opened[8 * 40 + 30], "a 2 px blade goes");
        }

        [Test]
        public void FarFromInkDropsOut()
        {
            var body = new RectInt(0, 0, 4, 4);
            var arc = new RectInt(20, 0, 4, 4);
            var mask = Grid(30, 5, body, arc);
            var ink = Grid(30, 5, new RectInt(0, 0, 1, 4));
            var kept = HurtboxFit.NearInk(mask, ink, 30, 5, 5);
            Assert.IsTrue(kept[0]);
            Assert.IsTrue(kept[3]);
            Assert.IsFalse(kept[21]);
        }

        [Test]
        public void ThinVerticalBladeIsIgnored()
        {
            var grid = Grid(20, 30, new RectInt(2, 2, 12, 8), new RectInt(5, 10, 1, 18));
            var box = HurtboxFit.BodyBox(grid, 20, 30, 0.3f);
            Assert.AreEqual(2, box.yMin);
            Assert.AreEqual(10, box.yMax);
        }
    }
}
