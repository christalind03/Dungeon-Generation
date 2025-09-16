using Code.Scripts.Attributes;
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
        /// The collection of categories that organize and define spawn rules for modules in this theme.
        /// </summary>
        public ModuleCategory[] ModuleCategories => moduleCategories;
        
        /// <summary>
        /// The collection of all modules that belong to this theme.
        /// </summary>
        public ModuleEntry[] ModuleData => moduleData;
    }
}