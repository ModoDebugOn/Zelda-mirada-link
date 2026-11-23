namespace Gaze.Domain
{
    /// <summary>
    /// Something in the level the head could look at: an eye on the wall, a painting, a cube...
    /// Only the ones the level designer marked as interesting can win the head's attention:
    /// the rest of the scenery is ignored, however close it is.
    /// </summary>
    public sealed class InterestPoint
    {
        /// <summary>To tell them apart outside the domain (whoever made the point gives it).</summary>
        public int Id { get; }

        public Position Position { get; }

        /// <summary>When several are eligible, the highest priority wins.</summary>
        public int Priority { get; }

        /// <summary>Marked by the designer as something the player should notice.</summary>
        public bool IsMarked { get; }

        public InterestPoint(int id, Position position, int priority, bool isMarked)
        {
            Id = id;
            Position = position;
            Priority = priority;
            IsMarked = isMarked;
        }
    }
}
