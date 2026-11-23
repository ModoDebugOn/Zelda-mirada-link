using Gaze.Domain;
using NUnit.Framework;

namespace Gaze.Tests
{
    public sealed class HeadTurnTests
    {
        [Test]
        public void StartsLookingAhead()
        {
            Assert.That(new HeadTurn().Angle, Is.EqualTo(0f));
        }

        [Test]
        public void TurnsAtMostAtItsSpeed()
        {
            var head = new HeadTurn();
            head.Step(60f, turnSpeed: 90f, deltaTime: 0.5f);
            Assert.That(head.Angle, Is.EqualTo(45f).Within(1e-4f));
            head.Step(60f, 90f, 0.5f);
            Assert.That(head.Angle, Is.EqualTo(60f).Within(1e-4f));
        }

        [Test]
        public void WithInfiniteSpeedItSnaps()
        {
            var head = new HeadTurn();
            head.Step(-70f, float.PositiveInfinity, 0.016f);
            Assert.That(head.Angle, Is.EqualTo(-70f));
        }

        [Test]
        public void GoesBackToTheFront()
        {
            var head = new HeadTurn();
            head.Step(40f, float.PositiveInfinity, 0.016f);
            head.Step(0f, 100f, 0.1f);
            Assert.That(head.Angle, Is.EqualTo(30f).Within(1e-4f));
            head.Reset();
            Assert.That(head.Angle, Is.EqualTo(0f));
        }

        [Test]
        public void NoTimeNoTurn()
        {
            var head = new HeadTurn();
            head.Step(40f, 100f, 0f);
            Assert.That(head.Angle, Is.EqualTo(0f));
        }
    }
}
