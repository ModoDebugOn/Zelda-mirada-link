using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>The colours of the debug layers, picked to read well on video (and the same in every layer).</summary>
    public static class DebugPalette
    {
        public static readonly Color Radius = new Color(0.25f, 0.85f, 1f);
        public static readonly Color Attention = new Color(1f, 0.85f, 0.2f, 0.28f);
        public static readonly Color NeckLimit = new Color(1f, 0.55f, 0.1f);
        public static readonly Color Rejected = new Color(1f, 0.25f, 0.25f, 0.35f);
        public static readonly Color GazeLine = new Color(0.35f, 1f, 0.45f);

        public static Color For(CandidateVerdict verdict) => verdict switch
        {
            CandidateVerdict.Chosen => new Color(0.35f, 1f, 0.45f),
            CandidateVerdict.Eligible => Color.white,
            CandidateVerdict.OutsideAngle => new Color(1f, 0.3f, 0.3f),
            CandidateVerdict.TooFar => new Color(0.35f, 0.55f, 1f),
            _ => new Color(0.5f, 0.5f, 0.5f),
        };
    }
}
