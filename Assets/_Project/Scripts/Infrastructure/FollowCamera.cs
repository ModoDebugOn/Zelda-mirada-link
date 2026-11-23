using UnityEngine;
using UnityEngine.InputSystem;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// A third-person camera that follows the character. Turn it around with Q/E, the right mouse button
    /// or the right stick; the mouse wheel brings it closer or further.
    /// </summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [Tooltip("Where the camera is around the target, in degrees: 0 = behind it if it faces +Z (world).")]
        [SerializeField] private float yaw;

        [Range(2f, 14f)] [SerializeField] private float distance = 5.5f;

        [Tooltip("Height of the camera over the target's feet.")]
        [SerializeField] private float height = 3.2f;

        [Tooltip("Height it looks at, over the target's feet (about the chest).")]
        [SerializeField] private float lookHeight = 1.1f;

        [Tooltip("How quickly it catches up (higher = stiffer).")]
        [SerializeField] private float smoothing = 8f;

        [Tooltip("Degrees per second with Q/E or the right stick.")]
        [SerializeField] private float orbitSpeed = 120f;

        [Tooltip("Degrees per pixel dragging with the right mouse button.")]
        [SerializeField] private float mouseSensitivity = 0.25f;

        /// <summary>Where the camera is around the target, in degrees (0 = behind it if it faces +Z).</summary>
        public float Yaw
        {
            get => yaw;
            set => yaw = value;
        }

        public float Distance
        {
            get => distance;
            set => distance = value;
        }

        /// <summary>Height of the camera over the target's feet.</summary>
        public float Height
        {
            get => height;
            set => height = value;
        }

        /// <summary>Height it looks at, over the target's feet.</summary>
        public float LookHeight
        {
            get => lookHeight;
            set => lookHeight = value;
        }

        /// <summary>Puts the camera in place at once, with no easing (for a shot that has to start framed).</summary>
        public void Snap() => Place(1f);

        private void Start() => Place(1f);

        private void LateUpdate()
        {
            // When something else drives the input (see DemoInput), the player's devices do not turn the camera.
            if (!DemoInput.IsOverridden)
                ReadInput();
            Place(1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }

        private void ReadInput()
        {
            float turn = 0f;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard[Key.Q].isPressed) turn -= 1f;
                if (keyboard[Key.E].isPressed) turn += 1f;
            }
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
                turn += gamepad.rightStick.ReadValue().x;
            yaw += turn * orbitSpeed * Time.deltaTime;

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                    yaw += mouse.delta.ReadValue().x * mouseSensitivity;
                distance = Mathf.Clamp(distance - mouse.scroll.ReadValue().y * 0.004f, 2f, 14f);
            }
        }

        private void Place(float blend)
        {
            if (target == null)
                return;

            Vector3 feet = target.position;
            Vector3 focus = feet + Vector3.up * lookHeight;
            Vector3 desired = feet + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, -distance) + Vector3.up * height;

            transform.position = Vector3.Lerp(transform.position, desired, blend);
            transform.LookAt(focus);
        }
    }
}
