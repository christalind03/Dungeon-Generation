using System;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    [Serializable]
    public class DungeonModuleCategory
    {
        /// <summary>
        /// The unique identifier for the category.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private string categoryID;
        
        [SerializeField]
        [Tooltip("The display name of the category")]
        private string categoryName;
        
        [SerializeField]
        [Tooltip("If enabled, the number of modules that may spawn from this category is limited to a random value between spawnMin and spawnMax")]
        private bool spawnLimit;
        
        [Min(0)]
        [SerializeField]
        [Tooltip("The maximum number of modules that may spawn from this category")]
        private int spawnMax;

        [Min(0)]
        [SerializeField]
        [Tooltip("The minimum number of modules that may spawn from this category")]
        private int spawnMin;
        
        [Min(0)]
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
        public string CategoryName => categoryName;
        
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