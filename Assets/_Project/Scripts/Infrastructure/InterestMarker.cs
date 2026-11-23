using System.Collections.Generic;
using UnityEngine;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// Makes an object of the level a point of interest: the eye on the wall, a painting, a cube...
    /// Untick "Is Interesting" and it stays in the list but as scenery: the head ignores it
    /// (useful to show that being close is not enough).
    /// </summary>
    public sealed class InterestMarker : MonoBehaviour
    {
        private static readonly List<InterestMarker> active = new List<InterestMarker>();

        /// <summary>Every enabled marker of the open scenes.</summary>
        public static IReadOnlyList<InterestMarker> Active => active;

        [Tooltip("Marked by the designer as something the player should notice.")]
        [SerializeField] private bool isInteresting = true;

        [Tooltip("When several can be looked at, the highest priority wins (on a tie, the nearest).")]
        [SerializeField] private int priority;

        [Tooltip("Optional: the exact spot to look at (the pupil of the eye). Empty = this object.")]
        [SerializeField] private Transform lookAt;

        public bool IsInteresting
        {
            get => isInteresting;
            set => isInteresting = value;
        }

        public int Priority => priority;

        public Vector3 Point => lookAt != null ? lookAt.position : transform.position;

        private static int lastId;

        /// <summary>A number of its own, to tell it apart from the others while the scene runs.</summary>
        public int Id { get; private set; }

        private void Awake() => Id = ++lastId;

        private void OnEnable() => active.Add(this);

        private void OnDisable() => active.Remove(this);
    }
}
