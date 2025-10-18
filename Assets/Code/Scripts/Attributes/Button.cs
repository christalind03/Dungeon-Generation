using System;
using UnityEngine;

namespace Code.Scripts.Attributes
{
    /// <summary>
    /// Attribute used to display a button in the Unity Inspector.
    /// When clicked, the button invokes a specified method on the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class Button : PropertyAttribute
    {
        /// <summary>
        /// The label displayed on the button in the inspector.
        /// </summary>
        public readonly string Label;
        
        /// <summary>
        /// The name of the method to invoke when the button is clicked.
        /// </summary>
        public readonly string CallbackFn;
        
        /// <summary>
        /// Optional parameters to pass to the callback method.
        /// </summary>
        public readonly object[] CallbackParams;

        /// <summary>
        /// Initializes a new instance of the <see cref="Button"/> attribute.
        /// </summary>
        /// <param name="buttonLabel">The label displayed in the inspector.</param>
        /// <param name="callbackFn">The name of the method to invoke when clicked.</param>
        /// <param name="callbackParams">Optional parameters to pass to the callback method.</param>
        public Button(string buttonLabel, string callbackFn, object[] callbackParams = null)
        {
            Label = buttonLabel;
            CallbackFn = callbackFn;
            CallbackParams = callbackParams;
        }
    }
}