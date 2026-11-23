using System;

namespace Gaze.Presentation
{
    /// <summary>
    /// What happens when debug mode turns on (the first layer appears) or turns off (the last one goes away).
    /// It receives the change to apply and decides when to do it: right away or, when recording,
    /// when the channel's "Modo debug: ON" switch gets into place.
    /// </summary>
    public interface IDebugSwitch
    {
        /// <summary>True while it is in the middle of a change (no others are accepted).</summary>
        bool IsBusy { get; }

        void TurnOn(Action apply);

        void TurnOff(Action apply);
    }

    /// <summary>The default switch: it applies the change right away, with no animation.</summary>
    public sealed class ImmediateSwitch : IDebugSwitch
    {
        public bool IsBusy => false;

        public void TurnOn(Action apply) => apply();

        public void TurnOff(Action apply) => apply();
    }
}
