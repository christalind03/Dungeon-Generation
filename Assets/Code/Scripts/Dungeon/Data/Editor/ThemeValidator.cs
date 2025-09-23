#if UNITY_EDITOR

using Code.Scripts.Attributes.Editor.Required;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Dungeon.Data.Editor
{
    /// <summary>
    /// Provides automated validation for <see cref="Theme"/> assets when entering Play Mode in the Unity Editor.
    /// </summary>
    [InitializeOnLoad]
    public static class ThemeValidator
    {
        /// <summary>
        /// Registers the validation check to run when Play Mode is entered.
        /// </summary>
        static ThemeValidator()
        {
            EditorApplication.playModeStateChanged += playModeState =>
            {
                if (playModeState != PlayModeStateChange.ExitingEditMode) return;
                
                CheckRequirements();
            };
        }
        
        /// <summary>
        /// Iterates through all <see cref="Theme"/> assets in the project and validates their serialized fields for assignment and weight distribution.
        /// </summary>
        /// <remarks>If any validation errors are detected, Play Mode is immediately terminated.</remarks>
        private static void CheckRequirements()
        {
            var hasErrors = false;
            var themeGUIDs = AssetDatabase.FindAssets("t:Theme");

            foreach (var themeGUID in themeGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(themeGUID);
                var assetTarget = AssetDatabase.LoadAssetAtPath<Theme>(assetPath);
                var fieldInfo = assetTarget.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (var currentInfo in fieldInfo)
                {
                    var infoValue = currentInfo.GetValue(assetTarget);

                    if (infoValue is Array infoArray)
                    {
                        // Temporarily disable validating weights until the user interface for module data has been updated
                        // EnforceWeights(assetTarget, currentInfo, infoArray, ref hasErrors);
                        
                        foreach (var arrayElement in infoArray)
                        {
                            RequiredValidator.EnsureAssignment(assetTarget, arrayElement, ref hasErrors);
                        }
                    }
                    else
                    {
                        RequiredValidator.EnsureAssignment(assetTarget, infoValue, ref hasErrors);
                    }
                }
            }

            if (!hasErrors) return;
            
            EditorApplication.ExitPlaymode();
        }

        /// <summary>
        /// Validates that the cumulative <c>spawnRate</c> for a given array of elements roughly sum to <c>1.0</c>.
        /// </summary>
        /// <param name="unityObject">The <see cref="UnityEngine.Object"/> being validated, used for context in error logging.</param>
        /// <param name="infoValue">The <see cref="FieldInfo"/> representing teh array field being validated.</param>
        /// <param name="targetArray">The array whose elements contain <c>spawnRate</c> fields to sum.</param>
        /// <param name="hasErrors">A reference to the error flag that will be set to <c>true</c> if the cumulative weight is invalid.</param>
        /// <remarks>
        /// If the cumulative weight does not fall within the [0.99, 1.0] range, an error message is logged to the console and Play Mode is prevented from starting.
        /// </remarks>
        private static void EnforceWeights(UnityEngine.Object unityObject, FieldInfo infoValue, Array targetArray, ref bool hasErrors)
        {
            if (0 < targetArray.Length)
            {
                var cumulativeWeight = 0f;
                
                foreach (var arrayElement in targetArray)
                {
                    var fieldInfos = arrayElement.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                
                    foreach (var fieldInfo in fieldInfos)
                    {
                        if (fieldInfo.Name != "spawnRate") continue;
                
                        cumulativeWeight += (float)fieldInfo.GetValue(arrayElement);
                    }
                }
                
                if (cumulativeWeight is >= 0.99f and <= 1f) return;
            }
            
            hasErrors = true;
            Debug.LogError($"<b>[{unityObject.GetType().Name}]</b> <b>{infoValue.Name}</b> cumulative spawn rate on <b>{unityObject.name}</b> should roughly equal 1.", unityObject);
        }
    }
}

#endif