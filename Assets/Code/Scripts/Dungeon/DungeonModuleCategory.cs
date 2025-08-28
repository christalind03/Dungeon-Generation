using System;
using UnityEngine;

namespace Code.Scripts.Dungeon
{
    [Serializable]
    public class DungeonModuleCategory
    {
        [HideInInspector]
        [SerializeField]
        private string categoryID;
        
        [SerializeField]
        private string categoryName;
        
        [SerializeField]
        [Tooltip("Restrict the total number of modules within this category to the spawnRange value; otherwise modules will spawn freely based on the spawnWeight")]
        private bool spawnLimit;
        
        [Min(0)]
        [SerializeField]
        [Tooltip("The maximum number in which modules under this category can appear in the dungeon")]
        private int spawnMax;

        [Min(0)]
        [SerializeField]
        [Tooltip("The minimum number in which modules under this category can appear in the dungeon")]
        private int spawnMin;
        
        [Min(0)]
        [SerializeField]
        [Tooltip("The relative probability for this category being chosen during dungeon generation")]
        private float spawnRate;
        
        public string CategoryID => categoryID;
        public string CategoryName => categoryName;
        public bool SpawnLimit => spawnLimit;
        public int SpawnMax => spawnMax;
        public int SpawnMin => spawnMin;
        public float SpawnRate => spawnRate;
    }
}