using Gaze.Application;
using Gaze.Presentation;
using UnityEngine;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// Receives what the gaze reports and passes it to the views: it is what updates the presentation.
    /// The use case only knows the output interface; the views only know what reaches them.
    /// It needs to be on the same object as the GazeController.
    /// </summary>
    public sealed class GazePresenter : MonoBehaviour, IGazeOutput
    {
        [Tooltip("What draws the gaze: the head and the debug layers. The order does not matter.")]
        [SerializeField] private GazeView[] views = new GazeView[0];

        // In Awake (the controller starts stepping in Update). The views of switched-off layers are
        // initialized too: they do not depend on their object being active.
        private void Awake()
        {
            foreach (GazeView view in views)
                view.Init();
        }

        public void OnTargetChanged(GazeState state)
        {
            foreach (GazeView view in views)
                view.OnTargetChanged(state);
        }

        public void OnUpdate(GazeState state)
        {
            foreach (GazeView view in views)
                view.OnUpdate(state);
        }
    }
}
