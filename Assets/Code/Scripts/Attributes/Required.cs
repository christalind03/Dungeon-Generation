using System;
using UnityEngine;

namespace Code.Scripts.Attributes
{
    /// <summary>
    /// Attribute used to indicate that a field is required and must be assigned a value in the Unity Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class Required : PropertyAttribute
    {
        /// <summary>
        /// Determines whether the field label should be displayed in the inspector when using the custom drawer.
        /// </summary>
        public readonly bool DisplayLabel;

        /// <summary>
        /// Specifies the name of another field that this field is conditionally required by.
        /// </summary>
        public readonly string RequireIf;

        /// <summary>
        /// Initializes a new instance of the <see cref="Required"/> attribute.
        /// </summary>
        /// <param name="displayLabel">
        /// If <c>true</c>, the field's label will be displayed in the inspector.
        /// If <c>false</c>, the field's label will still be validated, but its label will be hidden.
        /// </param>
        /// <param name="requireIf">
        /// The name of another field that this field is conditionally required by.
        /// If specified, this field will only be considered required when the referenced field meets its condition (non-null or true).
        /// Defaults to an empty string, meaning the field is always required.
        /// </param>
        public Required(bool displayLabel = true, string requireIf = "")
        {
            DisplayLabel = displayLabel;
            RequireIf = requireIf;
        }
    }
}