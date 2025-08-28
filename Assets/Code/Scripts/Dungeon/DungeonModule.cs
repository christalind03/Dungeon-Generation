using System;
using UnityEngine;

namespace Code.Scripts.Dungeon
{
    [Serializable]
    public class DungeonModule
    {
        [SerializeField]
        [Tooltip("The category this module belongs to")]
        private string moduleCategory;
        
        [SerializeField]
        [Tooltip("The prefab representing the dungeon module")]
        private GameObject modulePrefab;
        
        [Min(0)]
        [SerializeField]
        [Tooltip("The relative probability for this module being chosen during dungeon generation")]
        public float spawnRate;
        
        [SerializeField]
        [Tooltip("If enabled, this module can only appear once in the dungeon")]
        private bool spawnOnce;
        
        public string ModuleCategory => moduleCategory;
        public GameObject ModulePrefab => modulePrefab;
        public float SpawnRate => spawnRate;
        public bool SpawnOnce => spawnOnce;
    }
}