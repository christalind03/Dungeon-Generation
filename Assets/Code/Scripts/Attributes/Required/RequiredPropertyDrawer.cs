#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Attributes.Required
{
    /// <summary>
    /// A custom property drawer for the <see cref="RequiredAttribute"/> attribute.
    /// </summary>
    /// <remarks>
    /// Based on an implementation by <see href="https://www.youtube.com/watch?v=OidPCs1YECo">git-amend on YouTube</see>.
    /// </remarks>
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    internal class RequiredPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Renders the property field with additional validation visuals if unassigned.
        /// </summary>
        /// <param name="serializedProperty">The property being drawn.</param>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var attributeInfo = fieldInfo.GetCustomAttribute<RequiredAttribute>();
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
                SerializedPropertyType.Generic => serializedProperty.Copy().Next(true),
                SerializedPropertyType.Integer => serializedProperty.intValue != 0,
                SerializedPropertyType.Boolean => true,
                SerializedPropertyType.Float => !Mathf.Approximately(serializedProperty.floatValue, 0f),
                SerializedPropertyType.String => !string.IsNullOrEmpty(serializedProperty.stringValue),
                SerializedPropertyType.Color => serializedProperty.colorValue != default,
                SerializedPropertyType.ObjectReference => serializedProperty.objectReferenceValue is not null,
                SerializedPropertyType.LayerMask => serializedProperty.intValue != 0,
                SerializedPropertyType.Enum => serializedProperty.enumValueIndex != 0,
                SerializedPropertyType.Vector2 => serializedProperty.vector2Value != default,
                SerializedPropertyType.Vector3 => serializedProperty.vector3Value != default,
                SerializedPropertyType.Vector4 =>  serializedProperty.vector4Value != default,
                SerializedPropertyType.Rect => serializedProperty.rectValue != default,
                SerializedPropertyType.ArraySize => serializedProperty.arraySize != 0,
                SerializedPropertyType.Character => serializedProperty.intValue != 0,
                SerializedPropertyType.AnimationCurve => serializedProperty.animationCurveValue != null,
                SerializedPropertyType.Bounds => serializedProperty.boundsValue != default,
                SerializedPropertyType.Gradient => serializedProperty.gradientValue != null,
                SerializedPropertyType.Quaternion => serializedProperty.quaternionValue != default,
                SerializedPropertyType.ExposedReference => serializedProperty.exposedReferenceValue is not null,
                SerializedPropertyType.FixedBufferSize => serializedProperty.fixedBufferSize != 0,
                SerializedPropertyType.Vector2Int => serializedProperty.vector2IntValue != default,
                SerializedPropertyType.Vector3Int => serializedProperty.vector3IntValue != default,
                SerializedPropertyType.RectInt => serializedProperty.rectIntValue != default,
                SerializedPropertyType.BoundsInt => serializedProperty.boundsIntValue != default,
                SerializedPropertyType.ManagedReference => serializedProperty.managedReferenceValue is not null,
                SerializedPropertyType.Hash128 => serializedProperty.hash128Value != default,
                SerializedPropertyType.RenderingLayerMask => serializedProperty.intValue != 0,
                _ => false
            };
        }
    }
}

#endif