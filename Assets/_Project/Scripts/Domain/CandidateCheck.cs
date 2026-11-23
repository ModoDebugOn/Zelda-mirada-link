namespace Gaze.Domain
{
    /// <summary>What the rule says about one point of interest, checked in this order.</summary>
    public enum CandidateCheck
    {
        /// <summary>The designer did not mark it: it is scenery.</summary>
        NotMarked,

        /// <summary>Further than the radius.</summary>
        TooFar,

        /// <summary>Close enough, but outside the attention angle (behind you).</summary>
        OutsideAngle,

        /// <summary>Passes every check: it can be looked at.</summary>
        Eligible,
    }
}
