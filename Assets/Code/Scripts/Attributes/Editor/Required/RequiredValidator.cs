#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Attributes.Editor.Required
{
    /// <summary>
    /// Validates all <see cref="Required"/> fields before entering Play Mode.
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
                if (playModeState == PlayModeStateChange.ExitingEditMode)
                {
                    DisplayErrors();
                }
            };
        }

        /// <summary>
        /// Scans all <see cref="Required"/> instances in the scene and logs errors for any unassigned <see cref="Required"/> fields.
        /// </summary>
        private static void DisplayErrors()
        {
            var hasErrors = false;
            var monoBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (var monoBehaviour in monoBehaviours)
            {
                var fieldInfos = monoBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                foreach (var fieldInfo in fieldInfos)
                {
                    var requiredAttribute = fieldInfo.GetCustomAttribute<Attributes.Required>();
                    if (requiredAttribute == null) continue;
                    
                    var fieldValue = fieldInfo.GetValue(monoBehaviour);
                    if (fieldValue != null) continue;
                    
                    Debug.LogError($"[{monoBehaviour.GetType().Name}] <b>{fieldInfo.Name}</b> on <b>{monoBehaviour.name}</b> is not assigned.", monoBehaviour);
                    hasErrors = true;
                }
            }

            if (hasErrors)
            {
                EditorApplication.ExitPlaymode();
            }
        }
    }
}

#endif