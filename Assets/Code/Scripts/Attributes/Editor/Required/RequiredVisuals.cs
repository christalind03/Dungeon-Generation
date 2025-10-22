#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Code.Scripts.Attributes.Editor.Required
{
    /// <summary>
    /// Provides shared visual settings and assets for <see cref="Required"/> validation UI.
    /// </summary>
    public static class RequiredVisuals
    {
        public const float IconSize = 15;
        public static readonly Texture RequiredIcon = EditorGUIUtility.FindTexture("console.erroricon");
    }
}

#endif