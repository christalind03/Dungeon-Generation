#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Code.Scripts.Dungeon.Editor
{
    [CustomEditor(typeof(DungeonTheme))]
    public class DungeonThemeEditor : UnityEditor.Editor
    {
        private SerializedProperty themeName;
        private SerializedProperty moduleCategories;
        private SerializedProperty moduleData;

        private static class PropertyPaths
        {
            public const string CategoryID = "categoryID";
            public const string CategoryName = "categoryName";
            public const string ModuleCategory = "moduleCategory";
            public const string ModulePrefab = "modulePrefab";
            public const string SpawnLimit = "spawnLimit";
            public const string SpawnMax = "spawnMax";
            public const string SpawnMin = "spawnMin";
            public const string SpawnOnce = "spawnOnce";
            public const string SpawnRate = "spawnRate";
        }
        
        private void OnEnable()
        {
            themeName = serializedObject.FindProperty("themeName");
            moduleCategories = serializedObject.FindProperty("moduleCategories");
            moduleData = serializedObject.FindProperty("moduleData");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();

            var themeNameProperty = new PropertyField(themeName)
            {
                style =
                {
                    marginBottom = 5,
                    marginTop = 5
                }
            };
                        
            var moduleCategoriesProperty = CreateCategoryList();
            var moduleDataProperty = CreateDataList();
            
            // Ensure that when this property changes, the properties that are dependent on it are also updated
            moduleCategoriesProperty.TrackPropertyValue(moduleCategories, _ =>
            {
                moduleDataProperty.RefreshItems();
            });
            
            rootElement.Add(themeNameProperty);
            rootElement.Add(moduleCategoriesProperty);
            rootElement.Add(moduleDataProperty);
            
            return rootElement;
        }

        private BaseListView CreateCategoryList()
        {
            var moduleCategoriesList = new ListView
            {
                bindItem = (itemElement, itemIndex) =>
                {
                    itemElement.Clear();

                    var foldoutElement = (Foldout)itemElement;
                    var moduleCategory = moduleCategories.GetArrayElementAtIndex(itemIndex);
                    
                    CreateCategoryElement(itemIndex, foldoutElement, moduleCategory);
                },
                bindingPath = moduleCategories.propertyPath,
                destroyItem = itemElement => itemElement.Clear(),
                headerTitle = moduleCategories.displayName,
                makeItem = () => new Foldout(),
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = false,
                showFoldoutHeader = true,
                unbindItem = (itemElement, _) =>
                {
                    foreach (var childProperty in itemElement.Children())
                    {
                        if (childProperty is PropertyField propertyField)
                        {
                            propertyField.Unbind();
                        }
                    }
                    
                    itemElement.Clear();
                },
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            
            // Prevent duplicating the previous element's values when creating a new element
            moduleCategoriesList.itemsAdded += listIndices =>
            {
                foreach (var listIndex in listIndices)
                {
                    var insertedElement = moduleCategories.GetArrayElementAtIndex(listIndex);
                    
                    insertedElement.FindPropertyRelative(PropertyPaths.CategoryID).stringValue = GUID.Generate().ToString();
                    insertedElement.FindPropertyRelative(PropertyPaths.CategoryName).stringValue = string.Empty;
                    insertedElement.FindPropertyRelative(PropertyPaths.SpawnLimit).boolValue = false;
                    insertedElement.FindPropertyRelative(PropertyPaths.SpawnMax).intValue = 0;
                    insertedElement.FindPropertyRelative(PropertyPaths.SpawnMin).intValue = 0;
                    insertedElement.FindPropertyRelative(PropertyPaths.SpawnRate).floatValue = 0;
                }
                
                serializedObject.ApplyModifiedProperties();
            };
            
            moduleCategoriesList.BindProperty(moduleCategories);
            
            return moduleCategoriesList;
        }

        private BaseListView CreateDataList()
        {
            var moduleDataList = new MultiColumnListView
            {
                bindingPath = moduleData.propertyPath,
                headerTitle = moduleData.displayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = true,
                showBorder = true,
                showBoundCollectionSize = false,
                showFoldoutHeader = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            
            CreateDataColumns(moduleDataList);
            
            // Prevent duplicating the previous element's values when creating a new element
            moduleDataList.itemsAdded += listIndices =>
            {
                foreach (var listIndex in listIndices)
                {
                    var insertedElement = moduleData.GetArrayElementAtIndex(listIndex);

                    insertedElement.FindPropertyRelative(PropertyPaths.ModuleCategory).stringValue = string.Empty;
                    insertedElement.FindPropertyRelative(PropertyPaths.ModulePrefab).objectReferenceValue = null;
                    insertedElement.FindPropertyRelative(PropertyPaths.SpawnRate).floatValue = 0;
                    insertedElement.FindPropertyRelative(PropertyPaths.SpawnOnce).boolValue = false;
                }

                serializedObject.ApplyModifiedProperties();
            };
            
            moduleDataList.BindProperty(moduleData);
            
            return moduleDataList;
        }
        
        private void CreateCategoryElement(int itemIndex, Foldout itemElement, SerializedProperty serializedProperty)
        {
            var propertyContainerElement = new VisualElement();

            var categoryName = serializedProperty.FindPropertyRelative(PropertyPaths.CategoryName);
            var categoryNameProperty = new PropertyField(categoryName);
            
            categoryNameProperty.BindProperty(categoryName);
            categoryNameProperty.RegisterValueChangeCallback(changeEvent =>
            {
                var updatedName = changeEvent.changedProperty.stringValue;
                itemElement.text = string.IsNullOrEmpty(updatedName) ? $"Element {itemIndex}" : updatedName;
            });
            
            var spawnLimit = serializedProperty.FindPropertyRelative(PropertyPaths.SpawnLimit);
            var spawnLimitProperty = new PropertyField(spawnLimit);
            
            spawnLimitProperty.BindProperty(spawnLimit);
            
            var spawnRangeProperty = new VisualElement
            {
                style =
                {
                    marginLeft = 15
                }
            };
            
            var spawnMax = serializedProperty.FindPropertyRelative(PropertyPaths.SpawnMax);
            var spawnMaxProperty = new PropertyField(spawnMax);
            
            spawnMaxProperty.BindProperty(spawnMax);
            
            var spawnMin = serializedProperty.FindPropertyRelative(PropertyPaths.SpawnMin);
            var spawnMinProperty = new PropertyField(spawnMin);
            
            spawnMinProperty.BindProperty(spawnMin);
            
            spawnRangeProperty.Add(spawnMaxProperty);
            spawnRangeProperty.Add(spawnMinProperty);
            
            spawnLimitProperty.RegisterValueChangeCallback(changeEvent =>
            {
                spawnRangeProperty.style.display = changeEvent.changedProperty.boolValue
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            var spawnRate = serializedProperty.FindPropertyRelative(PropertyPaths.SpawnRate);
            var spawnRateProperty = new PropertyField(spawnRate);
            
            spawnRateProperty.BindProperty(spawnRate);
            
            propertyContainerElement.Add(categoryNameProperty);
            propertyContainerElement.Add(spawnLimitProperty);
            propertyContainerElement.Add(spawnRangeProperty);
            propertyContainerElement.Add(spawnRateProperty);
            
            itemElement.Add(propertyContainerElement);
        }
        
        private void CreateDataColumns(MultiColumnListView moduleDataList)
        {
            var moduleCategory = new Column
            {
                bindCell = (itemElement, itemIndex) =>
                {
                    var categoryIDs = new List<string>();
                    
                    for (var arrayIndex = 0; arrayIndex < moduleCategories.arraySize; arrayIndex++)
                    {
                        var categoryProperty = moduleCategories.GetArrayElementAtIndex(arrayIndex);
                        var categoryID = categoryProperty.FindPropertyRelative(PropertyPaths.CategoryID).stringValue;
                        
                        categoryIDs.Add(categoryID);
                    }
                    
                    var moduleEntry = moduleData.GetArrayElementAtIndex(itemIndex);
                    var moduleCategory = moduleEntry.FindPropertyRelative(PropertyPaths.ModuleCategory);
                    var propertyField = (PopupField<string>)itemElement;
                    
                    propertyField.choices = categoryIDs;
                  
                    propertyField.BindProperty(moduleCategory);
                },
                bindingPath = PropertyPaths.ModuleCategory,
                destroyCell = itemElement => itemElement.Clear(), 
                makeCell = () => new PopupField<string>(
                    new List<string>(),
                    0,
                    DisplayCategoryLabel,
                    DisplayCategoryLabel
                ),
                stretchable = true,
                title = "Module Category",
                unbindCell = (itemElement, _) => itemElement.Unbind()
            };

            var modulePrefab = new Column
            {
                bindCell = (itemElement, itemIndex) =>
                {
                    var moduleEntry = moduleData.GetArrayElementAtIndex(itemIndex);
                    var modulePrefab = moduleEntry.FindPropertyRelative(PropertyPaths.ModulePrefab);
                    var propertyField = (PropertyField)itemElement;

                    propertyField.BindProperty(modulePrefab);
                },
                bindingPath = PropertyPaths.ModulePrefab,
                destroyCell = itemElement => itemElement.Clear(),
                makeCell = () => new PropertyField
                {
                    label = string.Empty
                },
                stretchable = true,
                title = "Module Prefab",
                unbindCell = (itemElement, _) => itemElement.Unbind()
            };
            
            var spawnRate = new Column
            {
                bindCell = (itemElement, itemIndex) =>
                {
                    var moduleEntry = moduleData.GetArrayElementAtIndex(itemIndex);
                    var moduleSpawnRate = moduleEntry.FindPropertyRelative(PropertyPaths.SpawnRate);
                    var propertyField = (PropertyField)itemElement;
                    
                    propertyField.BindProperty(moduleSpawnRate);
                },
                bindingPath = PropertyPaths.SpawnRate,
                destroyCell = itemElement => itemElement.Clear(),
                makeCell = () => new PropertyField
                {
                    label = string.Empty
                },
                stretchable = true,
                title = "Spawn Rate",
                unbindCell = (itemElement, _) => itemElement.Unbind()
            };
            
            var spawnOnce = new Column
            {
                bindCell = (itemElement, itemIndex) =>
                {
                    var moduleEntry = moduleData.GetArrayElementAtIndex(itemIndex);
                    var moduleSpawnOnce = moduleEntry.FindPropertyRelative(PropertyPaths.SpawnOnce);
                    var propertyField = (Toggle)itemElement;
                    
                    propertyField.BindProperty(moduleSpawnOnce);
                },
                bindingPath = PropertyPaths.SpawnOnce,
                destroyCell = itemElement => itemElement.Clear(),
                makeCell = () => new Toggle(),
                stretchable = true,
                title = "Spawn Once",
                unbindCell = (itemElement, _) => itemElement.Unbind()
            };
            
            moduleDataList.columns.Add(moduleCategory);
            moduleDataList.columns.Add(modulePrefab);
            moduleDataList.columns.Add(spawnRate);
            moduleDataList.columns.Add(spawnOnce);
        }

        private string DisplayCategoryLabel(string categoryID)
        {
            for (var arrayIndex = 0; arrayIndex < moduleCategories.arraySize; arrayIndex++)
            {
                var targetCategory = moduleCategories.GetArrayElementAtIndex(arrayIndex);
                var targetCategoryID = targetCategory.FindPropertyRelative(PropertyPaths.CategoryID).stringValue;
                var targetCategoryName = targetCategory.FindPropertyRelative(PropertyPaths.CategoryName).stringValue;
                
                if (categoryID == targetCategoryID)
                {
                    return $"[{arrayIndex}] {targetCategoryName}"; 
                }
            }

            return null;
        }
    }
}

#endif