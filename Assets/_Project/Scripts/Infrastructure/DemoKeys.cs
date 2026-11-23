using Gaze.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// The demo's keys (walking and the camera have their own: PlayerMovement and FollowCamera).
    /// Tab: every debug layer on or off (the "Modo debug: ON" moment). 1-4: each layer on its own.
    /// H: the gaze hint on or off (before and after). T: smooth or snapping head.
    /// P: the character walks the fixed path of the take (any direction takes the control back).
    /// R: everything back to the start.
    /// </summary>
    public sealed class DemoKeys : MonoBehaviour
    {
        private static readonly Key[] LayerKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };

        [SerializeField] private DebugLayers layers;

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (layers != null)
            {
                if (keyboard[Key.Tab].wasPressedThisFrame)
                    layers.ToggleAll();
                for (int i = 0; i < LayerKeys.Length; i++)
                    if (keyboard[LayerKeys[i]].wasPressedThisFrame)
                        layers.Toggle(i);
            }

            if (keyboard[Key.H].wasPressedThisFrame)
                foreach (GazeController gaze in FindObjectsByType<GazeController>())
                    gaze.HintOn = !gaze.HintOn;

            if (keyboard[Key.T].wasPressedThisFrame)
                foreach (GazeController gaze in FindObjectsByType<GazeController>())
                    gaze.Snap = !gaze.Snap;

            if (keyboard[Key.P].wasPressedThisFrame)
                PlayTakePath();

            if (keyboard[Key.R].wasPressedThisFrame)
                Restart();
        }

        /// <summary>The paths that wait to be asked (the character's) set off from their beginning.</summary>
        public static void PlayTakePath()
        {
            foreach (PathWalker walker in FindObjectsByType<PathWalker>(FindObjectsInactive.Include))
                if (!walker.StartsOnItsOwn)
                    walker.Play();
        }

        /// <summary>Everything back to the start: the moving objects set off again, the character stands
        /// where the scene began, and every head looks ahead.</summary>
        public static void Restart()
        {
            foreach (PathWalker walker in FindObjectsByType<PathWalker>(FindObjectsInactive.Include))
            {
                if (walker.StartsOnItsOwn)
                    walker.Play();
                else
                    walker.ResetToStart();
            }
            foreach (GazeController gaze in FindObjectsByType<GazeController>())
                gaze.ResetGaze();
        }
    }
}
