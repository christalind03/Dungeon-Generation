using Code.Scripts.Attributes.Required;
using System;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// The data related to a category of dungeon modules.
    /// This includes its unique identifier, display name, spawn probability, and optional spawn limits. 
    /// </summary>
    [Serializable]
    public class ModuleCategory : IEquatable<ModuleCategory>
    {
        [Required(normalizeLayout: true)]
        [SerializeField]
        [Tooltip("The display name of the category")]
        private string categoryTitle;
        
        [Required(normalizeLayout: true)]
        [SerializeField]
        [Tooltip("If enabled, the number of modules that may spawn from this category is limited to a random value between spawnMin and spawnMax")]
        private bool spawnLimits;
        
        [Required(normalizeLayout: true, requireIf: nameof(spawnLimits))]
        [Min(0)]
        [SerializeField]
        [Tooltip("The maximum number of modules that may spawn from this category")]
        private int spawnMaximum;

        [Required(normalizeLayout: true, requireIf: nameof(spawnLimits))]
        [Min(0)]
        [SerializeField]
        [Tooltip("The minimum number of modules that may spawn from this category")]
        private int spawnMinimum;

        [Required(normalizeLayout: true)]
        [SerializeField]
        [Tooltip("If enabled, at least one module from this category is guaranteed to spawn (nullifies <c>spawnRate</c>)")]
        private bool spawnRequired;
        
        [Required(normalizeLayout: true, requireIf: "!" + nameof(spawnRequired))]
        [Range(0, 1)]
        [SerializeField]
        [Tooltip("The relative probability of this category being chosen during dungeon generation")]
        private float spawnRate;
        
        /// <summary>
        /// The display name of this category.
        /// </summary>
        public string CategoryTitle => categoryTitle;
        
        /// <summary>
        /// Determines whether a limit is enforced on the number of modules that may spawn from this category.
        /// </summary>
        public bool SpawnLimits => spawnLimits;
        
        /// <summary>
        /// The maximum number of modules that may spawn from this category.
        /// </summary>
        public int SpawnMaximum => spawnMaximum;
        
        /// <summary>
        /// The minimum number of modules that may spawn from this category.
        /// </summary>
        public int SpawnMinimum => spawnMinimum;

        /// <summary>
        /// Determines whether at least one module from this category is guaranteed to spawn (nullifies <see cref="SpawnRate"/>)
        /// </summary>
        public bool SpawnRequired => spawnRequired;
        
        /// <summary>
        /// The relative probability of this category being chosen during dungeon generation.
        /// </summary>
        public float SpawnRate => spawnRate;

        /// <summary>
        /// Determines whether the current <see cref="ModuleCategory"/> is equal to another <see cref="ModuleCategory"/> instance.
        /// </summary>
        /// <param name="otherCategory">The <see cref="ModuleCategory"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if all fields of the two <see cref="ModuleCategory"/> instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ModuleCategory otherCategory)
        {
            return categoryTitle == otherCategory.categoryTitle &&
                   spawnLimits == otherCategory.spawnLimits &&
                   spawnMaximum == otherCategory.spawnMaximum &&
                   spawnMinimum == otherCategory.spawnMinimum &&
                   spawnRequired == otherCategory.spawnRequired &&
                   spawnRate.Equals(otherCategory.spawnRate);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="ModuleCategory"/>.
        /// </summary>
        /// <param name="targetObject">The object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="targetObject"/> is a <see cref="ModuleCategory"/> and is equal to the current instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object targetObject)
        {
            return targetObject is ModuleCategory other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this <see cref="ModuleCategory"/>.
        /// </summary>
        /// <returns>
        /// A hash code that can be used in hashing algorithms and data structures such as a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(categoryTitle, spawnLimits, spawnMaximum, spawnMinimum, spawnRequired, spawnRate);
        }
    }
}