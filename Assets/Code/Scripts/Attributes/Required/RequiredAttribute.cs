using System;
using UnityEngine;

namespace Code.Scripts.Attributes.Required
{
    /// <summary>
    /// Attribute used to indicate that a field is required and must be assigned a value in the Unity Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : PropertyAttribute
    {
        /// <summary>
        /// Determines whether the field label should be displayed in the inspector when using the custom drawer.
        /// </summary>
        public readonly bool DisplayLabel;

        public readonly bool NormalizeLayout;
        
        /// <summary>
        /// Specifies the name of another field that this field is conditionally required by.
        /// </summary>
        public readonly string RequireIf;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredAttribute"/> attribute.
        /// </summary>
        /// <param name="displayLabel">
        /// If <c>true</c>, the field's label will be displayed in the inspector.
        /// If <c>false</c>, the field's label will still be validated, but its label will be hidden.
        /// </param>
        /// <param name="normalizeLayout">
        /// If <c>true</c>, adjusts the field's layout to align consistently with surrounding fields in the inspector.
        /// </param>
        /// <param name="requireIf">
        /// The name of another field that this field is conditionally required by.
        /// When set, this field is required only if the referenced field is non-null/true (invert with <c>!</c>).
        /// Defaults to an empty string, meaning the field is always required.
        /// </param>
        public RequiredAttribute(bool displayLabel = true, bool normalizeLayout = false, string requireIf = "")
        {
            DisplayLabel = displayLabel;
            NormalizeLayout = normalizeLayout;
            RequireIf = requireIf;
        }
    }
}