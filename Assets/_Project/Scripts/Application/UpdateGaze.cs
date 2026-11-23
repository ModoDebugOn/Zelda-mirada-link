using System;
using System.Collections.Generic;
using Gaze.Domain;

namespace Gaze.Application
{
    /// <summary>
    /// Use case: each step, decides what the head should look at and turns it a little towards it.
    /// It receives the scene as simple data, applies the rule (GazeRules), moves the head (HeadTurn)
    /// and reports the result through its output.
    /// </summary>
    public sealed class UpdateGaze
    {
        private const int NoTarget = int.MinValue;

        private readonly IGazeOutput output;
        private readonly HeadTurn head = new HeadTurn();
        private int lastTargetId = NoTarget;

        public UpdateGaze(IGazeOutput output)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary>Advances the gaze <paramref name="deltaTime"/> seconds and returns the new state.</summary>
        public GazeState Step(GazeInput input, float deltaTime)
        {
            GazeTuning tuning = input.Tuning;
            var rules = new GazeRules(new GazeSettings(tuning.Radius, tuning.AttentionAngle, tuning.NeckLimit, tuning.TurnSpeed));
            var headPosition = new Position(input.HeadX, input.HeadY, input.HeadZ);

            var points = new List<InterestPoint>(input.Points.Count);
            foreach (PointData p in input.Points)
                points.Add(new InterestPoint(p.Id, new Position(p.X, p.Y, p.Z), p.Priority, p.IsMarked));

            // With the hint off the rule is not applied: the head just looks ahead.
            InterestPoint chosen = tuning.HintOn ? rules.Choose(headPosition, input.Facing, points) : null;
            float angleToTarget = chosen != null ? rules.AngleFromFront(headPosition, input.Facing, chosen) : 0f;
            float targetHeadAngle = rules.HeadAngleFor(angleToTarget);
            head.Step(targetHeadAngle, tuning.TurnSpeed, deltaTime);

            var candidates = new List<CandidateState>(points.Count);
            CandidateState target = default;
            foreach (InterestPoint point in points)
            {
                CandidateState candidate = Describe(rules, headPosition, input.Facing, point, point == chosen);
                candidates.Add(candidate);
                if (point == chosen)
                    target = candidate;
            }

            var state = new GazeState(
                head.Angle, targetHeadAngle, chosen != null, target,
                atNeckLimit: chosen != null && Math.Abs(angleToTarget) > tuning.NeckLimit,
                tuning.HintOn, tuning.Radius, tuning.AttentionAngle, tuning.NeckLimit, candidates);

            int targetId = chosen?.Id ?? NoTarget;
            if (targetId != lastTargetId)
            {
                lastTargetId = targetId;
                output.OnTargetChanged(state);
            }
            output.OnUpdate(state);
            return state;
        }

        /// <summary>Head back to the front and no target (for restarting a take).</summary>
        public void Reset()
        {
            head.Reset();
            lastTargetId = NoTarget;
        }

        private static CandidateState Describe(GazeRules rules, Position head, float facing, InterestPoint point, bool isChosen)
        {
            CandidateVerdict verdict = isChosen ? CandidateVerdict.Chosen : rules.Check(head, facing, point) switch
            {
                CandidateCheck.NotMarked => CandidateVerdict.NotMarked,
                CandidateCheck.TooFar => CandidateVerdict.TooFar,
                CandidateCheck.OutsideAngle => CandidateVerdict.OutsideAngle,
                _ => CandidateVerdict.Eligible,
            };
            return new CandidateState(point.Id, point.Position.X, point.Position.Y, point.Position.Z, verdict,
                                      head.DistanceTo(point.Position), rules.AngleFromFront(head, facing, point),
                                      point.Priority);
        }
    }
}
