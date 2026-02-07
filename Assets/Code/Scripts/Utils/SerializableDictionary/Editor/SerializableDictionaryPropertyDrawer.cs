#if UNITY_EDITOR

using Code.Scripts.Utils.SerializableDictionary.Attributes;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Utils.SerializableDictionary.Editor
{
    /// <summary>
    /// A custom property drawer for <see cref="SerializableDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// If a <see cref="SerializableDictionaryAttribute"/> attribute was applied, the 
    /// </remarks>
    [CustomPropertyDrawer(typeof(SerializableDictionaryTemplate<,,>), true)]
    internal class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        /// <summary>
        /// Creates the custom property inspector user interface for <see cref="SerializableDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="serializedProperty">
        /// The <see cref="SerializedProperty"/> representing the <see cref="SerializableDictionary{TKey,TValue}"/> instance to be displayed and edited.
        /// </param>
        /// <returns>A <see cref="VisualElement"/> container holding all generated fields for this property.</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty serializedProperty)
        {
            var keysProperty = serializedProperty.FindPropertyRelative("dictionaryKeys");
            var valuesProperty = serializedProperty.FindPropertyRelative("dictionaryValues");
            
            if (keysProperty == null || valuesProperty == null || keysProperty.arraySize != valuesProperty.arraySize)
            {
                Debug.LogError("[SerializableDictionary] Unable to create Property GUI due to an invalid SerializableDictionary structure.");
                return null;
            }
            
            var dictionaryAttribute = fieldInfo.GetCustomAttribute<SerializableDictionaryAttribute>();
            var rootElement = new MultiColumnListView
            {
                itemsSource = Enumerable.Range(0, keysProperty.arraySize).ToList(),
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            
            rootElement.columns.Add(CreateColumn(dictionaryAttribute?.KeyLabel ?? "Keys", keysProperty));
            rootElement.columns.Add(CreateColumn(dictionaryAttribute?.ValueLabel ?? "Values", valuesProperty));
            
            rootElement.onAdd += _ =>
            {
                var arraySize = keysProperty.arraySize;
                
                keysProperty.InsertArrayElementAtIndex(arraySize);
                valuesProperty.InsertArrayElementAtIndex(arraySize);
                
                serializedProperty.serializedObject.ApplyModifiedProperties();
                rootElement.itemsSource.Add(arraySize);
                rootElement.RefreshItems();
            };

            rootElement.onRemove += _ =>
            {
                if (keysProperty.arraySize <= 0) return;
                
                var arraySize = keysProperty.arraySize - 1;
                
                keysProperty.DeleteArrayElementAtIndex(arraySize);
                valuesProperty.DeleteArrayElementAtIndex(arraySize);
                
                serializedProperty.serializedObject.ApplyModifiedProperties();
                rootElement.itemsSource.Remove(arraySize);
                rootElement.RefreshItems();
            };
            
            return rootElement;
        }

        /// <summary>
        /// Creates a column for the <see cref="MultiColumnListView"/> used in the <see cref="SerializableDictionary{TKey,TValue}"/> drawer.
        /// </summary>
        /// <param name="columnTitle">The display title of the column.</param>
        /// <param name="serializedProperty">The <see cref="SerializedProperty"/> to bind the column cells to.</param>
        /// <returns>A configured <see cref="Column"/> for the list view.</returns>
        private static Column CreateColumn(string columnTitle, SerializedProperty serializedProperty)
        {
            return new Column
            {
                bindCell = (itemElement, itemIndex) =>
                {
                    var propertyTarget = serializedProperty.GetArrayElementAtIndex(itemIndex);
                    (itemElement as PropertyField).BindProperty(propertyTarget);
                },
                makeCell = () => new PropertyField
                {
                    label = string.Empty
                },
                stretchable = true,
                title = columnTitle
            };
        }

    }
}

#endif