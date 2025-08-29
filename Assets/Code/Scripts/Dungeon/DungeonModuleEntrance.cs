using Code.Scripts.Attributes;
using UnityEngine;

namespace Code.Scripts.Dungeon
{
    /// <summary>
    /// Represents a single entrance/exit point within a <see cref="DungeonModuleAsset"/>.
    /// </summary>
    public class DungeonModuleEntrance : MonoBehaviour
    {
        [Required]
        [SerializeField]
        [Tooltip("The world-space orientation and position of the entrance.")]
        private Transform entrancePoint;
        
        [Header("Entrance Renderers")]
        [Required]
        [SerializeField]
        [Tooltip("The GameObject activated when the entrance is open and connected to another module.")]
        private GameObject openState;
        
        [Required]
        [SerializeField]
        [Tooltip("The GameObject activated when the entrance is closed and unavailable for connection.")]
        private GameObject closedState;

        /// <summary>
        /// The world-space orientation and position of the entrance.
        /// </summary>
        public Transform EntrancePoint => entrancePoint;

        /// <summary>
        /// Initializes the entrance to its default state by enabling the open entrance visuals.
        /// </summary>
        private void Awake()
        {
            EnableEntrance(true);
        }

        /// <summary>
        /// Enables or disables the entrance by toggling between the open and closed state renderers.
        /// </summary>
        /// <param name="isOpen">If <c>true</c>, the entrance is rendered as open; otherwise it is rendered as closed.</param>
        public void EnableEntrance(bool isOpen)
        {
            openState.SetActive(isOpen);
            closedState.SetActive(!isOpen);
        }
    }
}