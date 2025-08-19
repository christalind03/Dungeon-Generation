#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

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
        /// <param name="guiPosition">The rectangle area allocated for the property.</param>
        /// <param name="serializedProperty">The property being drawn.</param>
        /// <param name="guiContent">The label content for the property.</param>
        public override void OnGUI(Rect guiPosition, SerializedProperty serializedProperty, GUIContent guiContent)
        {
            EditorGUI.BeginProperty(guiPosition, guiContent, serializedProperty);
            EditorGUI.BeginChangeCheck();

            Rect fieldRect = new(guiPosition.x, guiPosition.y, guiPosition.width - RequiredVisuals.IconSize, guiPosition.height);
            
            EditorGUI.PropertyField(fieldRect, serializedProperty, guiContent);
            
            // If the field is required but unassigned, display an error icon 
            if (IsUnassigned(serializedProperty))
            {
                Rect iconRect = new(guiPosition.xMax - RequiredVisuals.IconSize * 0.85f, guiPosition.y, RequiredVisuals.IconSize, RequiredVisuals.IconSize);
                
                GUI.Label(iconRect, new GUIContent(RequiredVisuals.RequiredIcon, "This field is required."));
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedProperty.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
                
                // Force a repaint of the hierarchy
                EditorApplication.RepaintProjectWindow();
            }
            
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Determines if the given serialized property is considered to be unassigned.
        /// </summary>
        /// <param name="serializedProperty">The property to validate.</param>
        /// <returns><c>true</c> if the property is <c>null</c> or otherwise unassigned; otherwise <c>false</c> if it has a valid reference.</returns>
        private bool IsUnassigned(SerializedProperty serializedProperty)
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