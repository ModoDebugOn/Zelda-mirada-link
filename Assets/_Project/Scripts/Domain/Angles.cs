using System;

namespace Gaze.Domain
{
    /// <summary>Small helpers for angles in degrees.</summary>
    public static class Angles
    {
        /// <summary>
        /// How much you have to turn to go from one angle to the other, the short way:
        /// between -180 and 180. Positive = to the right.
        /// </summary>
        public static float Delta(float from, float to)
        {
            float delta = (to - from) % 360f;
            if (delta > 180f) delta -= 360f;
            if (delta <= -180f) delta += 360f;
            return delta;
        }

        /// <summary>Moves an angle towards a target by at most maxStep degrees (never past it).</summary>
        public static float MoveTowards(float current, float target, float maxStep)
        {
            float delta = target - current;
            if (MathF.Abs(delta) <= maxStep)
                return target;
            return current + MathF.Sign(delta) * maxStep;
        }
    }
}
