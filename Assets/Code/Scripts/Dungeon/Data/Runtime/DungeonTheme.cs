using Code.Scripts.Attributes.Required;
using Code.Scripts.Utils;
using System.Linq;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data.Runtime
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
            ObjectValidator.AssertConditions(
                this,
                (maximumModules < minimumModules, $"<b>{nameof(maximumModules)}</b> must be greater than <b>{nameof(minimumModules)}</b>"),
                (moduleData.Length <= 0, $"<b>{nameof(moduleData)}</b> must contain at least one element."),
                (ContainsInvalidLimits(), $"All <b>{nameof(moduleData)}</b> entries contain spawn limits that exceed this theme's bounds."),
                (ContainsInvalidWeights(), $"All <b>{nameof(moduleData)}</b> entries' cumulative spawn rate should roughly equal 1."),
                (ContainsImpossibleRequirements(), $"All <b>{nameof(moduleData)}</b> entries must have sufficient unique modules to satisfy their spawn requirements."),
                (ContainsUnassignedCategories(), $"All <b>{nameof(moduleData)}</b> entries must have a valid list of <b>{nameof(ModuleAsset)}."),
                (ValidateRequiredModules(), $"The minimum number of required modules must be between <b>{minimumModules}</b> and <b>{maximumModules}</b>.")
            );
        }
        
        /// <summary>
        /// Checks whether any module category has spawn limits that exceed this theme's defined <see cref="minimumModules"/> and <see cref="maximumModules"/> bounds.
        /// </summary>
        /// <returns>
        /// <c>true</c> if all module categories have invalid spawn limits; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsInvalidLimits()
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
        /// Determines whether any <see cref="ModuleCategory"/> in <see cref="moduleData"/> has spawn limits that make it impossible to satisfy.
        /// </summary>
        /// <remarks>
        /// A <see cref="ModuleCategory"/> is considered to be impossible if:
        /// <list type="bullet">
        /// <item><description>The <see cref="ModuleCategory.SpawnLimits"/> flag is set.</description></item>
        /// <item>
        ///     <description>
        ///         The number of unique <see cref="ModuleAsset"/> entries that can only spawn once is less than the <see cref="ModuleCategory"/>'s <see cref="ModuleCategory.SpawnMinimum"/> requirement.
        ///     </description>
        /// </item>
        /// </list> 
        /// </remarks>
        /// <returns><c>true</c> if one or more <see cref="ModuleCategory"/> have unsatisfiable spawn requirements; otherwise <c>false</c>.</returns>
        private bool ContainsImpossibleRequirements()
        {
            foreach (var moduleElement in moduleData)
            {
                var moduleAssets = moduleElement.ModuleAssets;
                var moduleCategory = moduleElement.ModuleCategory;
                var uniqueCount = moduleAssets.Count(moduleAsset => moduleAsset.SpawnOnce);

                var allUnique = moduleAssets.Length == uniqueCount;
                var spaceUnavailable = uniqueCount < moduleCategory.SpawnMinimum;
                
                if (allUnique && moduleCategory.SpawnLimits && spaceUnavailable) return true;
            }

            return false;
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
        
        /// <summary>
        /// Determines whether the total number of required modules in <see cref="moduleData"/> is outside the range defined by <see cref="minimumModules"/> and <see cref="maximumModules"/>.
        /// </summary>
        /// <remarks>
        /// When a <see cref="ModuleCategory"/> defines <see cref="ModuleCategory.SpawnLimits"/>, its <see cref="ModuleCategory.SpawnMinimum"/> value is used to calculate the total required count.
        /// </remarks>
        /// <returns><c>true</c> if the number of required modules is within the allowed range; otherwise, <c>false</c>.</returns>
        private bool ValidateRequiredModules()
        {
            var minimumRequired = 0;

            foreach (var moduleElement in moduleData)
            {
                var moduleCategory = moduleElement.ModuleCategory;
                if (moduleCategory.SpawnRequired)
                {
                    minimumRequired += moduleCategory.SpawnLimits ? moduleCategory.SpawnMinimum : 1;
                }
            }

            // Additionally check for whether the minimum number of modules is less than the maximum to prevent incorrect error messages.
            if (minimumRequired < minimumModules)
            {
                return minimumRequired < minimumModules || maximumModules < minimumRequired;
            }
            
            return false;
        }
        
        #endif
    }
}