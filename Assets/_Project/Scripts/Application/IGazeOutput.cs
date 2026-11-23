namespace Gaze.Application
{
    /// <summary>
    /// Where the gaze reports what happens, as simple data. Whoever implements it (Infrastructure)
    /// decides what to do with it; the use case does not know who is listening or how it gets drawn.
    /// First comes the specific notice (if any) and then, always, OnUpdate.
    /// </summary>
    public interface IGazeOutput
    {
        /// <summary>The head has a new target, or has lost it (then the state has no target).</summary>
        void OnTargetChanged(GazeState state);

        /// <summary>Called every step, with the state already up to date.</summary>
        void OnUpdate(GazeState state);
    }
}
