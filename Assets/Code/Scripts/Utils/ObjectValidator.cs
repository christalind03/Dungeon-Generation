#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Utils
{
    /// <summary>
    /// Provides utility methods for validating <see cref="Object"/> data and logging configuration errors in the Unity editor.
    /// </summary>
    /// <remarks>
    /// This class is only included in builds when running inside the Unity Editor.
    /// It helps developers catch invalid configurations early by logging descriptive errors and automatically stopping play mode to prevent unintended behavior.
    /// </remarks>
    public static class ObjectValidator
    {
        /// <summary>
        /// Asserts a set of validation checks for a specified Unity object  and logs detailed error messages for any failed assertions.
        /// If any condition fails while the editor is in <c>Play</c> mode, <c>Play</c> mode is terminated to prevent unintended runtime behavior.
        /// </summary>
        /// <param name="unityObject">
        /// The Unity object (e.g., <see cref="MonoBehaviour"/>, <see cref="ScriptableObject"/>) being validated.
        /// </param>
        /// <param name="scriptChecks">
        /// A list of validation conditions and error messages to evaluate.
        /// Each tuple contains:
        /// <list type="bullet">
        /// <item><description><c>invalidCondition</c> - A boolean indicating whether the check fails.</description></item>
        /// <item><description><c>errorMessage</c> - The message to log if the check fails.</description></item>
        /// </list>
        /// </param>
        public static void AssertConditions(Object unityObject, params (bool invalidCondition, string errorMessage)[] scriptChecks)
        {
            var hasErrors = false;

            foreach (var (invalidCondition, errorMessage) in scriptChecks)
            {
                if (!invalidCondition) continue;
                
                Debug.LogError($"[{unityObject.GetType().Name}] {errorMessage}", unityObject);
                hasErrors = true;
            }

            if (EditorApplication.isPlaying && hasErrors)
            {
                EditorApplication.ExitPlaymode();
            }
        }
    }
}

#endif