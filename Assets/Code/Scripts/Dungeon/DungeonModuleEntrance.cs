using Code.Scripts.Attributes;
using UnityEngine;

namespace Code.Scripts.Dungeon
{
    /// <summary>
    /// Represents a single entrance/exit point within a <see cref="DungeonModule"/>.
    /// </summary>
    public class DungeonModuleEntrance : MonoBehaviour
    {
        [Required, SerializeField] private Transform entrancePoint;
        
        [Header("Entrance Renderers")]
        [Required, SerializeField] private GameObject openState;
        [Required, SerializeField] private GameObject closedState;

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