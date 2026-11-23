namespace Gaze.Application
{
    /// <summary>
    /// The adjustable values of the demo (the sliders), as simple data. Our own values, not the game's.
    /// </summary>
    public readonly struct GazeTuning
    {
        /// <summary>Metres.</summary>
        public float Radius { get; }

        /// <summary>Degrees to each side of the front of the body.</summary>
        public float AttentionAngle { get; }

        /// <summary>Degrees the head can turn to each side.</summary>
        public float NeckLimit { get; }

        /// <summary>Degrees per second. Infinity = the head snaps.</summary>
        public float TurnSpeed { get; }

        /// <summary>The gaze hint switched on. Off = the head always looks ahead (the "before").</summary>
        public bool HintOn { get; }

        public GazeTuning(float radius, float attentionAngle, float neckLimit, float turnSpeed, bool hintOn)
        {
            Radius = radius;
            AttentionAngle = attentionAngle;
            NeckLimit = neckLimit;
            TurnSpeed = turnSpeed;
            HintOn = hintOn;
        }
    }
}
