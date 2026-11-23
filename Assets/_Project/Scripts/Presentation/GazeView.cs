using Gaze.Application;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// Base of everything that draws the gaze: the head, the debug layers...
    /// It is passive: it does not know the use case. Whoever drives it (the Infrastructure presenter)
    /// initializes it once and hands it the state; each view overrides only the notices it cares about.
    /// It is an abstract class (and not an interface) so it can be stored in an Inspector list.
    /// </summary>
    public abstract class GazeView : MonoBehaviour
    {
        /// <summary>Prepares the view. Called once, even if its object is turned off.</summary>
        public void Init() => OnInit();

        protected virtual void OnInit() { }

        /// <summary>The head has a new target, or has lost it.</summary>
        public virtual void OnTargetChanged(GazeState state) { }

        /// <summary>Every step, after the specific notice if there was one.</summary>
        public virtual void OnUpdate(GazeState state) { }
    }
}
