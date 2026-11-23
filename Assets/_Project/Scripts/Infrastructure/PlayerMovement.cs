using UnityEngine;
using UnityEngine.InputSystem;

namespace Gaze.Infrastructure
{
    /// <summary>
    /// Lets you walk the character around: WASD, the arrows or the left stick, relative to the camera.
    /// It climbs steps and falls with gravity (CharacterController). The body turns to face where it walks,
    /// and that is the "front" the gaze rule measures from.
    /// If the character has a PathWalker (the fixed path for recording, key P), touching any direction
    /// takes the control back.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Tooltip("Directions are relative to this camera (empty = the main camera).")]
        [SerializeField] private Transform view;

        [Tooltip("Metres per second.")]
        [SerializeField] private float speed = 2.6f;

        [Tooltip("Degrees per second when turning to face where it walks.")]
        [SerializeField] private float turnSpeed = 540f;

        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private PathWalker autopilot;
        private InputAction move;
        private float verticalSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            autopilot = GetComponent<PathWalker>();
            if (view == null && Camera.main != null)
                view = Camera.main.transform;

            move = new InputAction("Move", InputActionType.Value);
            move.AddBinding("<Gamepad>/leftStick");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        }

        private void OnEnable() => move.Enable();

        private void OnDisable() => move.Disable();

        private void OnDestroy() => move.Dispose();

        private void Update()
        {
            // The real devices, unless something else is driving the input (see DemoInput).
            Vector2 raw = DemoInput.IsOverridden ? DemoInput.Override.Move : move.ReadValue<Vector2>();
            Vector2 input = Vector2.ClampMagnitude(raw, 1f);

            // While the fixed path drives the body, the controller is off (it would fight the path).
            if (autopilot != null && autopilot.enabled)
            {
                if (input.sqrMagnitude < 0.04f)
                {
                    controller.enabled = false;
                    return;
                }
                autopilot.enabled = false;   // the player takes the control back
            }
            controller.enabled = true;

            Vector3 direction = RelativeToView(input);

            if (controller.isGrounded && verticalSpeed < 0f)
                verticalSpeed = -2f;   // keeps it pressed to the floor (and to the steps)
            verticalSpeed += gravity * Time.deltaTime;

            controller.Move((direction * speed + Vector3.up * verticalSpeed) * Time.deltaTime);

            if (direction.sqrMagnitude > 1e-4f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction),
                                                              turnSpeed * Time.deltaTime);
        }

        // Up on the stick = away from the camera, on the ground plane.
        private Vector3 RelativeToView(Vector2 input)
        {
            Vector3 forward = view != null ? view.forward : Vector3.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 1e-4f ? forward.normalized : Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            return forward * input.y + right * input.x;
        }
    }
}
