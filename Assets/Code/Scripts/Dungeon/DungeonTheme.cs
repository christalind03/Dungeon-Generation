using UnityEngine;

namespace Code.Scripts.Dungeon
{
    /// <summary>
    /// Defines the dataset used for procedural dungeon generation.
    /// This includes the theme's name, available module categories, and the individual models that can be spawned.
    /// </summary>
    [CreateAssetMenu(fileName = "Dungeon_Theme", menuName = "Dungeon Theme")]
    public class DungeonTheme : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The display name of the dungeon theme")]
        private string themeName;
        
        [SerializeField]
        [Tooltip("The collection of categories that organize and define spawn rules for modules in this theme")]
        private DungeonModuleCategory[] moduleCategories;
        
        [SerializeField]
        [Tooltip("The collection of all modules that belong to this theme")]
        private DungeonModule[] moduleData;
        
        /// <summary>
        /// The display name of the dungeon theme.
        /// </summary>
        public string ThemeName => themeName;
        
        /// <summary>
        /// The collection of categories that organize and define spawn rules for modules in this theme.
        /// </summary>
        public DungeonModuleCategory[] ModuleCategories => moduleCategories;
        
        /// <summary>
        /// The collection of all modules that belong to this theme.
        /// </summary>
        public DungeonModule[] ModuleData => moduleData;
    }
}