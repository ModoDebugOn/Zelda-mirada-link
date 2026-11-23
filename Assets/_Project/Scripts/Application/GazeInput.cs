using System.Collections.Generic;

namespace Gaze.Application
{
    /// <summary>What the gaze needs to know each frame, as simple data.</summary>
    public readonly struct GazeInput
    {
        /// <summary>Where the head is (its eyes), in metres.</summary>
        public float HeadX { get; }
        public float HeadY { get; }
        public float HeadZ { get; }

        /// <summary>Where the body faces, in degrees: 0 = +Z, 90 = +X.</summary>
        public float Facing { get; }

        public IReadOnlyList<PointData> Points { get; }

        public GazeTuning Tuning { get; }

        public GazeInput(float headX, float headY, float headZ, float facing,
                         IReadOnlyList<PointData> points, GazeTuning tuning)
        {
            HeadX = headX;
            HeadY = headY;
            HeadZ = headZ;
            Facing = facing;
            Points = points;
            Tuning = tuning;
        }
    }
}
