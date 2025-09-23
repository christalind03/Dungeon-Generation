#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using Code.Scripts.Attributes.Editor.Required;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Data.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="Theme"/>.
    /// Provides a UI Toolkit based editor to manage the theme name, module categories, and module data.
    /// </summary>
    [CustomEditor(typeof(Theme))]
    public class ThemeEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Serialized property for the dungeon theme's display name.
        /// </summary>
        private SerializedProperty titleProperty;

        private SerializedProperty maximumModulesProperty;
        private SerializedProperty minimumModulesProperty;
        
        /// <summary>
        /// Serialized property for the collection of dungeon module categories.
        /// </summary>
        private SerializedProperty categoriesProperty;

        /// <summary>
        /// Serialized property for the collection of dungeon module entries.
        /// </summary>
        private SerializedProperty dataProperty;
        
        /// <summary>
        /// Initializes references to <see cref="Theme"/> immediate serialized properties.
        /// </summary>
        /// <remarks>
        /// Since these property paths are only used once, they do not need to be included in <see cref="PropertyPaths"/>.
        /// </remarks>
        private void OnEnable()
        {
            titleProperty = serializedObject.FindProperty("themeTitle");
            maximumModulesProperty = serializedObject.FindProperty("maximumModules");
            minimumModulesProperty = serializedObject.FindProperty("minimumModules");
            categoriesProperty = serializedObject.FindProperty("moduleCategories");
            dataProperty = serializedObject.FindProperty("moduleData");
        }

        /// <summary>
        /// Creates and configures the custom inspector interface for the <see cref="Theme"/>.
        /// </summary>
        /// <returns>A <see cref="VisualElement"/> containing the root of the custom inspector.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();

            var titleField = new PropertyField(titleProperty)
            {
                style =
                {
                    marginBottom = 5,
                    marginTop = 5
                }
            };

            var maximumModulesField = new PropertyField(maximumModulesProperty);
            var minimumModulesField = new PropertyField(minimumModulesProperty);
            
            var categoriesField = CategoriesField();
            var dataField = DataField();
            
            var categoriesWeight = CumulativeWeightField(categoriesProperty);
            var dataWeight = CumulativeWeightField(dataProperty);
            
            // Ensure that when this property changes, the properties that are dependent on it are also updated
            categoriesField.TrackPropertyValue(categoriesProperty, serializedProperty =>
            {
                CalculateCumulativeWeight(serializedProperty, categoriesWeight);
                dataField?.RefreshItems();
            });

            dataField.TrackPropertyValue(dataProperty, serializedProperty =>
            {
                CalculateCumulativeWeight(serializedProperty, dataWeight);
            });

            rootElement.Add(titleField);
            rootElement.Add(maximumModulesField);
            rootElement.Add(minimumModulesField);
            rootElement.Add(categoriesField);
            rootElement.Add(categoriesWeight);
            rootElement.Add(dataField);
            rootElement.Add(dataWeight);

            return rootElement;
        }
        
        /// <summary>
        /// Creates the <see cref="ListView"/> for managing module categories.
        /// </summary>
        /// <returns>A <see cref="ListView"/> configured to display and edit module categories.</returns>
        private ListView CategoriesField()
        {
            var categoryList = new ListView
            {
                bindingPath = categoriesProperty.propertyPath,
                headerTitle = categoriesProperty.displayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                style =
                {
                    marginTop = 5
                },
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            
            // Prevent duplicating the previous element's values when creating a new element
            categoryList.itemsAdded += listIndices =>
            {
                foreach (var listIndex in listIndices)
                {
                    var targetElement = categoriesProperty.GetArrayElementAtIndex(listIndex);

                    targetElement.FindPropertyRelative("categoryID").stringValue = GUID.Generate().ToString();
                    targetElement.FindPropertyRelative("categoryTitle").stringValue = string.Empty;
                    targetElement.FindPropertyRelative("spawnLimits").boolValue = false;
                    targetElement.FindPropertyRelative("spawnMaximum").intValue = 0;
                    targetElement.FindPropertyRelative("spawnMinimum").intValue = 0;
                    targetElement.FindPropertyRelative("spawnRequired").boolValue = false;
                    targetElement.FindPropertyRelative("spawnRate").floatValue = 0;
                }
                
                serializedObject.ApplyModifiedProperties();
            };
            
            return categoryList;
        }
        
        /// <summary>
        /// Creates the <see cref="MultiColumnListView"/> for managing module data entries.
        /// </summary>
        /// <returns>A <see cref="MultiColumnListView"/> configured to display and edit module data entries.</returns>
        private MultiColumnListView DataField()
        {
            var dataList = new MultiColumnListView
            {
                bindingPath = dataProperty.propertyPath,
                headerTitle = dataProperty.displayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = true,
                showFoldoutHeader = true,
                style =
                {
                    marginTop = 5
                },
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };

            var fieldInfos = typeof(ModuleEntry).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var fieldInfo in fieldInfos)
            {
                var dataColumn = new Column();
                
                if (fieldInfo.Name == "moduleCategory")
                {
                    dataColumn.bindCell = (itemElement, itemIndex) =>
                    {
                        itemElement.Clear();
                        
                        var categoryIDs = new List<string>();

                        for (var i = 0; i < categoriesProperty.arraySize; i++)
                        {
                            var categoryProperty = categoriesProperty.GetArrayElementAtIndex(i);
                            var categoryID = categoryProperty.FindPropertyRelative("categoryID").stringValue;
                            
                            categoryIDs.Add(categoryID);
                        }

                        var moduleEntry = dataProperty.GetArrayElementAtIndex(itemIndex);
                        var moduleCategory = moduleEntry.FindPropertyRelative("moduleCategory");
                        
                        // We must manually render the required icon due to custom dropdown logic
                        var propertyField = new PopupField<string>(
                            new List<string>(),
                            0,
                            CategoryLabel,
                            CategoryLabel
                        )
                        {
                            choices = categoryIDs,
                            style =
                            {
                                flexGrow = 1
                            }
                        };

                        var propertyIcon = new Image
                        {
                            image = RequiredVisuals.RequiredIcon,
                            style =
                            {
                                marginLeft = 5,
                                height = RequiredVisuals.IconSize,
                                width = RequiredVisuals.IconSize,
                                visibility = string.IsNullOrEmpty(propertyField.value) ? Visibility.Visible : Visibility.Hidden
                            }
                        };

                        propertyField.BindProperty(moduleCategory);
                        propertyField.RegisterValueChangedCallback(_ =>
                        {
                            moduleCategory.serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(moduleCategory.serializedObject.targetObject);
                            
                            propertyIcon.style.visibility = string.IsNullOrEmpty(propertyField.value) ? Visibility.Visible : Visibility.Hidden;
                        });

                        itemElement.Add(propertyField);
                        itemElement.Add(propertyIcon);
;                    };

                    dataColumn.destroyCell = itemElement => itemElement.Clear();
                    
                    dataColumn.makeCell = () => new VisualElement
                    {
                        style =
                        {
                            alignItems = Align.Center,
                            flexDirection = FlexDirection.Row
                        }
                    };
                    
                    dataColumn.unbindCell = (itemElement, _) => itemElement.Unbind();
                }

                dataColumn.bindingPath = fieldInfo.Name;
                dataColumn.stretchable = true;
                dataColumn.title = ObjectNames.NicifyVariableName(fieldInfo.Name);
                
                dataList.columns.Add(dataColumn);
            }
            
            // Prevent duplicating the previous element's values when creating a new element
            dataList.itemsAdded += listIndices =>
            {
                foreach (var listIndex in listIndices)
                {
                    var targetElement = dataProperty.GetArrayElementAtIndex(listIndex);

                    targetElement.FindPropertyRelative("moduleCategory").stringValue = string.Empty;
                    targetElement.FindPropertyRelative("modulePrefab").objectReferenceValue = null;
                    targetElement.FindPropertyRelative("spawnRate").floatValue = 0f;
                    targetElement.FindPropertyRelative("spawnOnce").boolValue = false;
                }

                serializedObject.ApplyModifiedProperties();
            };
            
            return dataList;
        }

        /// <summary>
        /// Returns a display-friendly label for a module category given its unique identifier.
        /// </summary>
        /// <param name="targetID">The unique identifier of the category.</param>
        /// <returns>
        /// A formatted string containing the index and category name, or <c>null</c> if no category matches the identifier.
        /// </returns>
        private string CategoryLabel(string targetID)
        {
            for (var i = 0; i < categoriesProperty.arraySize; i++)
            {
                var categoryProperty = categoriesProperty.GetArrayElementAtIndex(i);
                
                var categoryID = categoryProperty.FindPropertyRelative("categoryID").stringValue;
                var categoryTitle = categoryProperty.FindPropertyRelative("categoryTitle").stringValue;
                
                if (categoryID == targetID) return $"[{i}] {categoryTitle}";
            }

            return null;
        }

        /// <summary>
        /// Creates a disabled <see cref="FloatField"/> that displays the cumulative spawn rate for a given serialized property representing a collection of weighted elements.
        /// </summary>
        /// <param name="serializedProperty">
        /// The <see cref="SerializedProperty"/> used to calculate the cumulative weight value.
        /// Usually represents a collection or element whose weight contributes to a total.
        /// </param>
        /// <returns>A read-only <see cref="FloatField"/> element showing the calculated cumulative weight.</returns>
        /// <remarks>
        /// The field is created as disabled to indicate that it is informational only and cannot be directly edited by the user.
        /// </remarks>
        private static FloatField CumulativeWeightField(SerializedProperty serializedProperty)
        {
            var floatField = new FloatField("Cumulative Weight")
            {
                enabledSelf = false,
                style =
                {
                    marginBottom = 5,
                    marginTop = 5
                }
            };
            
            CalculateCumulativeWeight(serializedProperty, floatField);
            
            return floatField;
        }

        /// <summary>
        /// Calculates the cumulative weight of all elements within the specified serialized property and updates the provided
        /// <see cref="FloatField"/> with the result.
        /// </summary>
        /// <param name="serializedProperty">
        /// The <see cref="SerializedProperty"/> representing an array of elements, each expected to have a <c>spawnRate</c> property contributing to the total weight.
        /// </param>
        /// <param name="targetField">The <see cref="FloatField"/> that will display the calculated cumulative weight.</param>
        private static void CalculateCumulativeWeight(SerializedProperty serializedProperty, FloatField targetField)
        {
            var cumulativeWeight = 0f;

            if (serializedProperty.isArray)
            {
                for (var arrayIndex = 0; arrayIndex < serializedProperty.arraySize; arrayIndex++)
                {
                    var arrayElement = serializedProperty.GetArrayElementAtIndex(arrayIndex);
                    var spawnRate = arrayElement.FindPropertyRelative("spawnRate");
                    
                    cumulativeWeight += spawnRate.floatValue;
                }

                var isValid = cumulativeWeight is >= 0.99f and <= 1f;
                
                targetField.labelElement.style.color = isValid ? StyleKeyword.Null : Color.softRed;
                targetField.labelElement.style.unityFontStyleAndWeight = isValid ? FontStyle.Normal : FontStyle.Bold;
            }
            
            targetField.SetValueWithoutNotify(cumulativeWeight);
        }
    }
}

#endif