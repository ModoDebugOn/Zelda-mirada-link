using System;

namespace Gaze.Domain
{
    /// <summary>
    /// A point in the world, in metres. Y is up and the front of the world is +Z, as in Unity.
    /// The head only turns around Y, so the angles are measured on the ground plane (X and Z).
    /// </summary>
    public readonly struct Position
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Straight-line distance, height included.</summary>
        public float DistanceTo(Position other)
        {
            float dx = other.X - X, dy = other.Y - Y, dz = other.Z - Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Where the other point is, seen from above, in degrees: 0 = straight ahead of the world (+Z),
        /// 90 = to the right (+X), -90 = to the left. The same as Unity's turn around Y.
        /// </summary>
        public float HeadingTo(Position other)
        {
            float dx = other.X - X, dz = other.Z - Z;
            return MathF.Atan2(dx, dz) * (180f / MathF.PI);
        }
    }
}
