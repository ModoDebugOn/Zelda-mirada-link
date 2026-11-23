namespace Gaze.Application
{
    /// <summary>What the rule said about a point, for whoever draws it.</summary>
    public enum CandidateVerdict
    {
        /// <summary>Not marked as interesting: scenery.</summary>
        NotMarked,

        /// <summary>Further than the radius.</summary>
        TooFar,

        /// <summary>Inside the radius but behind (outside the attention angle).</summary>
        OutsideAngle,

        /// <summary>Could be looked at, but another one won.</summary>
        Eligible,

        /// <summary>The one the head is looking at.</summary>
        Chosen,
    }

    /// <summary>One point of interest and its verdict, as simple data.</summary>
    public readonly struct CandidateState
    {
        public int Id { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public CandidateVerdict Verdict { get; }

        /// <summary>From the head, in metres.</summary>
        public float Distance { get; }

        /// <summary>From the front of the body, in degrees (positive = right).</summary>
        public float AngleFromFront { get; }

        public int Priority { get; }

        public CandidateState(int id, float x, float y, float z, CandidateVerdict verdict,
                              float distance, float angleFromFront, int priority)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Verdict = verdict;
            Distance = distance;
            AngleFromFront = angleFromFront;
            Priority = priority;
        }
    }
}
