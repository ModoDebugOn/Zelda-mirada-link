namespace Gaze.Domain
{
    /// <summary>
    /// The head's turn, relative to the body: 0 = looking ahead, positive = to the right.
    /// It never jumps: each step it moves towards the angle it is asked for, at most at the turning speed.
    /// With an infinite speed it snaps (to compare a smooth turn with an abrupt one).
    /// </summary>
    public sealed class HeadTurn
    {
        public float Angle { get; private set; }

        /// <summary>Turns towards <paramref name="target"/> during <paramref name="deltaTime"/> seconds.</summary>
        public void Step(float target, float turnSpeed, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;
            Angle = Angles.MoveTowards(Angle, target, turnSpeed * deltaTime);
        }

        /// <summary>Back to looking ahead, at once.</summary>
        public void Reset() => Angle = 0f;
    }
}
