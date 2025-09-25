#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
            var attributeInfo = fieldInfo.GetCustomAttribute<Attributes.Required>();
            var propertyContainer = new VisualElement
            {
                style =
                {
                    alignItems = Align.Center,
                    flexDirection = FlexDirection.Row
                }
            };

            var propertyElement = new PropertyField(serializedProperty)
            {
                label = attributeInfo.DisplayLabel ? serializedProperty.displayName : string.Empty,
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
                    visibility = IsAssigned(serializedProperty) ? Visibility.Hidden : Visibility.Visible
                },
                tooltip = "This field is required."
            };
            
            propertyElement.BindProperty(serializedProperty);
            propertyElement.RegisterValueChangeCallback(_ =>
            {
                serializedProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
                
                // Force a repaint of the hierarchy
                EditorApplication.RepaintProjectWindow();
                
                propertyIcon.style.visibility = IsAssigned(serializedProperty) ? Visibility.Hidden : Visibility.Visible; 
            });

            if (attributeInfo.NormalizeLayout)
            {
                propertyElement.schedule.Execute(() =>
                {
                    var fieldLabel = propertyElement.Q<Label>();
                    if (fieldLabel == null) return;
                 
                    fieldLabel.style.flexBasis = Length.Percent(50);
                    
                    var inputField = fieldLabel.parent.Q<VisualElement>(className: "unity-base-field__input");
                    if (inputField == null) return;
                    
                    inputField.style.flexBasis = Length.Percent(50);
                });
            }
            
            propertyContainer.Add(propertyElement);
            propertyContainer.Add(propertyIcon);
            
            return propertyContainer;
        }

        /// <summary>
        /// Determines if the given serialized property is considered to be unassigned.
        /// </summary>
        /// <param name="serializedProperty">The property to validate.</param>
        /// <returns><c>true</c> if the property is <c>null</c> or otherwise unassigned; otherwise <c>false</c> if it has a valid reference.</returns>
        private static bool IsAssigned(SerializedProperty serializedProperty)
        {
            return serializedProperty.propertyType switch
            {
                SerializedPropertyType.Boolean => true,
                SerializedPropertyType.Float => !Mathf.Approximately(serializedProperty.floatValue, float.Epsilon),
                SerializedPropertyType.Integer => serializedProperty.intValue != 0,
                SerializedPropertyType.ObjectReference => serializedProperty.objectReferenceValue,
                SerializedPropertyType.String => !string.IsNullOrEmpty(serializedProperty.stringValue),
                _ => false
            };
        }
    }
}

#endif