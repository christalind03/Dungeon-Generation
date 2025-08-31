using System;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// Represents a single dungeon module that can be used during dungeon generation.
    /// </summary>
    [Serializable]
    public class DungeonModule
    {
        [SerializeField]
        [Tooltip("The identifier of the category this module belongs to.")]
        private string moduleCategory;
        
        [SerializeField]
        [Tooltip("The prefab associated with this dungeon module.")]
        private GameObject modulePrefab;
        
        [Min(0)]
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