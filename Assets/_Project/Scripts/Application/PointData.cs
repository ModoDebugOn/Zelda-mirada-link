namespace Gaze.Application
{
    /// <summary>A point of interest of the scene, as simple data (whoever reads the scene builds it).</summary>
    public readonly struct PointData
    {
        public int Id { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public int Priority { get; }

        /// <summary>Marked by the designer as something the player should notice.</summary>
        public bool IsMarked { get; }

        public PointData(int id, float x, float y, float z, int priority, bool isMarked)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Priority = priority;
            IsMarked = isMarked;
        }
    }
}
