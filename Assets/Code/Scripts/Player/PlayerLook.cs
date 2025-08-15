using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Player
{
    /// <summary>
    /// Handles camera functionality by applying pitch (vertical) and yaw (horizontal) rotation to the camera and player body.
    /// </summary>
    public class PlayerLook : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField, Min(0.1f)] private float lookSensitivity = 1f;

        [Header("References")]
        [SerializeField] private InputActionReference actionReference;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform playerTransform;

        private float pitchRotation;
        private InputAction inputAction;
        
        /// <summary>
        /// Initializes the input action from the provided action reference.
        /// This method is invoked automatically by Unity during the script's loading phase.
        /// </summary>
        private void Awake()
        {
            inputAction = actionReference.action;
        }

        /// <summary>
        /// Locks and hides the cursor when the scene starts.
        /// </summary>
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Enables the input action when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            inputAction.Enable();
        }

        /// <summary>
        /// Reads the input each frame and applies rotation to the camera and player.
        /// Vertical input rotates the camera (pitch), while horizontal input rotates the player (yaw).
        /// </summary>
        private void Update()
        {
            Vector2 lookInput = inputAction.ReadValue<Vector2>() * (lookSensitivity * Time.deltaTime);

            pitchRotation -= lookInput.y;
            pitchRotation = Mathf.Clamp(pitchRotation, -90f, 90f);
            
            cameraTransform.localRotation = Quaternion.Euler(pitchRotation, 0f, 0f);
            playerTransform.Rotate(Vector3.up * lookInput.x);
        }
        
        /// <summary>
        /// Disables the input action when the object becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            inputAction.Disable();
        }

        /// <summary>
        /// Restores the cursor state when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
