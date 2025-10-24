#if UNITY_EDITOR

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Attributes.Required
{
    /// <summary>
    /// Validates all <see cref="RequiredAttribute"/> fields before entering Play Mode.
    /// </summary>
    /// <remarks>
    /// Based on an implementation by <see href="https://www.youtube.com/watch?v=cVxjKphoi5o">Freedom Coding on YouTube</see>.
    /// </remarks>
    [InitializeOnLoad]
    public static class RequiredValidator
    {
        /// <summary>
        /// Registers the validation callback for Play Mode state changes.
        /// </summary>
        static RequiredValidator()
        {
            EditorApplication.playModeStateChanged += playModeState =>
            {
                if (playModeState != PlayModeStateChange.ExitingEditMode) return;
                
                CheckRequirements();
            };
        }

        /// <summary>
        /// Scans all <see cref="RequiredAttribute"/> instances in the scene and logs errors for any unassigned <see cref="RequiredAttribute"/> fields.
        /// </summary>
        private static void CheckRequirements()
        {
            var hasErrors = false;
            var monoBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (var monoBehaviour in monoBehaviours)
            {
                EnsureAssignment(monoBehaviour, monoBehaviour, ref hasErrors);
            }

            if (!hasErrors) return;
            
            EditorApplication.ExitPlaymode();
        }

        /// <summary>
        /// Ensures that all fields marked with <see cref="RequiredAttribute"/> in the given target object are properly assigned.
        /// If any required fields are unassigned, it logs an error in the Unity console and updates <see cref="hasErrors"/> to indicate validation failure.
        /// </summary>
        /// <param name="unityObject">
        /// The <see cref="UnityEngine.Object"/> that owns the <paramref name="targetObject"/>.
        /// Used for context in error messages.
        /// </param>
        /// <param name="targetObject">
        /// The object instance whose fields are inspected for the <see cref="RequiredAttribute"/> attribute.
        /// </param>
        /// <param name="hasErrors">
        /// A reference to a flag indicating whether validation errors have occurred.
        /// This will be set to <c>true</c> if any unassigned required fields are detected.
        /// </param>
        public static void EnsureAssignment(UnityEngine.Object unityObject, object targetObject, ref bool hasErrors)
        {
            var fieldInfo = targetObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var currentInfo in fieldInfo)
            {
                var requiredAttribute = currentInfo.GetCustomAttribute<Attributes.Required.RequiredAttribute>();
                
                if (requiredAttribute == null) continue;
                if (!string.IsNullOrEmpty(requiredAttribute.RequireIf))
                {
                    var isInverted = requiredAttribute.RequireIf[0] == '!';
                    var dependencyInfo = targetObject
                        .GetType()
                        .GetField(
                            isInverted ? requiredAttribute.RequireIf[1..] : requiredAttribute.RequireIf,
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                        );
                    
                    if (dependencyInfo == null) continue;
                    
                    var shouldValidate = true;                    
                    var dependencyValue = dependencyInfo.GetValue(targetObject);

                    if (dependencyValue is bool booleanValue)
                    {
                        shouldValidate = isInverted ^ booleanValue;
                    }
                    
                    if (!shouldValidate) continue;
                }
                
                var infoValue = currentInfo.GetValue(targetObject);
                if (IsAssigned(infoValue)) continue;
                
                Debug.LogError($"[{unityObject.GetType().Name}] <b>{currentInfo.Name}</b> on <b>{unityObject.name}</b> is not assigned.", unityObject);
                hasErrors = true;
            }
        }
        
        /// <summary>
        /// Determines whether a given object is considered "assigned."
        /// </summary>
        /// <param name="targetObject">The object to evaluate.</param>
        /// <returns><c>true</c> if the object has a meaningful, non-default value; otherwise <c>false</c>.</returns>
        private static bool IsAssigned(object targetObject)
        {
            return targetObject switch
            {
                null => false,
                UnityEngine.Object unityObject => !unityObject.Equals(null),
                
                // Primitive Types
                bool => true,
                int castedValue => castedValue != 0,
                float castedValue => !Mathf.Approximately(castedValue, 0f),
                double castedValue => castedValue != 0,
                string castedValue => !string.IsNullOrEmpty(castedValue),
                char castedValue => castedValue != '\0',
                
                // Unity Types
                Color castedValue => castedValue != default,
                Vector2 castedValue => castedValue != default,
                Vector3 castedValue => castedValue != default,
                Vector4 castedValue => castedValue != default,
                Bounds castedValue => castedValue != default,
                Rect castedValue => castedValue != default,
                Quaternion castedValue => castedValue != default,
                Vector2Int castedValue => castedValue != default,
                Vector3Int castedValue => castedValue != default,
                BoundsInt castedValue => castedValue != default,
                RectInt castedValue => castedValue != default,
                Hash128 castedValue => castedValue != default,
                
                // Unity Utility Types
                Gradient => true,
                AnimationCurve => true,
                ExposedReference<UnityEngine.Object> castedValue => castedValue is { },
                LayerMask castedValue => castedValue != 0,

                // Enumerable Types
                IEnumerable castedValue => castedValue.Cast<object>().Any(),

                // Fallback for managed types or structs
                _ => !targetObject.GetType().IsValueType || !targetObject.Equals(Activator.CreateInstance(targetObject.GetType()))
            };
        }
    }
}

#endif