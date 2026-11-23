using System.Collections.Generic;
using System.Linq;
using Gaze.Application;
using NUnit.Framework;

namespace Gaze.Tests
{
    public sealed class UpdateGazeTests
    {
        private sealed class FakeOutput : IGazeOutput
        {
            public readonly List<GazeState> TargetChanges = new List<GazeState>();
            public readonly List<GazeState> Updates = new List<GazeState>();

            public void OnTargetChanged(GazeState state) => TargetChanges.Add(state);

            public void OnUpdate(GazeState state) => Updates.Add(state);
        }

        // Head at the origin facing +Z. Radius 4, attention 100°, neck 70°, 90°/s.
        private static GazeInput Input(bool hintOn, params PointData[] points) =>
            new GazeInput(0f, 0f, 0f, 0f, points, new GazeTuning(4f, 100f, 70f, 90f, hintOn));

        private static PointData Eye(float x, float z, bool marked = true) => new PointData(7, x, 0f, z, 0, marked);

        [Test]
        public void TheHeadTurnsSmoothlyTowardsTheTarget()
        {
            var output = new FakeOutput();
            var gaze = new UpdateGaze(output);

            // A point at 45° to the right.
            GazeState state = gaze.Step(Input(true, Eye(2f, 2f)), 0.25f);

            Assert.That(state.HasTarget, Is.True);
            Assert.That(state.Target.Id, Is.EqualTo(7));
            Assert.That(state.TargetHeadAngle, Is.EqualTo(45f).Within(1e-3f));
            Assert.That(state.HeadAngle, Is.EqualTo(22.5f).Within(1e-3f));
        }

        [Test]
        public void ReportsTheTargetChangeOnceAndUpdatesEveryStep()
        {
            var output = new FakeOutput();
            var gaze = new UpdateGaze(output);

            gaze.Step(Input(true, Eye(2f, 2f)), 0.1f);
            gaze.Step(Input(true, Eye(2f, 2f)), 0.1f);
            gaze.Step(Input(true), 0.1f);   // the point is gone

            Assert.That(output.Updates, Has.Count.EqualTo(3));
            Assert.That(output.TargetChanges, Has.Count.EqualTo(2));
            Assert.That(output.TargetChanges[0].HasTarget, Is.True);
            Assert.That(output.TargetChanges[1].HasTarget, Is.False);
        }

        [Test]
        public void BeyondTheNeckTheHeadStopsAtTheLimit()
        {
            var gaze = new UpdateGaze(new FakeOutput());

            // 90° to the right: inside the attention angle (100) but past the neck (70).
            GazeState state = gaze.Step(Input(true, Eye(3f, 0f)), 10f);

            Assert.That(state.AtNeckLimit, Is.True);
            Assert.That(state.HeadAngle, Is.EqualTo(70f).Within(1e-3f));
        }

        [Test]
        public void WithTheHintOffTheHeadLooksAhead()
        {
            var gaze = new UpdateGaze(new FakeOutput());

            GazeState state = gaze.Step(Input(false, Eye(2f, 2f)), 10f);

            Assert.That(state.HasTarget, Is.False);
            Assert.That(state.HeadAngle, Is.EqualTo(0f));
            Assert.That(state.HintOn, Is.False);
        }

        [Test]
        public void EveryPointGetsItsVerdict()
        {
            var gaze = new UpdateGaze(new FakeOutput());
            PointData[] points =
            {
                new PointData(1, 0f, 0f, 2f, 0, true),    // ahead: chosen
                new PointData(2, 0f, 0f, 3f, 0, true),    // ahead but further: eligible
                new PointData(3, 1f, 0f, 1f, 0, false),   // scenery
                new PointData(4, 0f, 0f, 9f, 0, true),    // too far
                new PointData(5, 0f, 0f, -2f, 0, true),   // behind
            };

            GazeState state = gaze.Step(Input(true, points), 0.1f);
            Dictionary<int, CandidateVerdict> verdicts = state.Candidates.ToDictionary(c => c.Id, c => c.Verdict);

            Assert.That(verdicts[1], Is.EqualTo(CandidateVerdict.Chosen));
            Assert.That(verdicts[2], Is.EqualTo(CandidateVerdict.Eligible));
            Assert.That(verdicts[3], Is.EqualTo(CandidateVerdict.NotMarked));
            Assert.That(verdicts[4], Is.EqualTo(CandidateVerdict.TooFar));
            Assert.That(verdicts[5], Is.EqualTo(CandidateVerdict.OutsideAngle));
        }

        [Test]
        public void ResetBringsTheHeadBack()
        {
            var output = new FakeOutput();
            var gaze = new UpdateGaze(output);
            gaze.Step(Input(true, Eye(2f, 2f)), 10f);

            gaze.Reset();
            GazeState state = gaze.Step(Input(true, Eye(2f, 2f)), 0f);

            Assert.That(state.HeadAngle, Is.EqualTo(0f));
            Assert.That(output.TargetChanges, Has.Count.EqualTo(2));   // after a reset the target is announced again
        }
    }
}
