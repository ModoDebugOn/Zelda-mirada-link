using Gaze.Domain;
using NUnit.Framework;

namespace Gaze.Tests
{
    public sealed class GazeRulesTests
    {
        // Head at the origin, body facing +Z. Radius 4 m, attention 100°, neck 70°.
        private static readonly Position Head = new Position(0f, 0f, 0f);
        private const float Facing = 0f;

        private static GazeRules Rules() => new GazeRules(new GazeSettings(4f, 100f, 70f, 180f));

        private static InterestPoint Point(int id, float x, float z, int priority = 0, bool marked = true) =>
            new InterestPoint(id, new Position(x, 0f, z), priority, marked);

        [Test]
        public void UnmarkedPointsAreScenery()
        {
            Assert.That(Rules().Check(Head, Facing, Point(1, 0f, 1f, marked: false)), Is.EqualTo(CandidateCheck.NotMarked));
        }

        [Test]
        public void PointsOutsideTheRadiusAreTooFar()
        {
            Assert.That(Rules().Check(Head, Facing, Point(1, 0f, 4.5f)), Is.EqualTo(CandidateCheck.TooFar));
            Assert.That(Rules().Check(Head, Facing, Point(1, 0f, 3.9f)), Is.EqualTo(CandidateCheck.Eligible));
        }

        [Test]
        public void PointsBehindAreOutsideTheAngle()
        {
            Assert.That(Rules().Check(Head, Facing, Point(1, 0f, -2f)), Is.EqualTo(CandidateCheck.OutsideAngle));
            Assert.That(Rules().Check(Head, Facing, Point(1, -2f, 0f)), Is.EqualTo(CandidateCheck.Eligible));
        }

        [Test]
        public void TheAngleFollowsWhereTheBodyFaces()
        {
            // Facing +X (90°), a point on +X is straight ahead and one on +Z is to the left.
            Assert.That(Rules().AngleFromFront(Head, 90f, Point(1, 3f, 0f)), Is.EqualTo(0f).Within(1e-3f));
            Assert.That(Rules().AngleFromFront(Head, 90f, Point(1, 0f, 3f)), Is.EqualTo(-90f).Within(1e-3f));
        }

        [Test]
        public void HighestPriorityWins()
        {
            InterestPoint near = Point(1, 0f, 1f, priority: 0);
            InterestPoint important = Point(2, 0f, 3f, priority: 5);
            Assert.That(Rules().Choose(Head, Facing, new[] { near, important }), Is.SameAs(important));
        }

        [Test]
        public void OnATieTheNearestWins()
        {
            InterestPoint far = Point(1, 0f, 3f);
            InterestPoint near = Point(2, 1f, 1f);
            Assert.That(Rules().Choose(Head, Facing, new[] { far, near }), Is.SameAs(near));
        }

        [Test]
        public void IneligiblePointsNeverWin()
        {
            InterestPoint scenery = Point(1, 0f, 1f, priority: 9, marked: false);
            InterestPoint behind = Point(2, 0f, -1f, priority: 9);
            InterestPoint farAway = Point(3, 0f, 10f, priority: 9);
            Assert.That(Rules().Choose(Head, Facing, new[] { scenery, behind, farAway }), Is.Null);
        }

        [Test]
        public void TheHeadStopsAtTheNeckLimit()
        {
            GazeRules rules = Rules();
            Assert.That(rules.HeadAngleFor(30f), Is.EqualTo(30f));
            Assert.That(rules.HeadAngleFor(95f), Is.EqualTo(70f));
            Assert.That(rules.HeadAngleFor(-95f), Is.EqualTo(-70f));
        }

        [Test]
        public void SettingsRejectImpossibleValues()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GazeSettings(-1f, 90f, 70f, 180f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GazeSettings(4f, 200f, 70f, 180f));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GazeSettings(4f, 90f, 70f, 0f));
        }
    }
}
