#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Behaviours.Editor
{
    /// <summary>
    /// A custom inspector for <see cref="DungeonGenerator"/> objects.
    /// Provides grouped property fields and editor-only tools for generation control.
    /// </summary>
    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonGeneratorEditor : UnityEditor.Editor
    {
        /// <summary>
        /// The currently inspected <see cref="DungeonGenerator"/> instance.
        /// </summary>
        private DungeonGenerator activeObject;

        /// <summary>
        /// Serialized reference to the list of possible dungeon themes.
        /// </summary>
        private SerializedProperty possibleThemesProperty;

        /// <summary>
        /// Serialized reference to the dungeon placement layers configuration.
        /// </summary>
        private SerializedProperty placementLayersProperty;

        /// <summary>
        /// Serialized reference to the flag that controls loop generation.
        /// </summary>
        private SerializedProperty enableLoopsProperty;

        /// <summary>
        /// Serialized reference to the event triggered when dungeon generation fails.
        /// </summary>
        private SerializedProperty onGenerationFailedProperty;

        /// <summary>
        /// Serialized reference to the event triggered when dungeon generation succeeds.
        /// </summary>
        private SerializedProperty onGenerationSuccessProperty;

        /// <summary>
        /// Caches serialized properties when enabled.
        /// </summary>
        private void OnEnable()
        {
            activeObject = target as DungeonGenerator;
            
            possibleThemesProperty = serializedObject.FindProperty("possibleThemes");
            placementLayersProperty = serializedObject.FindProperty("placementLayers");
            enableLoopsProperty = serializedObject.FindProperty("enableLoops");
            onGenerationFailedProperty = serializedObject.FindProperty("onGenerationFailed");
            onGenerationSuccessProperty = serializedObject.FindProperty("onGenerationSuccess");
        }
        
        /// <summary>
        /// Creates and configures the custom inspector interface for the <see cref="DungeonGenerator"/>.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the root of the custom inspector.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();
            
            rootElement.Add(HeaderLabel("Generation Parameters"));
            rootElement.Add(InspectorSpace(3));
            rootElement.Add(new PropertyField(possibleThemesProperty));
            rootElement.Add(InspectorSpace(5));
            rootElement.Add(new PropertyField(placementLayersProperty));
            rootElement.Add(new PropertyField(enableLoopsProperty));
            rootElement.Add(InspectorSpace(10));
            rootElement.Add(HeaderLabel("Generation Callbacks"));
            rootElement.Add(new PropertyField(onGenerationFailedProperty));
            rootElement.Add(InspectorSpace(5));
            rootElement.Add(new PropertyField(onGenerationSuccessProperty));
            rootElement.Add(EditorUtilities());
            
            return rootElement;
        }

        /// <summary>
        /// Creates a bold label for section headers in the Inspector. 
        /// </summary>
        /// <param name="labelText">The text to display in the header label.</param>
        /// <returns>A <see cref="Label"/> element with bold text styling.</returns>
        private static VisualElement HeaderLabel(string labelText)
        {
            return new Label(labelText)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
        }
        
        /// <summary>
        /// Inserts a fixed vertical space between Inspector sections.
        /// </summary>
        /// <param name="spaceSize">The pixel size of the gap.</param>
        /// <returns>A blank <see cref="VisualElement"/> with a specified size.</returns>
        private static VisualElement InspectorSpace(int spaceSize)
        {
            return new VisualElement
            {
                style =
                {
                    height = spaceSize,
                    width = spaceSize
                }
            };
        }
        
        /// <summary>
        /// Creates a group of buttons for generating and destroying dungeon layouts.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> container containing the layout control buttons.</returns>
        private VisualElement EditorUtilities()
        {
            var editorUtilities = new VisualElement();

            var buttonContainer = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1
                }
            };
            
            var generateButton = InvokeButton("Generate", "Generate");
            var destroyButton = InvokeButton("Destroy", "Destroy");
            
            buttonContainer.Add(generateButton);
            buttonContainer.Add(destroyButton);
            
            editorUtilities.Add(HeaderLabel("Editor Utilities"));
            editorUtilities.Add(buttonContainer);
            
            return editorUtilities;
        }
        
        /// <summary>
        /// Creates a button that invokes a method on the target <see cref="DungeonGenerator"/> via reflection.
        /// </summary>
        /// <param name="buttonLabel">The text displayed on the button.</param>
        /// <param name="targetMethod">The method name to invoke on the target.</param>
        /// <returns>A <see cref="Button"/> element configured to call the specified method.</returns>
        private Button InvokeButton(string buttonLabel, string targetMethod)
        {
            var buttonElement = new Button
            {
                style =
                {
                    flexGrow = 1  
                },
                text = buttonLabel
            };
            
            var methodInfo = activeObject.GetType().GetMethod(
                targetMethod, 
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
            );

            buttonElement.clicked += () =>
            {
                buttonElement.SetEnabled(false);
                methodInfo?.Invoke(activeObject, null);
                buttonElement.SetEnabled(true);
            };
            
            return buttonElement;
        }
    }
}

#endif