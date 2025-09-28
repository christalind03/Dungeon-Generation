using Code.Scripts.Algorithms;
using Code.Scripts.Dungeon.Data;
using System;
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
        /// The currently selected <see cref="Theme"/> for dungeon generation.
        /// </summary>
        private Theme activeTheme;
        
        /// <summary>
        /// A mapping of <see cref="ModuleCategory"/> entries to their remaining spawn counts.
        /// </summary>
        /// <remarks>
        /// The dictionary keys are only <see cref="ModuleCategory"/> entries which have <c>SpawnLimits</c> set to <c>true</c>.
        /// </remarks>
        private Dictionary<ModuleCategory, int> restrictedCategories;
        
        /// <summary>
        /// A collection of <see cref="ModuleAsset"/> instances that must be placed in the dungeon.
        /// </summary>
        private List<ModuleAsset> requiredModules;
        
        /// <summary>
        /// A collection of <see cref="ModuleAsset"/> instances that can only be spawned once.
        /// </summary>
        private List<ModuleAsset> uniqueModules;
        
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
            SelectTheme();
            
            // Create alias probability lookup tables for drawing random values in O(1) time
            var categoryProbabilities = CreateCategoryProbabilities();
            var categorizedModuleProbabilities = CreateCategorizedModuleProbabilities();
            
            // Generate the dungeon layout by dynamically spawning module prefabs and verifying their locations in world space
            var moduleCount = 0;
            var moduleLimit = UnityEngine.Random.Range(activeTheme.MinimumModules, activeTheme.MaximumModules + 1);

            restrictedCategories = activeTheme.ModuleData
                .Where(moduleElement => moduleElement.ModuleCategory.SpawnLimits)
                .ToDictionary(
                    moduleElement => moduleElement.ModuleCategory,
                    _ => 0
                );
            
            InitializeRequiredModules(moduleLimit, categorizedModuleProbabilities);

            while (moduleCount < moduleLimit)
            {
                var openSlots = moduleLimit - moduleCount;
                if (openSlots == requiredModules.Count)
                {
                    InstantiateRequiredModule(ref moduleCount);
                    continue;
                }

                if (0 < requiredModules.Count)
                {
                    const float baseChance = 0.5f;
                    
                    var requiredUrgency = Mathf.Clamp01((float)requiredModules.Count / openSlots);
                    var requiredChance = Mathf.Lerp(baseChance, 1f, requiredUrgency);
                    var randomChance = UnityEngine.Random.value;

                    if (randomChance <= requiredChance)
                    {
                        InstantiateRequiredModule(ref moduleCount);
                        continue;
                    }
                }

                var chosenCategory = categoryProbabilities.Sample();
                if (chosenCategory.SpawnLimits)
                {
                    if (chosenCategory.SpawnMaximum <= restrictedCategories[chosenCategory]) continue;

                    var spawnCount = 0;
                    var spawnLimit = UnityEngine.Random.Range(chosenCategory.SpawnMinimum, chosenCategory.SpawnMaximum + 1);

                    if (openSlots < spawnLimit)
                    {
                        spawnLimit = Math.Min(
                            openSlots,
                            chosenCategory.SpawnMaximum - restrictedCategories[chosenCategory]
                        );
                    }

                    var anticipatedCount = restrictedCategories[chosenCategory] + spawnLimit;
                    if (anticipatedCount > chosenCategory.SpawnMaximum) continue;

                    while (spawnCount < spawnLimit)
                    {
                        var chosenModule = SampleModule(chosenCategory, categorizedModuleProbabilities);
                        if (chosenModule == null) continue;

                        Instantiate(chosenModule.ModulePrefab);
                        
                        spawnCount++;
                        moduleCount++;
                        restrictedCategories[chosenCategory]++;
                    }
                }
                else
                {
                    var chosenModule = SampleModule(chosenCategory, categorizedModuleProbabilities);
                    if (chosenModule == null) continue;
                    
                    Instantiate(chosenModule.ModulePrefab);

                    moduleCount++;
                }
            }
        }

        /// <summary>
        /// Randomly selects and assigns the <see cref="activeTheme"/> from the available themes.
        /// </summary>
        private void SelectTheme()
        {
            activeTheme = availableThemes[UnityEngine.Random.Range(0, availableThemes.Length)];
        }
        
        /// <summary>
        /// Creates an <see cref="AliasProbability{TObject}"/> table for all non-required module categories.
        /// </summary>
        /// <returns>An alias probability lookup table for selecting <see cref="ModuleCategory"/> objects.</returns>
        private AliasProbability<ModuleCategory> CreateCategoryProbabilities()
        {
            var categoryList = new List<ModuleCategory>();
            var weightsList = new List<float>();
            
            foreach (var moduleElement in activeTheme.ModuleData)
            {
                var moduleCategory = moduleElement.ModuleCategory;
                
                if (moduleCategory.SpawnRequired) continue;
                
                categoryList.Add(moduleCategory);
                weightsList.Add(moduleCategory.SpawnRate);
            }
            
            return new AliasProbability<ModuleCategory>(categoryList, weightsList);
        }
        
        /// <summary>
        /// Creates alias probability tables for all modules in the theme, grouped by their category.
        /// </summary>
        /// <returns>
        /// A dictionary mapping category identifiers to <see cref="AliasProbability{TObject}"/> instances for sampling module entries within that category.
        /// </returns>
        private Dictionary<ModuleCategory, AliasProbability<ModuleAsset>> CreateCategorizedModuleProbabilities()
        {
            var categorizedModuleProbabilities = new Dictionary<ModuleCategory, AliasProbability<ModuleAsset>>();
            
            // Create alias probability lookup tables for module entries in each module category
            foreach (var moduleElement in activeTheme.ModuleData)
            {
                categorizedModuleProbabilities[moduleElement.ModuleCategory] = new AliasProbability<ModuleAsset>(
                    moduleElement.ModuleAssets,
                    moduleElement.ModuleAssets.Select(categorizedEntry => categorizedEntry.SpawnRate).ToList()
                );
            }
            
            return categorizedModuleProbabilities;
        }
        
        /// <summary>
        /// Populates the <see cref="requiredModules"/> and <see cref="uniqueModules"/> lists with modules that are marked as required for the current <see cref="activeTheme"/>.
        /// </summary>
        /// <param name="moduleLimit">
        /// The maximum number of modules that can be spawned in the dungeon.
        /// Used to prevent overpopulation.
        /// </param>
        /// <param name="categorizedModuleProbabilities">
        /// A mapping of <see cref="ModuleCategory"/> entries to weighted probabilities of their corresponding <see cref="ModuleAsset"/> instances.
        /// Used to randomly select modules.
        /// </param>
        private void InitializeRequiredModules(
            int moduleLimit,
            Dictionary<ModuleCategory, AliasProbability<ModuleAsset>> categorizedModuleProbabilities
        )
        {
            requiredModules = new List<ModuleAsset>();
            uniqueModules = new List<ModuleAsset>();

            foreach (var moduleElement in activeTheme.ModuleData)
            {
                var openSlots = moduleLimit - requiredModules.Count;
                if (openSlots <= 0)
                {
                     Debug.LogWarning($"<b>[{name}]</b> Some required modules could not be placed due to a lack of available slots.", this);
                     break;
                }
                 
                var moduleCategory = moduleElement.ModuleCategory;
                if (moduleCategory.SpawnRequired == false) continue;

                if (moduleCategory.SpawnLimits)
                {
                    var spawnCount = 0;
                    var spawnLimit = UnityEngine.Random.Range(moduleCategory.SpawnMinimum, moduleCategory.SpawnMaximum + 1);

                    if (openSlots < spawnLimit)
                    {
                        if (openSlots < moduleCategory.SpawnMinimum)
                        {
                            Debug.LogWarning($"<b>[{name}]</b> Unable to meet the minimum spawn requirement for category <b>'{moduleCategory.CategoryTitle}'</b>.", this);
                            continue;
                        }

                        spawnLimit = Math.Min(
                            openSlots,
                            moduleCategory.SpawnMaximum - restrictedCategories[moduleCategory]
                        );
                    }

                    while (spawnCount < spawnLimit)
                    {
                        var chosenModule = SampleModule(moduleCategory, categorizedModuleProbabilities);
                        if (chosenModule == null) continue;
                        
                        spawnCount++;
                        requiredModules.Add(chosenModule);
                    }
                }
                else
                {
                    var chosenModule = SampleModule(moduleCategory, categorizedModuleProbabilities);
                    if (chosenModule == null) continue;
                    
                    requiredModules.Add(chosenModule);                    
                }
            }
        }

        /// <summary>
        /// Instantiates the next required module from <see cref="requiredModules"/> and increments the active module count.
        /// </summary>
        /// <param name="moduleCount">
        /// A reference to the current count of instantiated modules.
        /// This value is incremented when a required module is successfully loaded into the dungeon.
        /// </param>
        private void InstantiateRequiredModule(ref int moduleCount)
        {
            var chosenModule = requiredModules[0];
                    
            Instantiate(chosenModule.ModulePrefab);

            moduleCount++;
            requiredModules.Remove(chosenModule);
        }

        /// <summary>
        /// Samples a <see cref="ModuleAsset"/> from the given category using alias-based probability.
        /// </summary>
        /// <param name="moduleCategory">The <see cref="ModuleCategory"/> from which to select a module from.</param>
        /// <param name="categorizedModuleProbabilities">
        /// A dictionary mapping <see cref="ModuleCategory"/> entries to their alias-probability samplers.
        /// </param>
        /// <returns>
        /// The selected <see cref="ModuleAsset"/>, or <c>null</c> if the selected module is marked as unique and has already been chosen.
        /// </returns>
        private ModuleAsset SampleModule(
            ModuleCategory moduleCategory,
            Dictionary<ModuleCategory, AliasProbability<ModuleAsset>> categorizedModuleProbabilities
        )
        {
            var chosenModule = categorizedModuleProbabilities[moduleCategory].Sample();

            if (chosenModule.SpawnOnce)
            {
                if (uniqueModules.Contains(chosenModule)) return null;
                
                uniqueModules.Add(chosenModule);
            }
            
            return chosenModule;
        }
    }
}