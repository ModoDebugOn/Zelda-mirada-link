using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// Turns the character's head to the angle that arrives. It only turns sideways (around Y):
    /// the capsule's head is round, so no seam shows.
    /// </summary>
    public sealed class HeadView : GazeView
    {
        [Tooltip("The Head child of the capsule (KitPersonajes, Separate Head on).")]
        [SerializeField] private Transform head;

        public override void OnUpdate(GazeState state) =>
            head.localRotation = Quaternion.Euler(0f, state.HeadAngle, 0f);
    }
}
