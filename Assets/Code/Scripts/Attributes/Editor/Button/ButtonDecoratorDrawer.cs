#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Attributes.Editor.Button
{
    /// <summary>
    /// A custom decorator drawer for the <see cref="Attributes.Button"/> attribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(Attributes.Button))]
    public class ButtonDecoratorDrawer : DecoratorDrawer
    {
        /// <summary>
        /// Generates a <see cref="UnityEngine.UIElements.Button"/> that calls the method specified by the attribute when clicked.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> representing the button.</returns>
        public override VisualElement CreatePropertyGUI()
        {
            var buttonAttribute = (Attributes.Button)attribute;
            
            var targetObject = ResolveObject();
            var targetMethod = ResolveMethod(buttonAttribute.CallbackFn, targetObject);
            
            var rootElement = new UnityEngine.UIElements.Button
            {
                text = buttonAttribute.Label
            };

            rootElement.clicked += () =>
            {
                rootElement.SetEnabled(false);

                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        targetMethod.Invoke(targetObject, buttonAttribute.CallbackParams);
                    }
                    finally
                    {
                        rootElement.SetEnabled(true);
                    }
                };
            };
            
            return rootElement;
        }

        /// <summary>
        /// Finds the <see cref="Object"/> on which the button's callback method should be invoked.
        /// </summary>
        /// <returns>The target <see cref="Object"/> for the callback method.</returns>
        private static Object ResolveObject()
        {
            if (Selection.activeObject is GameObject gameObject)
            {
                var inspectedComponent = gameObject.GetComponent<MonoBehaviour>();
                return inspectedComponent ?? Selection.activeObject;
            }
            
            return Selection.activeObject;
        }
        
        /// <summary>
        /// Finds the <see cref="MethodInfo"/> corresponding to the given method name on the target <see cref="Object"/>.
        /// </summary>
        /// <param name="methodName">The name of the method to find.</param>
        /// <param name="targetObject">The <see cref="Object"/> on which to look for the method.</param>
        /// <returns>The <see cref="MethodInfo"/> if found; otherwise, null.</returns>
        private static MethodInfo ResolveMethod(string methodName, Object targetObject)
        {
            return targetObject?.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }
    }
}

#endif