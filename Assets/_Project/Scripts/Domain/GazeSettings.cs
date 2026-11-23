using System;

namespace Gaze.Domain
{
    /// <summary>
    /// The numbers of the rule. They are our own values: the game does not say how it decides.
    /// </summary>
    public sealed class GazeSettings
    {
        /// <summary>How far a point can be and still catch the head's attention, in metres.</summary>
        public float Radius { get; }

        /// <summary>
        /// Half the angle, in degrees from the front of the body, inside which points are noticed.
        /// Something behind you (outside it) is ignored.
        /// </summary>
        public float AttentionAngle { get; }

        /// <summary>
        /// How far the head can turn to each side, in degrees. If the point is further round,
        /// the head stops here: it cannot twist all the way.
        /// </summary>
        public float NeckLimit { get; }

        /// <summary>Maximum turning speed of the head, in degrees per second. Infinity = it snaps.</summary>
        public float TurnSpeed { get; }

        public GazeSettings(float radius, float attentionAngle, float neckLimit, float turnSpeed)
        {
            if (radius < 0f) throw new ArgumentOutOfRangeException(nameof(radius));
            if (attentionAngle < 0f || attentionAngle > 180f) throw new ArgumentOutOfRangeException(nameof(attentionAngle));
            if (neckLimit < 0f || neckLimit > 180f) throw new ArgumentOutOfRangeException(nameof(neckLimit));
            if (turnSpeed <= 0f) throw new ArgumentOutOfRangeException(nameof(turnSpeed));

            Radius = radius;
            AttentionAngle = attentionAngle;
            NeckLimit = neckLimit;
            TurnSpeed = turnSpeed;
        }
    }
}
