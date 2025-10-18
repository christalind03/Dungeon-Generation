using UnityEditor;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Data.Editor
{
    /// <summary>
    /// Provides a custom property drawer for the <see cref="ModuleAsset"/> struct.
    /// </summary>
    [CustomPropertyDrawer(typeof(ModuleAsset))]
    public class ModuleAssetPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Creates the custom property inspector user interface for <see cref="ModuleAsset"/>.
        /// </summary>
        /// <param name="serializedProperty">
        /// The <see cref="SerializedProperty"/> representing the <see cref="ModuleAsset"/> instance to be displayed and edited.
        /// </param>
        /// <returns>A <see cref="VisualElement"/> container holding all generated fields for this property.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            return base.CreatePropertyGUI(serializedProperty);
        }
    }
}