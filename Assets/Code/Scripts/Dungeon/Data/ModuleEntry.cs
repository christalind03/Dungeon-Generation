using System;
using Code.Scripts.Attributes;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// Represents a single dungeon module that can be used during dungeon generation.
    /// </summary>
    [Serializable]
    public struct ModuleEntry
    {
        [Required]
        [SerializeField]
        [Tooltip("The identifier of the category this module belongs to.")]
        private string moduleCategory;
        
        [Required(displayLabel: false)]
        [SerializeField]
        [Tooltip("The prefab associated with this dungeon module.")]
        private GameObject modulePrefab;
        
        [Required(displayLabel: false)]
        [Range(0, 1)]
        [SerializeField]
        [Tooltip("The relative probability of this module being chosen during dungeon generation.")]
        public float spawnRate;
        
        [SerializeField]
        [Tooltip("Determines whether this module can only appear once in the generated dungeon.")]
        private bool spawnOnce;
        
        /// <summary>
        /// The identifier of the category this module belongs to.
        /// </summary>
        public string ModuleCategory => moduleCategory;
        
        /// <summary>
        /// The prefab associated with this dungeon module.
        /// </summary>
        public GameObject ModulePrefab => modulePrefab;
        
        /// <summary>
        /// The relative probability of this module being chosen during dungeon generation.
        /// </summary>
        public float SpawnRate => spawnRate;
        
        /// <summary>
        /// Determines whether this module can only appear once in the generated dungeon.
        /// </summary>
        public bool SpawnOnce => spawnOnce;
    }
}