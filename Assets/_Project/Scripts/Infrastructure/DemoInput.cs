using UnityEngine;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// A replacement for the player's input: while it is active, the character and the camera ignore the real
    /// keyboard, mouse and gamepad and read this instead. It lets something else (for example a tool that records
    /// the same take over and over) drive the character. Nothing here knows who plugs in.
    /// </summary>
    public interface IInputOverride
    {
        /// <summary>True while it is in charge.</summary>
        bool Active { get; }

        /// <summary>Where to walk, like a stick: x = right, y = forward (relative to the camera), up to length 1.</summary>
        Vector2 Move { get; }
    }

    /// <summary>The place where an <see cref="IInputOverride"/> plugs in. Null = the real devices.</summary>
    public static class DemoInput
    {
        public static IInputOverride Override { get; set; }

        /// <summary>True if something is driving the input instead of the player.</summary>
        public static bool IsOverridden => Override != null && Override.Active;
    }
}
