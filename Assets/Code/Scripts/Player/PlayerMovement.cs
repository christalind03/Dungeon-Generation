using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Scripts.Player
{
    /// <summary>
    /// Manages player movement and gravity application using Unity's Input System and CharacterController component.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private float gravityForce = -9.8f;
        [SerializeField, Min(1f)] private float movementSpeed = 15f;
        
        [Header("References")]
        [SerializeField] private InputActionReference actionReference;
        [SerializeField] private CharacterController characterController;

        private Vector3 playerGravity;
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
        /// Enables the input action when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            inputAction.Enable();
        }

        /// <summary>
        /// Executes per-frame logic for applying gravity and player movement.
        /// </summary>
        private void Update()
        {
            HandleGravity();
            HandleMovement();
        }

        /// <summary>
        /// Applies gravitational force to the player over time.
        /// </summary>
        private void HandleGravity()
        {
            playerGravity.y += gravityForce * Time.deltaTime;
            
            characterController.Move(playerGravity * Time.deltaTime);
        }
        
        /// <summary>
        /// Reads directional input and moves the player relative to their current orientation in world space.
        /// </summary>
        private void HandleMovement()
        {
            Vector2 movementInput = inputAction.ReadValue<Vector2>();
            Vector3 characterMotion = transform.right * movementInput.x + transform.forward * movementInput.y;
            
            characterController.Move(characterMotion * (movementSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Disables the input action when the object becomes disabled or inactive.
        /// </summary>
        private void OnDisable()
        {
            inputAction.Disable();
        }
    }
}
