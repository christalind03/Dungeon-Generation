#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Attributes.Editor.Required
{
    /// <summary>
    /// Draws visual indicators in the Unity hierarchy when a <see cref="Required"/> field is unassigned.
    /// </summary>
    /// <remarks>
    /// Based on an implementation by <see href="https://www.youtube.com/watch?v=OidPCs1YECo">git-amend on YouTube</see>.
    /// </remarks>
    [InitializeOnLoad]
    public static class RequiredHierarchyDrawer
    {
        private static readonly Dictionary<Type, FieldInfo[]> CachedFieldInfo = new();
        
        /// <summary>
        /// Automatically registers the hierarchy GUI callback.
        /// </summary>
        static RequiredHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        /// <summary>
        /// Called for each item in the Unity hierarchy to determine if an error icon should be drawn for missing required fields.
        /// </summary>
        /// <param name="instanceID">The instance ID of the hierarchy object.</param>
        /// <param name="selectionRect">The rectangle area of the hierarchy row.</param>
        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is not GameObject gameObject) return;

            foreach (var objectComponent in gameObject.GetComponents<Component>())
            {
                if (objectComponent is null) continue;
                
                var requiredFields = GetRequiredFields(objectComponent.GetType());
                if (requiredFields == null) continue;

                if (requiredFields.Any(targetField => IsUnassigned(targetField.GetValue(objectComponent))))
                {
                    var iconRect = new Rect(selectionRect.xMax - RequiredVisuals.IconSize * 0.85f, selectionRect.y, RequiredVisuals.IconSize, RequiredVisuals.IconSize);
                    
                    GUI.Label(iconRect, new GUIContent(RequiredVisuals.RequiredIcon, "One or more required fields are unassigned."));
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieves the cached or newly discovered required fields for a given component type.
        /// </summary>
        /// <param name="componentType">The type of the component being inspected.</param>
        /// <returns>
        /// An array of <see cref="FieldInfo"/> objects for fields that are public or serialized and marked with <see cref="Required"/>.
        /// </returns>
        private static FieldInfo[] GetRequiredFields(Type componentType)
        {
            if (!CachedFieldInfo.TryGetValue(componentType, out var fieldInfos))
            {
                List<FieldInfo> requiredFields = new();
                fieldInfos = componentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (var fieldInfo in fieldInfos)
                {
                    var isSerialized = fieldInfo.IsPublic || fieldInfo.IsDefined(typeof(SerializeField), false);
                    var isRequired = fieldInfo.IsDefined(typeof(Attributes.Required), false);

                    if (isSerialized && isRequired)
                    {
                        requiredFields.Add(fieldInfo);
                    }
                }
                
                fieldInfos = requiredFields.ToArray();
                CachedFieldInfo[componentType] = fieldInfos;
            }
            
            return fieldInfos;
        }

        /// <summary>
        /// Determines if the given object is considered to be unassigned.
        /// </summary>
        /// <param name="targetObject">The object to evaluate.</param>
        /// <returns><c>true</c> is the object is null or contains null entries (if enumerable); otherwise <c>false</c> if it has a valid reference.</returns>
        private static bool IsUnassigned(object targetObject)
        {
            switch (targetObject)
            {
                case null:
                    return true;
               
                case IEnumerable enumerableType:
                {
                    if (enumerableType.Cast<object>().Any(obj => obj == null)) return true;
                    break;
                }
            }

            return false;
        }
    }
}

#endif