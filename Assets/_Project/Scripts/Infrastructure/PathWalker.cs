using UnityEngine;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// Moves an object along a fixed path at a steady speed, so every take is the same:
    /// the cube coming closer and going away, or the character climbing the stairs for a recording.
    /// Enabled in the scene = it sets off on its own (the cube). Disabled = it waits for Play()
    /// (the character: key P), and the player walks it in the meantime.
    /// </summary>
    public sealed class PathWalker : MonoBehaviour
    {
        public enum Ending { Stop, Loop, PingPong }

        [SerializeField] private Transform[] waypoints = new Transform[0];

        [Tooltip("Metres per second.")]
        [Range(0.1f, 6f)] [SerializeField] private float speed = 1.2f;

        [Tooltip("Seconds standing still before setting off.")]
        [SerializeField] private float startDelay = 1f;

        [SerializeField] private Ending ending = Ending.Stop;

        [Tooltip("Turn to face where it is walking (a character), or keep its rotation (an object).")]
        [SerializeField] private bool faceDirection = true;

        [Tooltip("Degrees per second when turning to face the way.")]
        [SerializeField] private float turnSpeed = 240f;

        private int next;
        private int step = 1;
        private float wait;
        private Vector3 startPosition;
        private Quaternion startRotation;

        /// <summary>It was enabled in the scene: it moves without anyone asking.</summary>
        public bool StartsOnItsOwn { get; private set; }

        // Awake runs even if the component starts disabled.
        private void Awake()
        {
            StartsOnItsOwn = enabled;
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Start() => Restart();

        /// <summary>Back to the first waypoint and off along the path (after the start delay).</summary>
        public void Play()
        {
            Restart();
            enabled = true;
        }

        /// <summary>Back to where it was when the scene began, standing still.</summary>
        public void ResetToStart()
        {
            enabled = false;
            Teleport(startPosition, startRotation);
        }

        private void Restart()
        {
            if (waypoints.Length == 0)
                return;

            Quaternion rotation = transform.rotation;
            if (faceDirection && waypoints.Length > 1)
            {
                Vector3 flat = Flat(waypoints[1].position - waypoints[0].position);
                if (flat.sqrMagnitude > 1e-6f)
                    rotation = Quaternion.LookRotation(flat);
            }
            Teleport(waypoints[0].position, rotation);
            next = Mathf.Min(1, waypoints.Length - 1);
            step = 1;
            wait = startDelay;
        }

        // A CharacterController keeps its own idea of where it is: it is switched off to move it by hand.
        private void Teleport(Vector3 position, Quaternion rotation)
        {
            var controller = GetComponent<CharacterController>();
            bool wasOn = controller != null && controller.enabled;
            if (wasOn) controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            if (wasOn) controller.enabled = true;
        }

        private void Update()
        {
            if (waypoints.Length < 2)
                return;
            if (wait > 0f)
            {
                wait -= Time.deltaTime;
                return;
            }

            Vector3 target = waypoints[next].position;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (faceDirection)
            {
                Vector3 flat = Flat(target - transform.position);
                if (flat.sqrMagnitude > 1e-4f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat),
                                                                  turnSpeed * Time.deltaTime);
            }

            if ((transform.position - target).sqrMagnitude < 1e-6f)
                Advance();
        }

        private void Advance()
        {
            int last = waypoints.Length - 1;
            if (next + step >= 0 && next + step <= last)
            {
                next += step;
                return;
            }

            switch (ending)
            {
                case Ending.Loop:
                    next = 0;
                    break;
                case Ending.PingPong:
                    step = -step;
                    next += step;
                    break;
                default:
                    enabled = false;   // arrived: the player has the control again
                    break;
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
