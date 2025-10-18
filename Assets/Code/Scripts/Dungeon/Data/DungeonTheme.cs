using Code.Scripts.Attributes;
using Code.Scripts.Utils;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// Defines the dataset used for procedural dungeon generation.
    /// This includes the theme's name, available module categories, and the individual models that can be spawned.
    /// </summary>
    [CreateAssetMenu(fileName = "Dungeon_Theme", menuName = "Dungeon Theme")]
    public class DungeonTheme : ScriptableObject
    {
        [Required]
        [SerializeField]
        [Tooltip("The display name of the dungeon theme")]
        private string themeTitle;

        [Required]
        [Min(0)]
        [SerializeField]
        [Tooltip("The maximum number of modules that can exist within this theme")]
        private int maximumModules;
        
        [Required]
        [Min(0)]
        [SerializeField]
        [Tooltip("The minimum number of modules that can exist within this theme")]
        private int minimumModules;
        
        [SerializeField]
        [Tooltip("Defines which module assets belong to which module category")]
        private ModuleData[] moduleData;
        
        /// <summary>
        /// The display name of the dungeon theme.
        /// </summary>
        public string ThemeTitle => themeTitle;
        
        /// <summary>
        /// The maximum number of modules that can exist within this theme.
        /// </summary>
        public int MaximumModules => maximumModules;
        
        /// <summary>
        /// The minimum number of modules that can exist within this theme.
        /// </summary>
        public int MinimumModules => minimumModules;
        
        public ModuleData[] ModuleData => moduleData;
        
        #if UNITY_EDITOR
        
        /// <summary>
        /// Validates this theme's configuration and logs errors to help catch setup issues early.
        /// </summary>
        public void OnValidate()
        {
            if (EditorApplication.isPlaying) return;
            
            ScriptValidator.LogError(
                this,
                (maximumModules < minimumModules, $"<b>{nameof(maximumModules)}</b> must be greater than <b>{nameof(minimumModules)}</b>"),
                (moduleData.Length <= 0, $"<b>{nameof(moduleData)}</b> must contain at least one element."),
                (ContainsInvalidBounds(), $"All <b>{nameof(moduleData)}</b> entries contain spawn limits that exceed this theme's bounds."),
                (ContainsInvalidWeights(), $"All <b>{nameof(moduleData)}</b> entries' cumulative spawn rate should roughly equal 1."),
                (ContainsUnassignedCategories(), $"All <b>{nameof(moduleData)}</b> entries must have a valid list of <b>{nameof(ModuleAsset)}.")
            );
        }
        
        /// <summary>
        /// Checks whether any module category has spawn limits that exceed this theme's defined <see cref="minimumModules"/> and <see cref="maximumModules"/> bounds.
        /// </summary>
        /// <returns>
        /// <c>true</c> if all module categories have invalid spawn limits; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsInvalidBounds()
        {
            foreach (var moduleElement in moduleData)
            {
                var moduleCategory = moduleElement.ModuleCategory;
                if (moduleCategory.SpawnLimits)
                {
                    if (maximumModules < moduleCategory.SpawnMinimum) continue;
                }
                
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Validates the cumulative spawn weights of both <see cref="ModuleAsset"/> and <see cref="ModuleCategory"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if any category's assets or the combined categories have a total spawn weight outside the valid range; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsInvalidWeights()
        {
            var categoryWeight = 0f;
            
            foreach (var moduleElement in moduleData)
            {
                var assetWeight = moduleElement.ModuleAssets.Sum(moduleAsset => moduleAsset.SpawnRate);
                if (assetWeight is < 0.99f or > 1f) return true;
                
                categoryWeight += moduleElement.ModuleCategory.SpawnRate;
            }

            return categoryWeight is < 0.99f or > 1f;
        }
        
        /// <summary>
        /// Determines whether there are module categories that do not have a valid list of <see cref="ModuleAsset"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if one or more categories do not contain a valid list of <see cref="ModuleAsset"/>; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsUnassignedCategories()
        {
            return moduleData.Any(moduleElement => moduleElement.ModuleAssets.Length <= 0);
        }
        
        #endif
    }
}