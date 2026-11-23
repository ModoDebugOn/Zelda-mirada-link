using System;
using System.Linq;
using UnityEngine;

namespace Gaze.Presentation
{
    /// <summary>
    /// The debug layers, to turn them on one by one and explain the trick step by step.
    /// Each layer is an object in the scene: turning it on is activating it. They start as they are in the scene.
    /// When the first one turns on or the last one turns off, the change goes through the debug mode switch.
    /// It does not read the keyboard: whoever uses it tells it which layer to change.
    /// </summary>
    public sealed class DebugLayers : MonoBehaviour
    {
        [Serializable]
        private sealed class DebugLayer
        {
            [SerializeField] private string name;

            [SerializeField] private GameObject target;

            public bool IsOn => target.activeSelf;

            public void TurnOn() => target.SetActive(true);

            public void TurnOff() => target.SetActive(false);
        }

        [SerializeField] private DebugLayer[] layers = new DebugLayer[0];

        /// <summary>What happens on entering or leaving debug mode. By default, it is applied right away.</summary>
        public IDebugSwitch Switch { get; set; } = new ImmediateSwitch();

        public int Count => layers.Length;

        private bool AnyOn => layers.Any(layer => layer.IsOn);

        /// <summary>Turns layer <paramref name="number"/> on or off (starting at 0).</summary>
        public void Toggle(int number)
        {
            if (Switch.IsBusy || number < 0 || number >= layers.Length)
                return;

            DebugLayer layer = layers[number];
            if (layer.IsOn)
                TurnOff(layer);
            else
                TurnOn(layer);
        }

        /// <summary>If any is on, turns them all off; if not, turns them all on.</summary>
        public void ToggleAll()
        {
            if (Switch.IsBusy)
                return;

            if (AnyOn)
                Switch.TurnOff(() => Array.ForEach(layers, layer => layer.TurnOff()));
            else
                Switch.TurnOn(() => Array.ForEach(layers, layer => layer.TurnOn()));
        }

        // The first one to turn on turns debug mode on.
        private void TurnOn(DebugLayer layer)
        {
            if (AnyOn)
                layer.TurnOn();
            else
                Switch.TurnOn(layer.TurnOn);
        }

        // The last one to turn off turns debug mode off.
        private void TurnOff(DebugLayer layer)
        {
            bool isTheLast = layers.All(other => other == layer || !other.IsOn);
            if (isTheLast)
                Switch.TurnOff(layer.TurnOff);
            else
                layer.TurnOff();
        }
    }
}
