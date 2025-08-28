using UnityEngine;

namespace Code.Scripts.Dungeon
{
    [CreateAssetMenu(fileName = "Dungeon_Theme", menuName = "Dungeon Theme")]
    public class DungeonTheme : ScriptableObject
    {
        [SerializeField] private string themeName;
        [SerializeField] private DungeonModuleCategory[] moduleCategories;
        [SerializeField] private DungeonModule[] moduleData;
        
        public string ThemeName => themeName;
        public DungeonModuleCategory[] ModuleCategories => moduleCategories;
        public DungeonModule[] ModuleData => moduleData;
    }
}