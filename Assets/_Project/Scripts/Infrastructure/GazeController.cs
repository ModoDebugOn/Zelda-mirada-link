using System.Collections.Generic;
using Gaze.Application;
using UnityEngine;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// Runs one character's gaze: every frame it reads the scene (where the body is and faces, and the
    /// points of interest), hands it to the use case as simple data and lets the presenter update the views.
    /// The sliders are the rule's values: our own, not the game's.
    /// It needs a GazePresenter on the same object.
    /// </summary>
    [RequireComponent(typeof(GazePresenter))]
    public sealed class GazeController : MonoBehaviour
    {
        [Tooltip("The character's body: where it faces is the front of the rule.")]
        [SerializeField] private Transform body;

        [Tooltip("At eye height, child of the body: distances and angles are measured from here.")]
        [SerializeField] private Transform eyeLevel;

        [Header("Rule (our own values)")]
        [Tooltip("How far something can be and still catch the head's attention, in metres.")]
        [Range(0.5f, 12f)] [SerializeField] private float radius = 4f;

        [Tooltip("Degrees to each side of the front inside which things are noticed.")]
        [Range(0f, 180f)] [SerializeField] private float attentionAngle = 110f;

        [Tooltip("How far the head can turn to each side, in degrees.")]
        [Range(0f, 120f)] [SerializeField] private float neckLimit = 70f;

        [Tooltip("Degrees per second.")]
        [Range(20f, 1000f)] [SerializeField] private float turnSpeed = 220f;

        [Tooltip("The head snaps instead of turning smoothly (to compare).")]
        [SerializeField] private bool snap;

        [Tooltip("The gaze hint: off, the head always looks ahead (the \"before\").")]
        [SerializeField] private bool hintOn = true;

        private readonly List<PointData> points = new List<PointData>();
        private UpdateGaze gaze;

        public bool HintOn
        {
            get => hintOn;
            set => hintOn = value;
        }

        public bool Snap
        {
            get => snap;
            set => snap = value;
        }

        public float Radius
        {
            get => radius;
            set => radius = value;
        }

        /// <summary>The last state, for whoever wants to read it (recording tools).</summary>
        public GazeState State { get; private set; }

        private void Awake() => gaze = new UpdateGaze(GetComponent<GazePresenter>());

        private void Update()
        {
            points.Clear();
            foreach (InterestMarker marker in InterestMarker.Active)
            {
                Vector3 p = marker.Point;
                points.Add(new PointData(marker.Id, p.x, p.y, p.z, marker.Priority, marker.IsInteresting));
            }

            Vector3 forward = body.forward;
            float facing = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            Vector3 eyes = eyeLevel.position;
            var tuning = new GazeTuning(radius, attentionAngle, neckLimit,
                                        snap ? float.PositiveInfinity : turnSpeed, hintOn);

            State = gaze.Step(new GazeInput(eyes.x, eyes.y, eyes.z, facing, points, tuning), Time.deltaTime);
        }

        /// <summary>Head back to the front, at once (for restarting a take).</summary>
        public void ResetGaze() => gaze?.Reset();
    }
}
