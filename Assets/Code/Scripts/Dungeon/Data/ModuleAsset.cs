using Code.Scripts.Attributes.Required;
using System;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// Represents a single dungeon module that can be used during dungeon generation.
    /// </summary>
    [Serializable]
    public class ModuleAsset : IEquatable<ModuleAsset>
    {
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

        /// <summary>
        /// Determines whether the current <see cref="ModuleAsset"/> is equal to another <see cref="ModuleAsset"/> instance.
        /// </summary>
        /// <param name="otherAsset">The <see cref="ModuleAsset"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if all fields of the two <see cref="ModuleAsset"/> instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ModuleAsset otherAsset)
        {
            return Equals(modulePrefab, otherAsset.modulePrefab) &&
                   spawnRate.Equals(otherAsset.spawnRate) &&
                   spawnOnce == otherAsset.spawnOnce;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="ModuleAsset"/>.
        /// </summary>
        /// <param name="targetObject">The object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="targetObject"/> is a <see cref="ModuleAsset"/> and is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object targetObject)
        {
            return targetObject is ModuleAsset other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this <see cref="ModuleAsset"/>.
        /// </summary>
        /// <returns>
        /// A hash code that can be used in hashing algorithms and data structures such as a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(modulePrefab, spawnRate, spawnOnce);
        }
    }
}