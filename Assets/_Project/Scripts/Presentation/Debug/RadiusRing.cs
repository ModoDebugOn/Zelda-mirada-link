using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// Layer 1, the radius of interest: a circle on the floor around the character.
    /// Only what is inside it can catch the head's attention.
    /// </summary>
    public sealed class RadiusRing : GazeView
    {
        [Tooltip("The character's feet: the circle is drawn around it, on the floor.")]
        [SerializeField] private Transform feet;

        [SerializeField] private float width = 0.05f;

        private LineRenderer ring;
        private float radius;

        protected override void OnInit() => ring = DebugDraw.CreateLine("Ring", transform, DebugPalette.Radius, width);

        public override void OnUpdate(GazeState state) => radius = state.Radius;

        // In LateUpdate: the character has already moved this frame.
        private void LateUpdate() => DebugDraw.Circle(ring, feet.position + Vector3.up * 0.03f, radius, Vector3.up);
    }
}
