using Code.Scripts.Attributes;
using System;
using System.Linq;
using Code.Scripts.Utils;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// Defines the dataset used for procedural dungeon generation.
    /// This includes the theme's name, available module categories, and the individual models that can be spawned.
    /// </summary>
    [CreateAssetMenu(fileName = "Dungeon_Theme", menuName = "Dungeon Theme")]
    public class Theme : ScriptableObject
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
        [Tooltip("The collection of categories that organize and define spawn rules for modules in this theme")]
        private ModuleCategory[] moduleCategories;
        
        [SerializeField]
        [Tooltip("The collection of all modules that belong to this theme")]
        private ModuleEntry[] moduleData;
        
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
        
        /// <summary>
        /// The collection of categories that organize and define spawn rules for modules in this theme.
        /// </summary>
        public ModuleCategory[] ModuleCategories => moduleCategories;
        
        /// <summary>
        /// The collection of all modules that belong to this theme.
        /// </summary>
        public ModuleEntry[] ModuleData => moduleData;

        #if UNITY_EDITOR
        
        /// <summary>
        /// Validates this theme's configuration and logs errors to help catch setup issues early.
        /// </summary>
        private void OnValidate()
        {
            ScriptValidator.LogError(
                this,
                (maximumModules < minimumModules, $"<b>{nameof(maximumModules)}</b> must be greater than <b>{nameof(minimumModules)}</b>"),
                (moduleCategories.Length <= 0, $"<b>{nameof(moduleCategories)}</b> must contain at least one element."),
                (moduleData.Length <= 0, $"<b>{nameof(moduleData)}</b> must contain at least one element."),
                (ContainsInvalidBounds(), $"All <b>{nameof(moduleCategories)}</b> entries contain spawn limits that exceed this theme's bounds."),
                (ContainsUnassignedCategories(), $"All <b>{nameof(moduleCategories)}</b> must be assigned to at least one <b>{nameof(moduleData)}</b> entry.")
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
            var hasErrors = true;

            foreach (var moduleCategory in moduleCategories)
            {
                if (moduleCategory.SpawnLimits)
                {
                    if (maximumModules < moduleCategory.SpawnMinimum) continue;
                }
                
                hasErrors = false;
                break;
            }
            
            return hasErrors;
        }

        /// <summary>
        /// Determines whether there are module categories that are not references by any <see cref="moduleData"/> entries.
        /// </summary>
        /// <returns>
        /// <c>true</c> if one or more categories are unassigned; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsUnassignedCategories()
        {
            var unassignedCategories = moduleCategories.Select(moduleCategory => moduleCategory.CategoryID).ToList();

            foreach (var moduleEntry in moduleData)
            {
                if (unassignedCategories.Contains(moduleEntry.ModuleCategory))
                {
                    unassignedCategories.Remove(moduleEntry.ModuleCategory);
                }
            }
            
            return 0 < unassignedCategories.Count;
        }
        
        #endif
    }
}