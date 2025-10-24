#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Attributes.Required
{
    /// <summary>
    /// Provides shared visual settings and assets for <see cref="RequiredAttribute"/> validation UI.
    /// </summary>
    internal static class RequiredVisuals
    {
        public const float IconSize = 15;
        public static readonly Texture RequiredIcon = EditorGUIUtility.FindTexture("console.erroricon");
    }
}

#endif