using Code.Scripts.Algorithms;
using Code.Scripts.Dungeon.Data;
using Code.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Scripts.Dungeon.Behaviours
{
    /// <summary>
    /// Procedurally constructs a dungeon layout at runtime using a selected <see cref="DungeonTheme"/>.
    /// This generator selects module categories and modules based on weighted probabilities and spawns them into the scene to create a randomized, replayable dungeon experience.
    /// </summary>
    public class DungeonGenerator : MonoBehaviour
    {
        private const int BacktrackLimit = 100;
        private const float RequiredChance = 0.5f;

        [SerializeField]
        [Tooltip("References to the available dungeon theme assets used by this dungeon.")]
        private DungeonTheme[] availableThemes;
        
        [SerializeField]
        [Tooltip("Specifies which layers are considered when detecting overlaps with existing modules.")]
        private LayerMask placementLayers;
        
        /// <summary>
        /// The currently selected <see cref="DungeonTheme"/> for generation.
        /// </summary>
        private DungeonTheme activeDungeonTheme;

        /// <summary>
        /// Tracks the number of backtracking attempts made during generation.
        /// </summary>
        private int backtrackAttempts;
        
        /// <summary>
        /// Represents the error encountered during the generation process.
        /// </summary>
        private GenerationError generationError;
        
        /// <summary>
        /// The current number of successfully instantiated modules.
        /// </summary>
        private int moduleCount;
        
        /// <summary>
        /// The maximum number of modules allowed in the current generation pass.
        /// </summary>
        private int moduleLimit;
        
        /// <summary>
        /// A stack storing the historical sequence of module placements during generation.
        /// </summary>
        private Stack<HistoryEntry> moduleHistory;
        
        /// <summary>
        /// A collection of currently active <see cref="DungeonModule"/> that contain at least one open connection point.
        /// </summary>
        private List<DungeonModule> connectableModules;

        /// <summary>
        /// The alias probability table for sampling <see cref="ModuleCategory"/> values in constant time.
        /// </summary>
        private AliasProbability<ModuleCategory> categoryProbabilities;
        
        /// <summary>
        /// The alias probability tables for each <see cref="ModuleCategory"/> used to sample associated <see cref="ModuleAsset"/> instances.
        /// </summary>
        private Dictionary<ModuleCategory, AliasProbability<ModuleAsset>> moduleProbabilities;

        /// <summary>
        /// Tracks how many times each <see cref="ModuleCategory"/> has been spawned during generation.
        /// </summary>
        private FrequencyDictionary<ModuleCategory> categorizedModules;

        /// <summary>
        /// A frequency-tracked collection of required <see cref="ModuleAsset"/> and <see cref="ModuleCategory"/> pairs.
        /// </summary>
        private FrequencyDictionary<(ModuleAsset, ModuleCategory)> requiredModules;
        
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
        /// <item><description>Picking a random <see cref="DungeonTheme"/> from the available themes</description></item>
        /// <item><description>Setting up probability tables for the weighted category and module selection</description></item>
        /// <item><description>Spawning modules until the target count is reached, respecting required category constraints</description></item>
        /// </list>
        /// </summary>
        private void Generate()
        {
            InitializeGeneration();

            while (generationError == GenerationError.None && moduleCount < moduleLimit)
            {
                var openSlots = moduleLimit - moduleCount;
                if (openSlots < requiredModules.Count)
                {
                    generationError = GenerationError.InsufficientSpace;
                    break;
                }

                if (openSlots == requiredModules.Count)
                {
                    InstantiateRequiredModule();
                    continue;
                }

                if (0 < requiredModules.Count)
                {
                    var requiredUrgency = Mathf.Clamp01((float)requiredModules.Count / openSlots);
                    var requiredChance = Mathf.Lerp(RequiredChance, 1f, requiredUrgency);
                    var randomChance = UnityEngine.Random.value;

                    if (randomChance <= requiredChance)
                    {
                        InstantiateRequiredModule();
                        continue;
                    }
                }

                var chosenCategory = categoryProbabilities.Sample();
                if (chosenCategory.SpawnLimits)
                {
                    if (chosenCategory.SpawnMaximum <= categorizedModules[chosenCategory]) continue;

                    var categoryCount = 0;
                    var categoryLimit = UnityEngine.Random.Range(chosenCategory.SpawnMinimum, chosenCategory.SpawnMaximum + 1);

                    if (openSlots < categoryLimit)
                    {
                        categoryLimit = Math.Min(
                            openSlots,
                            chosenCategory.SpawnMaximum - categorizedModules[chosenCategory]
                        );
                    }

                    var anticipatedCount = categorizedModules[chosenCategory] + categoryLimit;
                    if (anticipatedCount > chosenCategory.SpawnMaximum) continue;

                    while (generationError == GenerationError.None && categoryCount < categoryLimit)
                    {
                        if (!InstantiateWeightedModule(chosenCategory)) continue;
                        categoryCount++;
                    }
                }
                else
                {
                    InstantiateWeightedModule(chosenCategory);
                }
            }

            #if UNITY_EDITOR

            if (generationError != GenerationError.None)
            {
                var errorMessage = generationError switch
                {
                    GenerationError.InsufficientSpace => "Insufficient space for module placement.",
                    GenerationError.InsufficientRequiredSpace => "Insufficient space to place all required modules.",
                    GenerationError.InvalidHistory => "No generation history available for backtracking.",
                    GenerationError.InvalidBacktracking => "Backtracking limit exceeded.",
                    _ => "Unknown Error"
                };

                Debug.LogError($"<b>[{name}]</b> Generation Failed: {errorMessage}");
            }

            #endif

            // NOTE: This is for debugging purposes only...
            Debug.Log($"Spawned: {moduleCount}/{moduleLimit}, Backtracked: {backtrackAttempts}");
            
            foreach (var connectableModule in connectableModules)
            {
                connectableModule.ToggleEntrances(false);                
            }
        }
        
        /// <summary>
        /// Initializes all data structures, probability tables, and variables required to begin generation.
        /// </summary>
        /// <remarks>
        /// It must be called before any generation logic to ensure all dependencies are properly initialized and cleared.
        /// </remarks>
        private void InitializeGeneration()
        {
            activeDungeonTheme = availableThemes[UnityEngine.Random.Range(0, availableThemes.Length)];

            backtrackAttempts = 0;
            generationError = GenerationError.None;

            moduleCount = 0;
            moduleLimit = UnityEngine.Random.Range(activeDungeonTheme.MinimumModules, activeDungeonTheme.MaximumModules + 1);
            moduleHistory = new Stack<HistoryEntry>();
            connectableModules = new List<DungeonModule>();

            // Create alias probability lookup tables for drawing random values in O(1) time
            InitializeCategoryProbabilities();
            InitializeCategorizedModuleProbabilities();

            categorizedModules = new FrequencyDictionary<ModuleCategory>();

            foreach (var moduleElement in activeDungeonTheme.ModuleData)
            {
                categorizedModules[moduleElement.ModuleCategory] = 0;
            }

            InitializeRequiredModules();
        }

        /// <summary>
        /// Creates an <see cref="AliasProbability{TObject}"/> table for all non-required module categories.
        /// </summary>
        /// <returns>An alias probability lookup table for selecting <see cref="ModuleCategory"/> objects.</returns>
        private void InitializeCategoryProbabilities()
        {
            var categoryList = new List<ModuleCategory>();
            var weightsList = new List<float>();

            foreach (var moduleElement in activeDungeonTheme.ModuleData)
            {
                var moduleCategory = moduleElement.ModuleCategory;
                var moduleWeight = moduleCategory.SpawnRate;

                if (moduleCategory.SpawnRequired) continue;

                categoryList.Add(moduleCategory);
                weightsList.Add(moduleWeight);
            }

            categoryProbabilities = new AliasProbability<ModuleCategory>(categoryList, weightsList);
        }

        /// <summary>
        /// Creates alias probability tables for all modules in the theme, grouped by their category.
        /// </summary>
        /// <returns>
        /// A dictionary mapping <see cref="ModuleCategory"/> to <see cref="AliasProbability{TObject}"/> instances for sampling module entries within that category.
        /// </returns>
        private void InitializeCategorizedModuleProbabilities()
        {
            moduleProbabilities = new Dictionary<ModuleCategory, AliasProbability<ModuleAsset>>();

            // Create alias probability lookup tables for module entries in each module category
            foreach (var moduleElement in activeDungeonTheme.ModuleData)
            {
                moduleProbabilities[moduleElement.ModuleCategory] = new AliasProbability<ModuleAsset>(
                    moduleElement.ModuleAssets,
                    moduleElement.ModuleAssets.Select(categorizedEntry => categorizedEntry.SpawnRate).ToList()
                );
            }
        }

        /// <summary>
        /// Populates the <see cref="requiredModules"/> and <see cref="uniqueModules"/> lists with modules that are marked as required for the current <see cref="activeDungeonTheme"/>.
        /// </summary>
        private void InitializeRequiredModules()
        {
            requiredModules = new FrequencyDictionary<(ModuleAsset, ModuleCategory)>();
            uniqueModules = new List<ModuleAsset>();

            foreach (var moduleElement in activeDungeonTheme.ModuleData)
            {
                var openSlots = moduleLimit - requiredModules.Count;
                if (openSlots <= 0)
                {
                    generationError = GenerationError.InsufficientRequiredSpace;
                    break;
                }

                var moduleCategory = moduleElement.ModuleCategory;
                if (moduleCategory.SpawnRequired == false) continue;

                // TODO: Ensure enough room exists to meet all required module spawn constraints
                if (moduleCategory.SpawnLimits)
                {
                    var spawnCount = 0;
                    var spawnLimit = UnityEngine.Random.Range(moduleCategory.SpawnMinimum, moduleCategory.SpawnMaximum + 1);

                    if (openSlots < spawnLimit)
                    {
                        if (openSlots < moduleCategory.SpawnMinimum)
                        {
                            generationError = GenerationError.InsufficientRequiredSpace;
                            break;
                        }

                        spawnLimit = Math.Min(
                            openSlots,
                            moduleCategory.SpawnMaximum - categorizedModules[moduleCategory]
                        );
                    }

                    while (spawnCount < spawnLimit)
                    {
                        var moduleAsset = SampleWeightedModule(moduleCategory);
                        if (moduleAsset == null) continue;

                        requiredModules.Increment((moduleAsset, moduleCategory));
                        spawnCount++;
                    }
                }
                else
                {
                    var moduleAsset = SampleWeightedModule(moduleCategory);
                    if (moduleAsset == null) continue;

                    requiredModules.Increment((moduleAsset, moduleCategory));
                }
            }
        }
        
        /// <summary>
        /// Attempts to instantiate a required module from the remaining <see cref="requiredModules"/> pool.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a required module was successfully instantiated; otherwise, <c>false</c>.
        /// </returns>
        private bool InstantiateRequiredModule()
        {
            var requiredModule = SampleRequiredModule();
            if (requiredModule is null) return false;

            var (moduleAsset, moduleCategory) = requiredModule.Value;

            return TryInstantiatingModule(moduleAsset, moduleCategory, true);
        }

        /// <summary>
        /// Attempts to instantiate a non-required module from the provided weighted category pool.
        /// </summary>
        /// <param name="moduleCategory">The <see cref="ModuleCategory"/> from which to sample a weighted module.</param>
        /// <returns>
        /// <c>true</c> if a weighted module was successfully instantiated; otherwise, <c>false</c>.
        /// </returns>
        private bool InstantiateWeightedModule(ModuleCategory moduleCategory)
        {
            var moduleAsset = SampleWeightedModule(moduleCategory);
            if (moduleAsset == null) return false;

            return TryInstantiatingModule(moduleAsset, moduleCategory, false);
        }

        /// <summary>
        /// Randomly selects a required module from the remaining pool of required modules.
        /// </summary>
        /// <returns>
        /// A tuple containing the <see cref="ModuleAsset"/> and its <see cref="ModuleCategory"/> if available; otherwise, <c>null</c>.
        /// </returns>
        private (ModuleAsset, ModuleCategory)? SampleRequiredModule()
        {
            var targetIndex = UnityEngine.Random.Range(0, requiredModules.Keys.Count());
            var targetPair = requiredModules.ElementAt(targetIndex);

            return 0 < targetPair.Value ? targetPair.Key : null;
        }

        /// <summary>
        /// Samples a module from the alias probability table for the specified category.
        /// </summary>
        /// <param name="moduleCategory">The <see cref="ModuleCategory"/> from which to sample the module.</param>
        /// <returns>
        /// The sampled <see cref="ModuleAsset"/> if valid; otherwise, <c>null</c> if the asset cannot be used.
        /// </returns>
        private ModuleAsset SampleWeightedModule(ModuleCategory moduleCategory)
        {
            var chosenModule = moduleProbabilities[moduleCategory].Sample();
            if (chosenModule.SpawnOnce)
            {
                if (uniqueModules.Contains(chosenModule)) return null;
                uniqueModules.Add(chosenModule);
            }

            return chosenModule;
        }

        /// <summary>
        /// Attempts to instantiate and position a module in the current dungeon layout.
        /// </summary>
        /// <param name="moduleAsset">The <see cref="ModuleAsset"/> to instantiate.</param>
        /// <param name="moduleCategory">The <see cref="ModuleCategory"/> associated with the module.</param>
        /// <param name="isRequired">Indicates whether the module is part of the required set.</param>
        /// <returns>
        /// <c>true</c> if the module was successfully instantiated and placed; otherwise, <c>false</c>.
        /// </returns>
        private bool TryInstantiatingModule(ModuleAsset moduleAsset, ModuleCategory moduleCategory, bool isRequired)
        {
            if (0 < moduleCount && connectableModules.Count <= 0)
            {
                return TryBacktracking();
            }

            var moduleInstance = Instantiate(moduleAsset.ModulePrefab, transform);

            var instantiatedModule = moduleInstance.GetComponent<DungeonModule>();
            var instantiatedEntrance = instantiatedModule.SelectConnectableEntrance();

            if (connectableModules.Count <= 0)
            {
                connectableModules.Add(instantiatedModule);

                RecordModule(new HistoryEntry(moduleAsset, moduleCategory, null, moduleInstance), isRequired);
                return true;
            }

            var connectedModule = connectableModules[UnityEngine.Random.Range(0, connectableModules.Count)];
            var connectedEntrance = connectedModule.SelectConnectableEntrance();

            AlignModules(moduleInstance.transform, instantiatedEntrance.EntrancePoint, connectedEntrance.EntrancePoint);

            if (ContainsIntersections(instantiatedModule.ModuleBounds))
            {
                Destroy(moduleInstance);
                return false;
            }

            instantiatedModule.RemoveConnectableEntrance(instantiatedEntrance);
            connectedModule.RemoveConnectableEntrance(connectedEntrance);

            RefreshConnections(instantiatedModule);
            RefreshConnections(connectedModule);

            RecordModule(new HistoryEntry(moduleAsset, moduleCategory, connectedEntrance, moduleInstance), isRequired);
            return true;
        }

        /// <summary>
        /// Aligns a module's transform to connect its entrance with another module's entrance.
        /// </summary>
        /// <param name="instantiatedObject">The <see cref="Transform"/> of the module being positioned.</param>
        /// <param name="instantiatedEntrance">The <see cref="Transform"/> of the entrance point on the instantiated module.</param>
        /// <param name="connectableEntrance">The <see cref="Transform"/> of the entrance point on the existing module.</param>
        private static void AlignModules(Transform instantiatedObject, Transform instantiatedEntrance, Transform connectableEntrance)
        {
            var rotationOffset = Mathf.DeltaAngle(instantiatedEntrance.eulerAngles.y, connectableEntrance.eulerAngles.y) + 180f;

            instantiatedObject.Rotate(0, rotationOffset, 0);
            instantiatedObject.position = instantiatedObject.position - instantiatedEntrance.position + connectableEntrance.position;

            Physics.SyncTransforms();
        }

        /// <summary>
        /// Determines whether the provided module bounds intersect with any existing geometry.
        /// </summary>
        /// <param name="targetBounds">The <see cref="Collider"/> bounds of the module being tested.</param>
        /// <returns>
        /// <c>true</c> if the module's colliders overlap with any other colliders in the <see cref="placementLayers"/>; otherwise, <c>false</c>.
        /// </returns>
        private bool ContainsIntersections(Collider[] targetBounds)
        {
            var containsInteractions = false;

            foreach (var moduleBounds in targetBounds)
            {
                var hitContacts = Physics.OverlapBox(moduleBounds.bounds.center, moduleBounds.bounds.extents, Quaternion.identity, placementLayers);
                if (hitContacts.All(hitContact => hitContact.transform.root == moduleBounds.transform.root)) continue;

                containsInteractions = true;
                break;
            }

            return containsInteractions;
        }

        /// <summary>
        /// Updates the connection list to include or exclude a <see cref="DungeonModule"/> based on its available entrances.
        /// </summary>
        /// <param name="targetModule">The <see cref="DungeonModule"/> whose connection state is being updated.</param>
        private void RefreshConnections(DungeonModule targetModule)
        {
            if (targetModule.ContainsConnectableEntrance())
            {
                if (connectableModules.Contains(targetModule)) return;
                connectableModules.Add(targetModule);
            }
            else
            {
                connectableModules.Remove(targetModule);
            }
        }

        /// <summary>
        /// Records the result of a successful module instantiation to the <see cref="moduleHistory"/>.
        /// </summary>
        /// <param name="historyEntry">The <see cref="HistoryEntry"/> describing the spawned module.</param>
        /// <param name="isRequired">Indicates whether the module was a required one.</param>
        private void RecordModule(HistoryEntry historyEntry, bool isRequired)
        {
            moduleCount++;
            moduleHistory.Push(historyEntry);

            if (isRequired)
            {
                requiredModules.Decrement((historyEntry.Asset, historyEntry.Category));
            }

            categorizedModules.Increment(historyEntry.Category);
        }

        /// <summary>
        /// Attempts to backtrack the generation process by removing the most recently placed module.
        /// </summary>
        /// <returns>
        /// <c>true</c> if backtracking succeeded; otherwise, <c>false</c>.
        /// </returns>
        private bool TryBacktracking()
        {
            if (moduleHistory.Count <= 0)
            {
                generationError = GenerationError.InvalidHistory;
                return false;
            }

            if (BacktrackLimit <= backtrackAttempts)
            {
                generationError = GenerationError.InvalidBacktracking;
                return false;
            }

            var historyEntry = moduleHistory.Pop();

            moduleCount--;
            categorizedModules.Decrement(historyEntry.Category);
            
            var dungeonLink = historyEntry.Entrance;
            if (dungeonLink is not null)
            {
                var connectedModule = dungeonLink.DungeonModule;
                if (connectedModule is not null)
                {
                    connectedModule.RegisterConnectableEntrance(dungeonLink);
                }
            }
            
            if (requiredModules.ContainsKey((historyEntry.Asset, historyEntry.Category)))
            {
                requiredModules.Increment((historyEntry.Asset, historyEntry.Category));
            }

            Destroy(historyEntry.Instance);

            backtrackAttempts++;
            return true;
        }

        /// <summary>
        /// Represents the possible errors that can occur during dungeon generation.
        /// </summary>
        private enum GenerationError
        {
            None = 0,
            InsufficientSpace = 101,
            InsufficientRequiredSpace = 201,
            InvalidHistory = 301,
            InvalidBacktracking = 302,
            UnknownError = 999
        }

        /// <summary>
        /// Represents a single step in the history for the <see cref="DungeonGenerator"/> used for backtracking or undoing module placements.
        /// </summary>
        private readonly struct HistoryEntry
        {
            public readonly ModuleAsset Asset;
            public readonly ModuleCategory Category;
            public readonly DungeonLink Entrance;
            public readonly GameObject Instance;

            public HistoryEntry(ModuleAsset moduleAsset, ModuleCategory moduleCategory, DungeonLink dungeonLink, GameObject objectInstance)
            {
                Asset = moduleAsset;
                Category = moduleCategory;
                Entrance = dungeonLink;
                Instance = objectInstance;
            }
        }
    }
}