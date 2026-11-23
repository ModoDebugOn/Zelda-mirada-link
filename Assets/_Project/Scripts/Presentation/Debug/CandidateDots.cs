using System.Collections.Generic;
using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// Layer 4, the candidates: a dot over every point of interest with the rule's verdict.
    /// Green = looked at, white = could be, red = behind, blue = too far, grey = not marked (scenery).
    /// </summary>
    public sealed class CandidateDots : GazeView
    {
        [SerializeField] private float size = 0.18f;

        [Tooltip("How far above the point the dot floats, so it does not hide inside the object.")]
        [SerializeField] private float lift = 0.45f;

        private readonly List<Renderer> dots = new List<Renderer>();
        private MaterialPropertyBlock block;

        protected override void OnInit() => block = new MaterialPropertyBlock();

        public override void OnUpdate(GazeState state)
        {
            while (dots.Count < state.Candidates.Count)
                dots.Add(DebugDraw.CreateDot($"Dot{dots.Count}", transform, size));

            for (int i = 0; i < dots.Count; i++)
            {
                bool used = i < state.Candidates.Count;
                dots[i].enabled = used;
                if (!used)
                    continue;

                CandidateState candidate = state.Candidates[i];
                dots[i].transform.position = new Vector3(candidate.X, candidate.Y + lift, candidate.Z);
                DebugDraw.SetColour(dots[i], DebugPalette.For(candidate.Verdict), block);
            }
        }
    }
}
