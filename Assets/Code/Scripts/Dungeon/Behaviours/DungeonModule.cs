using Code.Scripts.Attributes;
using Code.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Scripts.Dungeon.Behaviours
{
    /// <summary>
    /// Represents a modular section of a dungeon.
    /// </summary>
    [ExecuteInEditMode]
    public class DungeonModule : MonoBehaviour
    {
        [Required]
        [SerializeField]
        [Tooltip("The colliders that define the physical bounds of the dungeon module.")]
        private Collider[] moduleBounds;
        
        [SerializeField]
        [Tooltip("The entrances that can connect this module to other dungeon modules.")]
        private DungeonPassage[] moduleEntrances;

        /// <summary>
        /// A runtime list of entrances on this <see cref="DungeonModule"/> that are currently available for connections to other modules.
        /// These entrances are updated during dungeon generation.
        /// </summary>
        private List<DungeonPassage> connectableEntrances;
        
        /// <summary>
        /// Gets the collection of <see cref="Collider"/> components that define the physical bounds of this module.
        /// </summary>
        public Collider[] ModuleBounds => moduleBounds;

        /// <summary>
        /// Initializes this module's collection of connectable entrances at runtime.
        /// Each entrance is assigned a reference to its parent <see cref="DungeonModule"/> and activated in its open state to indicate availability for connection.
        /// </summary>
        private void Awake()
        {
            connectableEntrances = moduleEntrances.ToList();
            
            foreach (var connectableEntrance in connectableEntrances)
            {
                connectableEntrance.AssignModule(this);
            }
            
            ToggleConnectableEntrances(true);
        }
        
        /// <summary>
        /// Determines whether this module currently has any connectable entrances available.
        /// </summary>
        /// <returns><c>true</c> if one or more connectable entrances exist; otherwise, <c>false</c>.</returns>
        public bool ContainsConnectableEntrance()
        {
            return 0 < connectableEntrances.Count;
        }
        
        /// <summary>
        /// Registers a new connectable entrance to this module's list of connectable entrances.
        /// </summary>
        /// <param name="dungeonPassage">The <see cref="DungeonPassage"/> instance representing the entrance to be registered.</param>
        public void RegisterConnectableEntrance(DungeonPassage dungeonPassage)
        {
            connectableEntrances.Add(dungeonPassage);
        }
        
        /// <summary>
        /// Removes an entrance from the list of available entrances.
        /// This should be called once an entrance has successfully been connected with another module's entrance.
        /// </summary>
        /// <param name="dungeonPassage">The <see cref="DungeonPassage"/> instance to be removed.</param>
        public void RemoveConnectableEntrance(DungeonPassage dungeonPassage)
        {
            connectableEntrances.RemoveAll(availableEntrance => availableEntrance == dungeonPassage);
        }
        
        /// <summary>
        /// Selects an unused entrance at random from the module.
        /// </summary>
        /// <returns>
        /// A <see cref="Transform"/> of the selected entrance or <c>null</c> if no entrances are available.
        /// </returns>
        public DungeonPassage SelectConnectableEntrance()
        {
            if (connectableEntrances.Count <= 0) return null;
            
            var entranceIndex = Random.Range(0, connectableEntrances.Count);
            return connectableEntrances[entranceIndex];
        }
        
        /// <summary>
        /// Sets the current active state of all connectable entrances belonging to this module.
        /// When enabled, entrances are visually and functionally open for connection.
        /// When disabled, entrances are closed and unavailable for further connections.
        /// </summary>
        public void ToggleConnectableEntrances(bool isOpen)
        {
            foreach (var connectableEntrance in connectableEntrances)
            {
                connectableEntrance.EnableEntrance(isOpen);
            }
        }
        
        #if UNITY_EDITOR

        /// <summary>
        /// Ensures <see cref="moduleBounds"/> and <see cref="moduleEntrances"/> are not empty and logs errors if requirements are not met.
        /// If validation fails while in Play mode, the editor will immediately exit Play mode to prevent further issues regarding dungeon generation.
        /// </summary>
        public void OnValidate()
        {
            ScriptValidator.LogError(
                this,
                (moduleBounds.Length <= 0, $"<b>{nameof(moduleBounds)}</b> must contain at least one element."),
                (moduleEntrances.Length <= 0, $"<b>{nameof(moduleEntrances)}</b> must contain at least one element.")
            );
        }
                
        #endif
    }
}