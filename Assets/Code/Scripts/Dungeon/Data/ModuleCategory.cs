using System;
using Code.Scripts.Attributes;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// The data related to a category of dungeon modules.
    /// This includes its unique identifier, display name, spawn probability, and optional spawn limits. 
    /// </summary>
    [Serializable]
    public struct ModuleCategory
    {
        /// <summary>
        /// The unique identifier for the category.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private string categoryID;
        
        [Required]
        [SerializeField]
        [Tooltip("The display name of the category")]
        private string categoryTitle;
        
        [SerializeField]
        [Tooltip("If enabled, the number of modules that may spawn from this category is limited to a random value between spawnMin and spawnMax")]
        private bool spawnLimit;
        
        [Required(requireIf: "spawnLimit")]
        [Min(0)]
        [SerializeField]
        [Tooltip("The maximum number of modules that may spawn from this category")]
        private int spawnMax;

        [Required(requireIf: "spawnLimit")]
        [Min(0)]
        [SerializeField]
        [Tooltip("The minimum number of modules that may spawn from this category")]
        private int spawnMin;
        
        [Required]
        [Range(0, 1)]
        [SerializeField]
        [Tooltip("The relative probability of this category being chosen during dungeon generation")]
        private float spawnRate;
        
        /// <summary>
        /// The unique identifier for this category.
        /// </summary>
        public string CategoryID => categoryID;
        
        /// <summary>
        /// The display name of this category.
        /// </summary>
        public string CategoryTitle => categoryTitle;
        
        /// <summary>
        /// Determines whether a limit is enforced on the number of modules that may spawn from this category.
        /// </summary>
        public bool SpawnLimit => spawnLimit;
        
        /// <summary>
        /// The maximum number of modules that may spawn from this category.
        /// </summary>
        public int SpawnMax => spawnMax;
        
        /// <summary>
        /// The minimum number of modules that may spawn from this category.
        /// </summary>
        public int SpawnMin => spawnMin;
        
        /// <summary>
        /// The relative probability of this category being chosen during dungeon generation.
        /// </summary>
        public float SpawnRate => spawnRate;
    }
}