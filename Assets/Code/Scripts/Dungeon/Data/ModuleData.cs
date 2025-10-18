using System;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data
{
    /// <summary>
    /// Represents a data entry for a single module configuration within a <see cref="DungeonTheme"/>.
    /// Each entry associates a <see cref="ModuleCategory"/> with a collection of <see cref="ModuleAssets"/> instances.
    /// </summary>
    [Serializable]
    public struct ModuleData
    {
        [SerializeField] private ModuleAsset[] moduleAssets;
        [SerializeField] private ModuleCategory moduleCategory;
     
        /// <summary>
        /// The list of <see cref="ModuleAsset"/> objects that belong to this module data entry.
        /// These define individual prefab assets available for spawning.
        /// </summary>
        public ModuleAsset[] ModuleAssets => moduleAssets;
        
        /// <summary>
        /// The category that this set of module assets belongs to.
        /// Determines how frequently and under what conditions these assets are chosen during generation.
        /// </summary>
        public ModuleCategory ModuleCategory => moduleCategory;
    }
}