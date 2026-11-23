using Gaze.Domain;
using NUnit.Framework;

namespace Gaze.Tests
{
    public sealed class PositionAndAnglesTests
    {
        [Test]
        public void DistanceIncludesHeight()
        {
            var a = new Position(0f, 0f, 0f);
            var b = new Position(0f, 3f, 4f);
            Assert.That(a.DistanceTo(b), Is.EqualTo(5f).Within(1e-4f));
        }

        [TestCase(0f, 1f, 0f)]
        [TestCase(1f, 0f, 90f)]
        [TestCase(-1f, 0f, -90f)]
        [TestCase(0f, -1f, 180f)]
        public void HeadingIsMeasuredLikeUnitysYaw(float x, float z, float expected)
        {
            var origin = new Position(0f, 0f, 0f);
            Assert.That(origin.HeadingTo(new Position(x, 5f, z)), Is.EqualTo(expected).Within(1e-3f));
        }

        [TestCase(0f, 90f, 90f)]
        [TestCase(350f, 10f, 20f)]
        [TestCase(10f, 350f, -20f)]
        [TestCase(-170f, 170f, -20f)]
        [TestCase(0f, 180f, 180f)]
        public void DeltaTakesTheShortWay(float from, float to, float expected)
        {
            Assert.That(Angles.Delta(from, to), Is.EqualTo(expected).Within(1e-3f));
        }

        [Test]
        public void MoveTowardsNeverOvershoots()
        {
            Assert.That(Angles.MoveTowards(0f, 30f, 10f), Is.EqualTo(10f));
            Assert.That(Angles.MoveTowards(25f, 30f, 10f), Is.EqualTo(30f));
            Assert.That(Angles.MoveTowards(0f, -30f, 10f), Is.EqualTo(-10f));
        }
    }
}
