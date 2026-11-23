using System;
using System.Collections.Generic;

namespace Gaze.Domain
{
    /// <summary>
    /// The rule that decides where the head looks:
    /// 1. only points marked as interesting, 2. inside the radius, 3. inside the attention angle.
    /// Among the ones that pass, the highest priority wins, and on a tie the nearest.
    /// The head then turns towards the winner, but never past the neck limit.
    /// </summary>
    public sealed class GazeRules
    {
        public GazeSettings Settings { get; }

        public GazeRules(GazeSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Where the point is compared with the front of the body, in degrees (-180 to 180, positive = right).
        /// </summary>
        /// <param name="head">Where the head is.</param>
        /// <param name="facing">Where the body faces (0 = +Z, 90 = +X).</param>
        public float AngleFromFront(Position head, float facing, InterestPoint point) =>
            Angles.Delta(facing, head.HeadingTo(point.Position));

        public CandidateCheck Check(Position head, float facing, InterestPoint point)
        {
            if (!point.IsMarked)
                return CandidateCheck.NotMarked;
            if (head.DistanceTo(point.Position) > Settings.Radius)
                return CandidateCheck.TooFar;
            if (MathF.Abs(AngleFromFront(head, facing, point)) > Settings.AttentionAngle)
                return CandidateCheck.OutsideAngle;
            return CandidateCheck.Eligible;
        }

        /// <summary>The point the head should look at, or null if none is eligible.</summary>
        public InterestPoint Choose(Position head, float facing, IEnumerable<InterestPoint> points)
        {
            InterestPoint best = null;
            float bestDistance = float.MaxValue;
            foreach (InterestPoint point in points)
            {
                if (Check(head, facing, point) != CandidateCheck.Eligible)
                    continue;

                float distance = head.DistanceTo(point.Position);
                bool wins = best == null
                            || point.Priority > best.Priority
                            || (point.Priority == best.Priority && distance < bestDistance);
                if (wins)
                {
                    best = point;
                    bestDistance = distance;
                }
            }
            return best;
        }

        /// <summary>The angle the head turns to for a point at that angle: as far as the neck lets it.</summary>
        public float HeadAngleFor(float angleFromFront) =>
            Math.Clamp(angleFromFront, -Settings.NeckLimit, Settings.NeckLimit);
    }
}
