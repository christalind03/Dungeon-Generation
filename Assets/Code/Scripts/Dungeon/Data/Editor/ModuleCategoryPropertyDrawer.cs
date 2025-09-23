#if UNITY_EDITOR

using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Data.Editor
{
    /// <summary>
    /// Custom property drawer for the <see cref="ModuleCategory"/> struct.
    /// </summary>
    [CustomPropertyDrawer(typeof(ModuleCategory))]
    public class ModuleCategoryPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Creates the custom property inspector user interface for <see cref="ModuleCategory"/>.
        /// </summary>
        /// <param name="serializedProperty">
        /// The <see cref="SerializedProperty"/> representing the <see cref="ModuleCategory"/> instance to be displayed and edited.
        /// </param>
        /// <returns>A <see cref="VisualElement"/> container holding all generated fields for this property.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            VisualElement containerElement;
            
            var categoryTitle = serializedProperty.FindPropertyRelative("categoryTitle");
            var categoryTitlePropertyField = new PropertyField(categoryTitle);
            
            var isArrayElement = serializedProperty.propertyPath.Contains("Array");
            if (isArrayElement)
            {
                var arrayIndex = ParseIndex(serializedProperty);
                containerElement = new Foldout
                {
                    text = $"Element {arrayIndex}"
                };
                
                categoryTitlePropertyField.RegisterValueChangeCallback(changeEvent =>
                {
                    var changedTitle = changeEvent.changedProperty.stringValue;
                    (containerElement as Foldout).text = string.IsNullOrEmpty(changedTitle)? $"Element {ParseIndex(serializedProperty)}" : changedTitle;
                });
            }
            else
            {
                containerElement = new VisualElement();
            }
            
            var spawnRangePropertyField = new VisualElement();
            
            var spawnLimitsPropertyField = CreatePropertyField(serializedProperty, "spawnLimits");
            var spawnMaximumPropertyField = CreatePropertyField(serializedProperty, "spawnMaximum");
            var spawnMinimumPropertyField = CreatePropertyField(serializedProperty, "spawnMinimum");

            spawnMaximumPropertyField.style.marginLeft = 15;
            spawnMinimumPropertyField.style.marginLeft = 15;
            
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

            spawnRatePropertyField.style.marginLeft = 15;
            
            spawnBehaviorPropertyField.Add(spawnRequiredPropertyField);
            spawnBehaviorPropertyField.Add(spawnRatePropertyField);
            
            spawnRequiredPropertyField.RegisterValueChangeCallback(changeEvent =>
            {
                spawnRatePropertyField.style.display = changeEvent.changedProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            });
            
            containerElement.Add(categoryTitlePropertyField);
            containerElement.Add(spawnRangePropertyField);
            containerElement.Add(spawnBehaviorPropertyField);
            
            return containerElement;
        }

        /// <summary>
        /// Extracts the array index from the given property's path string.
        /// </summary>
        /// <param name="serializedProperty">The <see cref="SerializedProperty"/> whose path should be parsed for an array index.</param>
        /// <returns>A zero-based index of the array element if parsing succeeds; otherwise <c>-1</c>.</returns>
        private static int ParseIndex(SerializedProperty serializedProperty)
        {
            var regexMatch = Regex.Match(serializedProperty.propertyPath, @"\[(\d+)\]");
            return regexMatch.Success ? int.Parse(regexMatch.Groups[1].Value) : -1;
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