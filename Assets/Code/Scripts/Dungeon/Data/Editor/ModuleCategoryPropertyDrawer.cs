#if UNITY_EDITOR

using Code.Scripts.Dungeon.Data.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Data.Editor
{
    /// <summary>
    /// Provides a custom property drawer for the <see cref="ModuleCategory"/> struct.
    /// </summary>
    [CustomPropertyDrawer(typeof(ModuleCategory))]
    internal class ModuleCategoryPropertyDrawer : PropertyDrawer
    {
        private const int SubregionMargin = 15;
        
        /// <summary>
        /// Creates the custom property inspector user interface for <see cref="ModuleCategory"/>.
        /// </summary>
        /// <param name="serializedProperty">
        /// The <see cref="SerializedProperty"/> representing the <see cref="ModuleCategory"/> instance to be displayed and edited.
        /// </param>
        /// <returns>A <see cref="VisualElement"/> container holding all generated fields for this property.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var containerElement = new VisualElement();

            var categoryTitlePropertyField = CreatePropertyField(serializedProperty, "categoryTitle");
            
            var spawnRangePropertyField = new VisualElement();
            
            var spawnLimitsPropertyField = CreatePropertyField(serializedProperty, "spawnLimits");
            var spawnMaximumPropertyField = CreatePropertyField(serializedProperty, "spawnMaximum");
            var spawnMinimumPropertyField = CreatePropertyField(serializedProperty, "spawnMinimum");

            spawnMaximumPropertyField.style.marginLeft = SubregionMargin;
            spawnMinimumPropertyField.style.marginLeft = SubregionMargin;
            
            spawnRangePropertyField.Add(spawnLimitsPropertyField);
            spawnRangePropertyField.Add(spawnMaximumPropertyField);
            spawnRangePropertyField.Add(spawnMinimumPropertyField);
            
            spawnLimitsPropertyField.RegisterValueChangeCallback(changeEvent =>
            {
                var displayStyle = changeEvent.changedProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                
                spawnMaximumPropertyField.style.display = displayStyle;
                spawnMinimumPropertyField.style.display = displayStyle;
            });
            
            var spawnBehaviorPropertyField = new VisualElement();
            
            var spawnRequiredPropertyField = CreatePropertyField(serializedProperty, "spawnRequired");
            var spawnRatePropertyField = CreatePropertyField(serializedProperty, "spawnRate");

            spawnRatePropertyField.style.marginLeft = SubregionMargin;
            
            spawnBehaviorPropertyField.Add(spawnRequiredPropertyField);
            spawnBehaviorPropertyField.Add(spawnRatePropertyField);
            
            spawnRequiredPropertyField.RegisterValueChangeCallback(changeEvent =>
            {
                var spawnRequired = changeEvent.changedProperty.boolValue;
                if (spawnRequired)
                {
                    var spawnRateProperty = serializedProperty.FindPropertyRelative("spawnRate");
                    if (spawnRateProperty != null)
                    {
                        spawnRateProperty.floatValue = 0f;
                        spawnRateProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
                
                spawnRatePropertyField.style.display = changeEvent.changedProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            containerElement.Add(categoryTitlePropertyField);
            containerElement.Add(spawnRangePropertyField);
            containerElement.Add(spawnBehaviorPropertyField);
            
            return containerElement;
        }
        
        /// <summary>
        /// Creates and binds a <see cref="PropertyField"/> for a relative property.
        /// </summary>
        /// <param name="serializedProperty">The parent <see cref="SerializedProperty"/> containing the target property.</param>
        /// <param name="targetProperty">The name of the relative property to locate and display.</param>
        /// <returns>A <see cref="PropertyField"/> bound to the specified relative property.</returns>
        private static PropertyField CreatePropertyField(SerializedProperty serializedProperty, string targetProperty)
        {
            var propertyRelative = serializedProperty.FindPropertyRelative(targetProperty);
            var propertyField = new PropertyField(propertyRelative);

            propertyField.BindProperty(propertyRelative);

            return propertyField;
        }
    }
}

#endif