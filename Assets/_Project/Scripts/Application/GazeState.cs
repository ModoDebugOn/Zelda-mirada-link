using System.Collections.Generic;

namespace Gaze.Application
{
    /// <summary>
    /// What has to be shown of the gaze at one instant, as simple data (no domain types).
    /// The views draw it without knowing how anything was decided.
    /// </summary>
    public readonly struct GazeState
    {
        /// <summary>The head's current turn, relative to the body, in degrees (positive = right).</summary>
        public float HeadAngle { get; }

        /// <summary>The turn the head is heading for (0 if there is nothing to look at).</summary>
        public float TargetHeadAngle { get; }

        public bool HasTarget { get; }

        /// <summary>The point being looked at (only if <see cref="HasTarget"/>).</summary>
        public CandidateState Target { get; }

        /// <summary>The target is further round than the neck can turn: the head stops at the limit.</summary>
        public bool AtNeckLimit { get; }

        public bool HintOn { get; }

        // The rule's values, so the debug layers can draw them.
        public float Radius { get; }
        public float AttentionAngle { get; }
        public float NeckLimit { get; }

        /// <summary>Every point of interest of the scene with its verdict.</summary>
        public IReadOnlyList<CandidateState> Candidates { get; }

        public GazeState(float headAngle, float targetHeadAngle, bool hasTarget, CandidateState target,
                         bool atNeckLimit, bool hintOn, float radius, float attentionAngle, float neckLimit,
                         IReadOnlyList<CandidateState> candidates)
        {
            HeadAngle = headAngle;
            TargetHeadAngle = targetHeadAngle;
            HasTarget = hasTarget;
            Target = target;
            AtNeckLimit = atNeckLimit;
            HintOn = hintOn;
            Radius = radius;
            AttentionAngle = attentionAngle;
            NeckLimit = neckLimit;
            Candidates = candidates;
        }
    }
}
