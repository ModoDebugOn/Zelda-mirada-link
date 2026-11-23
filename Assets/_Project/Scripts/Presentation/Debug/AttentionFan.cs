using System.Linq;
using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// Layer 2, angle and neck limit: a fan at waist height with the attention angle (yellow) and two lines
    /// where the neck stops (orange). It turns red when something marked is inside the radius but behind,
    /// outside the fan, and nothing is being looked at: the head ignores it.
    /// </summary>
    public sealed class AttentionFan : GazeView
    {
        [Tooltip("At eye height, child of the body (not of the head): the fan follows the body.")]
        [SerializeField] private Transform eyeLevel;

        [Tooltip("How long the fan and the lines are drawn, in metres (the radius is the other layer).")]
        [SerializeField] private float length = 1.6f;

        [SerializeField] private float lineWidth = 0.035f;

        [Tooltip("Height compared with the eyes. Below them (at the waist), so it never covers the face, " +
                 "whether the camera is high or low.")]
        [SerializeField] private float lift = -0.6f;

        private MeshFilter fan;
        private LineRenderer leftLimit;
        private LineRenderer rightLimit;

        private float drawnAngle = -1f;
        private bool drawnRejected;
        private float neckLimit;

        protected override void OnInit()
        {
            fan = DebugDraw.CreateShape("Fan", transform);
            leftLimit = DebugDraw.CreateLine("NeckLimitLeft", transform, DebugPalette.NeckLimit, lineWidth);
            rightLimit = DebugDraw.CreateLine("NeckLimitRight", transform, DebugPalette.NeckLimit, lineWidth);
        }

        public override void OnUpdate(GazeState state)
        {
            neckLimit = state.NeckLimit;
            bool rejected = !state.HasTarget && state.HintOn &&
                            state.Candidates.Any(c => c.Verdict == CandidateVerdict.OutsideAngle);

            // The mesh is only rebuilt when it changes.
            if (!Mathf.Approximately(drawnAngle, state.AttentionAngle) || rejected != drawnRejected)
            {
                drawnAngle = state.AttentionAngle;
                drawnRejected = rejected;
                DebugDraw.Fan(fan.sharedMesh, drawnAngle, length, rejected ? DebugPalette.Rejected : DebugPalette.Attention);
            }
        }

        private void LateUpdate()
        {
            Quaternion facing = Quaternion.Euler(0f, eyeLevel.eulerAngles.y, 0f);
            Vector3 origin = eyeLevel.position + Vector3.up * lift;
            fan.transform.SetPositionAndRotation(origin, facing);

            Line(leftLimit, origin, facing * Quaternion.Euler(0f, -neckLimit, 0f) * Vector3.forward);
            Line(rightLimit, origin, facing * Quaternion.Euler(0f, neckLimit, 0f) * Vector3.forward);
        }

        private void Line(LineRenderer line, Vector3 origin, Vector3 direction)
        {
            line.positionCount = 2;
            line.SetPosition(0, origin);
            line.SetPosition(1, origin + direction * length * 1.1f);
        }
    }
}
