using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// Layer 3, the line of the gaze: from the head to what it is looking at, with a circle around it.
    /// Nothing is drawn while there is no target.
    /// </summary>
    public sealed class GazeLine : GazeView
    {
        [Tooltip("Where the line starts: the head (it follows its turn).")]
        [SerializeField] private Transform eyes;

        [SerializeField] private float width = 0.04f;

        [SerializeField] private float circleRadius = 0.35f;

        private LineRenderer line;
        private LineRenderer circle;
        private bool hasTarget;
        private Vector3 target;

        protected override void OnInit()
        {
            line = DebugDraw.CreateLine("Line", transform, DebugPalette.GazeLine, width);
            circle = DebugDraw.CreateLine("TargetCircle", transform, DebugPalette.GazeLine, width);
        }

        public override void OnUpdate(GazeState state)
        {
            hasTarget = state.HasTarget;
            if (hasTarget)
                target = new Vector3(state.Target.X, state.Target.Y, state.Target.Z);
        }

        private void LateUpdate()
        {
            line.enabled = hasTarget;
            circle.enabled = hasTarget;
            if (!hasTarget)
                return;

            Vector3 from = eyes.position;
            Vector3 towards = target - from;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, target - towards.normalized * circleRadius);   // it stops at the circle
            DebugDraw.Circle(circle, target, circleRadius, towards);
        }
    }
}
