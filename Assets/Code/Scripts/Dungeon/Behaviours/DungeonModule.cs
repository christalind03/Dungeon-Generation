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
    public class DungeonModule : MonoBehaviour
    {
        [Required]
        [SerializeField]
        [Tooltip("The colliders that define the physical bounds of the dungeon module.")]
        private Collider[] moduleBounds;
        
        [Required]
        [SerializeField]
        [Tooltip("The entrances that can connect this module to other dungeon modules.")]
        private DungeonModuleEntrance[] moduleEntrances;
        
        /// <summary>
        /// A dynamic list of entrances that have not yet been connected to other modules.
        /// This is initialized at runtime and updated as entrances are used.
        /// </summary>
        private List<DungeonModuleEntrance> availableEntrances;

        public Collider[] ModuleBounds => moduleBounds;
        
        /// <summary>
        /// Initializes the currently available (unused) entrances at runtime and is updated as entrances are connected to other modules' entrances.
        /// </summary>
        private void Awake()
        {
            availableEntrances = moduleEntrances.ToList();
        }
        
        public void AddAvailableEntrance(DungeonModuleEntrance moduleEntrance)
        {
            availableEntrances.Add(moduleEntrance);
        }
        
        public bool HasAvailableEntrance()
        {
            return 0 < availableEntrances.Count;
        }
        
        /// <summary>
        /// Removes an entrance from the list of available entrances.
        /// This should be called once an entrance has successfully been connected with another module's entrance.
        /// </summary>
        /// <param name="moduleEntrance"></param>
        public void RemoveAvailableEntrance(DungeonModuleEntrance moduleEntrance)
        {
            availableEntrances.RemoveAll(availableEntrance => availableEntrance == moduleEntrance);
        }
        
        /// <summary>
        /// Selects an unused entrance at random from the module.
        /// </summary>
        /// <returns>
        /// A <see cref="Transform"/> of the selected entrance or <c>null</c> if no entrances are available.
        /// </returns>
        public DungeonModuleEntrance SelectAvailableEntrance()
        {
            if (availableEntrances.Count <= 0) return null;
            
            var entranceIndex = Random.Range(0, availableEntrances.Count);
            return availableEntrances[entranceIndex];
        }
        
        #if UNITY_EDITOR

        /// <summary>
        /// Ensures <see cref="moduleBounds"/> and <see cref="moduleEntrances"/> are not empty and logs errors if requirements are not met.
        /// If validation fails while in Play mode, the editor will immediately exit Play mode to prevent further issues regarding dungeon generation.
        /// </summary>
        private void OnValidate()
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