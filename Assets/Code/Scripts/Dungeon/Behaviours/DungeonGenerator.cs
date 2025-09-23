using Code.Scripts.Algorithms;
using Code.Scripts.Dungeon.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Scripts.Dungeon.Behaviours
{
    /// <summary>
    /// Procedurally constructs a dungeon layout at runtime using a selected <see cref="Theme"/>.
    /// This generator selects module categories and modules based on weighted probabilities and spawns them into the scene to create a randomized, replayable dungeon experience.
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("References to the available dungeon theme assets used by this dungeon.")]
        private Theme[] availableThemes;

        /// <summary>
        /// Automatically generates the dungeon when the object is loaded into the scene.
        /// </summary>
        private void Awake()
        {
            Generate();
        }

        /// <summary>
        /// Generates a randomized dungeon by completing the following steps:
        /// <list type="bullet">
        /// <item><description>Picking a random <see cref="Theme"/> from the available themes</description></item>
        /// <item><description>Setting up probability tables for the weighted category and module selection</description></item>
        /// <item><description>Spawning modules until the target count is reached, respecting required category constraints</description></item>
        /// </list>
        /// </summary>
        private void Generate()
        {
            var activeTheme = availableThemes[Random.Range(0, availableThemes.Length)];
            
            // Create alias probability lookup tables for drawing random values in O(1) time
            var categoryProbabilities = CreateCategoryProbabilities(activeTheme);
            var categorizedModulesProbabilities = CreateCategorizedModulesProbabilities(activeTheme);
            
            // Generate the dungeon layout by dynamically spawning module prefabs and verifying their locations in world space
            var activeModules = new List<GameObject>();
            var targetModuleCount = Random.Range(activeTheme.MinimumModules, activeTheme.MaximumModules + 1);
            
            var requiredModuleCategories = activeTheme.ModuleCategories.Select(moduleCategory => moduleCategory.SpawnRequired).ToList();
            
            while (activeModules.Count < targetModuleCount)
            {
                var chosenCategory = categoryProbabilities.Sample();

                if (chosenCategory.SpawnRequired)
                {
                    var activeSpawnCount = 0;
                    var targetSpawnCount = Random.Range(chosenCategory.SpawnMinimum, chosenCategory.SpawnMaximum + 1);

                    while (activeSpawnCount < targetSpawnCount)
                    {
                        var chosenModule = categorizedModulesProbabilities[chosenCategory.CategoryID].Sample();
                        
                        activeSpawnCount++;
                        activeModules.Add(Instantiate(chosenModule.ModulePrefab));
                    }
                }
                else
                {
                    var chosenModule = categorizedModulesProbabilities[chosenCategory.CategoryID].Sample();
                     
                    activeModules.Add(Instantiate(chosenModule.ModulePrefab));
                }
            }
        }

        /// <summary>
        /// Creates an <see cref="AliasProbability{TObject}"/> table for all non-required module categories.
        /// </summary>
        /// <param name="activeTheme">The currently selected dungeon theme.</param>
        /// <returns>An alias probability lookup table for selecting <see cref="ModuleCategory"/> objects.</returns>
        private static AliasProbability<ModuleCategory> CreateCategoryProbabilities(Theme activeTheme)
        {
            var categoryList = new List<ModuleCategory>();
            var weightsList = new List<float>();
            
            foreach (var moduleCategory in activeTheme.ModuleCategories)
            {
                if (moduleCategory.SpawnRequired) continue;
                
                categoryList.Add(moduleCategory);
                weightsList.Add(moduleCategory.SpawnRate);
            }
            
            return new AliasProbability<ModuleCategory>(categoryList, weightsList);
        }
        
        /// <summary>
        /// Creates alias probability tables for all modules in the theme, grouped by their category.
        /// </summary>
        /// <param name="activeTheme">The currently selected dungeon theme.</param>
        /// <returns>
        /// A dictionary mapping category identifiers to <see cref="AliasProbability{TObject}"/> instances for sampling module entries within that category.
        /// </returns>
        private static Dictionary<string, AliasProbability<ModuleEntry>> CreateCategorizedModulesProbabilities(Theme activeTheme)
        {
            var categorizedModules = new Dictionary<string, List<ModuleEntry>>();
            var categorizedModulesProbabilities = new Dictionary<string, AliasProbability<ModuleEntry>>();

            // Categorize all module entries based on their module category
            foreach (var moduleEntry in activeTheme.ModuleData)
            {
                if (categorizedModules.TryGetValue(moduleEntry.ModuleCategory, out var entryList) == false)
                {
                    entryList = new List<ModuleEntry>();
                    categorizedModules[moduleEntry.ModuleCategory] = entryList;
                }
                
                entryList.Add(moduleEntry);
            }
            
            // Create alias probability lookup tables for module entries in each module category
            foreach (var (moduleCategory, moduleList) in categorizedModules)
            {
                categorizedModulesProbabilities[moduleCategory] = new AliasProbability<ModuleEntry>(
                    moduleList,
                    moduleList.Select(categorizedEntry => categorizedEntry.SpawnRate).ToList()
                );
            }
            
            return categorizedModulesProbabilities;
        }
    }
}