#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Data.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="DungeonTheme"/>.
    /// Provides a UI Toolkit based editor to manage the theme name, module categories, and module data.
    /// </summary>
    [CustomEditor(typeof(DungeonTheme))]
    public class DungeonThemeEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Serialized property for the dungeon theme's display name.
        /// </summary>
        private SerializedProperty titleProperty;

        /// <summary>
        /// Serialized property for the maximum number of modules that can be present within a theme.
        /// </summary>
        private SerializedProperty maximumModulesProperty;
        
        /// <summary>
        /// Serialized property for the minimum number of modules that can be present within a theme.
        /// </summary>
        private SerializedProperty minimumModulesProperty;

        /// <summary>
        /// Serialized property for the collection of dungeon module entries.
        /// </summary>
        private SerializedProperty dataProperty;
        
        /// <summary>
        /// Initializes references to <see cref="DungeonTheme"/> immediate serialized properties.
        /// </summary>
        /// <remarks>
        /// Since these property paths are only used once, they do not need to be included in <see cref="PropertyPaths"/>.
        /// </remarks>
        private void OnEnable()
        {
            titleProperty = serializedObject.FindProperty("themeTitle");
            maximumModulesProperty = serializedObject.FindProperty("maximumModules");
            minimumModulesProperty = serializedObject.FindProperty("minimumModules");
            dataProperty = serializedObject.FindProperty("moduleData");
        }

        /// <summary>
        /// Creates and configures the custom inspector interface for the <see cref="DungeonTheme"/>.
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
            
            var dataField = DataField();
            var weightsField = WeightsField();
            
            CalculateDataWeight(weightsField);
            
            dataField.TrackPropertyValue(dataProperty, _ =>
            {
                CalculateDataWeight(weightsField);
            });
            
            rootElement.Add(titleField);
            rootElement.Add(maximumModulesField);
            rootElement.Add(minimumModulesField);
            rootElement.Add(dataField);
            rootElement.Add(weightsField);
            
            return rootElement;
        }
        
        /// <summary>
        /// Creates and configures a <see cref="MultiColumnListView"/> element for displaying and editing the serialized <see cref="dataProperty"/> collection.
        /// </summary>
        /// <returns>
        /// A fully configured <see cref="MultiColumnListView"/> that supports reordering, add/removing content, and dynamic row heights.
        /// </returns>
        private MultiColumnListView DataField()
        {
            var dataList = new MultiColumnListView
            {
                bindingPath = dataProperty.propertyPath,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = false,
                style =
                {
                    marginTop = 5,
                    marginBottom = 5
                },
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            
            dataList.columns.Add(CategoryColumn());
            dataList.columns.Add(AssetsColumn());
            
            return dataList;
        }
        
        /// <summary>
        /// Creates a <see cref="Column"/> for selecting and display the module category associated with each entry in the data list.
        /// </summary>
        /// <returns>
        /// A stretchable <see cref="Column"/> bound to the <c>moduleCategory</c> property.
        /// </returns>
        private static Column CategoryColumn()
        {
            return new Column
            {
                bindingPath = "moduleCategory",
                stretchable = true,
                title = "Module Category"
            };
        }

        /// <summary>
        /// Creates a <see cref="Column"/> that displays and manages the collection of module assets for each data entry.
        /// This includes dynamically recalculating and displaying their cumulative weight.
        /// </summary>
        /// <returns>
        /// A stretchable <see cref="Column"/> bound to the <c>moduleAssets</c> property, with custom cell creation, binding, and destruction logic.
        /// </returns>
        private Column AssetsColumn()
        {
            return new Column
            {
                bindingPath = "moduleAssets",
                bindCell = (itemElement, itemIndex) =>
                {
                    var assetsList = itemElement.Q<MultiColumnListView>();
                    var weightsField = itemElement.Q<FloatField>();
                    
                    var dataElement = dataProperty.GetArrayElementAtIndex(itemIndex);
                    var assetsProperty = dataElement.FindPropertyRelative("moduleAssets");
                    
                    CalculateAssetsWeight(assetsProperty, weightsField);
                    
                    assetsList.BindProperty(assetsProperty);
                    assetsList.TrackPropertyValue(assetsProperty, _ =>
                    {
                        CalculateAssetsWeight(assetsProperty, weightsField);
                    });
                    
                },
                destroyCell = itemElement => itemElement.Clear(),
                makeCell = () =>
                {
                    var rootElement = new VisualElement();
                    
                    rootElement.Add(AssetsList());
                    rootElement.Add(WeightsField());
                    
                    return rootElement;
                },
                stretchable = true,
                title = "Module Assets",
                unbindCell = (itemElement, _) => itemElement.Unbind()
            };
        }

        /// <summary>
        /// Creates a <see cref="MultiColumnListView"/> for displaying and editing the collection of <see cref="ModuleAsset"/> objects for each data entry.
        /// </summary>
        /// <returns>
        /// A fully configured <see cref="MultiColumnListView"/> with columns generated from the <see cref="ModuleAsset"/> fields.
        /// </returns>
        private static MultiColumnListView AssetsList()
        {
            var assetsList = new MultiColumnListView
            {
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = false,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
                    
            var fieldInfos = typeof(ModuleAsset).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    
            foreach (var fieldInfo in fieldInfos)
            {
                var dataColumn = new Column
                {
                    bindingPath = fieldInfo.Name,
                    stretchable = true,
                    title = ObjectNames.NicifyVariableName(fieldInfo.Name)
                };
                        
                assetsList.columns.Add(dataColumn);
            }
            
            return assetsList;
        }
        
        /// <summary>
        /// Creates a disabled <see cref="FloatField"/> that displays the cumulative spawn rate for a given serialized property representing a collection of weighted elements.
        /// </summary>
        /// <returns>A read-only <see cref="FloatField"/> element showing the calculated cumulative weight.</returns>
        /// <remarks>
        /// The field is created as disabled to indicate that it is informational only and cannot be directly edited by the user.
        /// </remarks>
        private static FloatField WeightsField()
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
            
            return floatField;
        }
        
        /// <summary>
        /// Calculates the cumulative weight of all elements within the specified serialized property and updates the provided
        /// <see cref="FloatField"/> with the result.
        /// </summary>
        /// <param name="propertyList">
        /// The <see cref="SerializedProperty"/> representing an array of elements, each expected to have a <c>spawnRate</c> property contributing to the total weight.
        /// </param>
        /// <param name="targetField">The <see cref="FloatField"/> that will display the calculated cumulative weight.</param>
        private static void CalculateWeight(List<SerializedProperty> propertyList, FloatField targetField)
        {
            var cumulativeWeight = 0f;
        
            foreach (var arrayElement in propertyList)
            {
                var spawnRate = arrayElement.FindPropertyRelative("spawnRate");
                
                cumulativeWeight += spawnRate.floatValue;
            }
    
            var isValid = cumulativeWeight is >= 0.99f and <= 1f;
            
            targetField.labelElement.style.color = isValid ? StyleKeyword.Null : Color.softRed;
            targetField.labelElement.style.unityFontStyleAndWeight = isValid ? FontStyle.Normal : FontStyle.Bold;
            
            targetField.SetValueWithoutNotify(cumulativeWeight);
        }
        
        /// <summary>
        /// Calculates the cumulative weight of all <see cref="ModuleAsset"/> in the given <see cref="SerializedProperty"/> array and updates the provided <see cref="FloatField"/>.
        /// </summary>
        /// <param name="assetsProperty">
        /// The <see cref="SerializedProperty"/> array representing the list of module assets.
        /// </param>
        /// <param name="weightsField">
        /// The read-only <see cref="FloatField"/> used to display the cumulative weight.
        /// </param>
        private void CalculateAssetsWeight(SerializedProperty assetsProperty, FloatField weightsField)
        {
            var propertyList = new List<SerializedProperty>();

            for (var propertyIndex = 0; propertyIndex < assetsProperty.arraySize; propertyIndex++)
            {
                var assetProperty = assetsProperty.GetArrayElementAtIndex(propertyIndex);
                            
                propertyList.Add(assetProperty);
            }
                        
            CalculateWeight(propertyList, weightsField);
        }
        
        /// <summary>
        /// Calculates the cumulative weight of all <see cref="ModuleCategory"/> assignments in the serialized data array and updates the provided <see cref="FloatField"/>.
        /// </summary>
        /// <param name="weightsField">
        /// The read-only <see cref="FloatField"/> used to display the cumulative category weight.
        /// </param>
        private void CalculateDataWeight(FloatField weightsField)
        {
            var propertyList = new List<SerializedProperty>();

            for (var propertyIndex = 0; propertyIndex < dataProperty.arraySize; propertyIndex++)
            {
                var dataElement = dataProperty.GetArrayElementAtIndex(propertyIndex);
                var categoryProperty = dataElement.FindPropertyRelative("moduleCategory");
                            
                propertyList.Add(categoryProperty);
            }
                        
            CalculateWeight(propertyList, weightsField);
        }
    }
}

#endif