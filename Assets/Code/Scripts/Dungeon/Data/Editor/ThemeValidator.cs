#if UNITY_EDITOR

using Code.Scripts.Attributes.Editor.Required;
using System;
using System.Reflection;
using UnityEditor;

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

                assetTarget.OnValidate();
                
                foreach (var currentInfo in fieldInfo)
                {
                    var infoValue = currentInfo.GetValue(assetTarget);
                    if (infoValue is Array infoArray)
                    {
                        var moduleData = (ModuleData[])infoArray;
                        
                        foreach (var moduleElement in moduleData)
                        {
                            RequiredValidator.EnsureAssignment(assetTarget, moduleElement.ModuleCategory, ref hasErrors);
                            if (hasErrors) break;

                            foreach (var moduleAsset in moduleElement.ModuleAssets)
                            {
                                RequiredValidator.EnsureAssignment(assetTarget, moduleAsset, ref hasErrors);
                                if (hasErrors) break;
                            }
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
    }
}

#endif