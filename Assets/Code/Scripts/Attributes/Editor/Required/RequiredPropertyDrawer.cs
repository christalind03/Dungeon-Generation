#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Code.Scripts.Attributes.Editor.Required
{
    /// <summary>
    /// Custom property drawer for the <see cref="Required"/> attribute.
    /// </summary>
    /// <remarks>
    /// Based on an implementation by <see href="https://www.youtube.com/watch?v=OidPCs1YECo">git-amend on YouTube</see>.
    /// </remarks>
    [CustomPropertyDrawer(typeof(Attributes.Required))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Renders the property field with additional validation visuals if unassigned.
        /// </summary>
        /// <param name="serializedProperty">The property being drawn.</param>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var propertyContainer = new VisualElement
            {
                style =
                {
                    alignItems = Align.Center,
                    flexDirection = FlexDirection.Row
                }
            };

            var propertyElement = new PropertyField(serializedProperty, serializedProperty.displayName)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            
            var propertyIcon = new Image
            {
                image = RequiredVisuals.RequiredIcon,
                style =
                {
                    marginLeft = 5,
                    height = RequiredVisuals.IconSize,
                    width = RequiredVisuals.IconSize,
                    visibility = IsUnassigned(serializedProperty) ? Visibility.Visible : Visibility.Hidden
                },
                tooltip = "This field is required."
            };
            
            propertyElement.RegisterValueChangeCallback(_ =>
            {
                serializedProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
                
                // Force a repaint of the hierarchy
                EditorApplication.RepaintProjectWindow();
                
                propertyIcon.style.visibility = IsUnassigned(serializedProperty) ? Visibility.Visible : Visibility.Hidden; 
            });
            
            propertyContainer.Add(propertyElement);
            propertyContainer.Add(propertyIcon);
            
            return propertyContainer;
        }

        /// <summary>
        /// Determines if the given serialized property is considered to be unassigned.
        /// </summary>
        /// <param name="serializedProperty">The property to validate.</param>
        /// <returns><c>true</c> if the property is <c>null</c> or otherwise unassigned; otherwise <c>false</c> if it has a valid reference.</returns>
        private static bool IsUnassigned(SerializedProperty serializedProperty)
        {
            return serializedProperty.propertyType switch
            {
                SerializedPropertyType.ObjectReference when serializedProperty.objectReferenceValue => false,
                _ => true
            };
        }
    }
}

#endif