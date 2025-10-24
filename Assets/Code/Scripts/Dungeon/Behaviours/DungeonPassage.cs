using Code.Scripts.Attributes.Required;
using System;
using UnityEngine;

namespace Code.Scripts.Dungeon.Behaviours
{
    /// <summary>
    /// Represents a single entrance/exit point within a <see cref="Behaviours.DungeonModule"/>.
    /// </summary>
    [Serializable]
    public class DungeonPassage
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
        /// The <see cref="DungeonModule"/> in which this entrance is associated with.
        /// </summary>
        public DungeonModule DungeonModule { get; private set; }
        
        /// <summary>
        /// The world-space orientation and position of the entrance.
        /// </summary>
        public Transform EntrancePoint => entrancePoint;
        
        /// <summary>
        /// Assigns the <see cref="DungeonModule"/> that owns or contains this object.
        /// This establishes a parent-child relationship between the module and its component.
        /// </summary>
        /// <param name="dungeonModule">The <see cref="DungeonModule"/> instance to associate as this object's parent.</param>
        public void AssignModule(DungeonModule dungeonModule)
        {
            DungeonModule = dungeonModule;
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