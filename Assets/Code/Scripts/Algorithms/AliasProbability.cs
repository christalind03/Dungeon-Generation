using System;
using System.Collections.Generic;
using System.Linq;

namespace Code.Scripts.Algorithms
{
    /// <summary>
    /// Implements the <c>Alias Method</c> for efficient weighted random sampling.
    /// </summary>
    /// <typeparam name="TObject">The type of object to be sampled.</typeparam>
    /// <remarks>
    /// This class precomputes alias and probability tables from the provided weights, enabling O(1) random sampling regardless of the number of items.
    /// </remarks>
    public class AliasProbability<TObject>
    {
        private readonly int[] aliasTable;
        private readonly float[] probabilityTable;
        private readonly TObject[] selectionPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasProbability{TObject}"/> class.
        /// </summary>
        /// <param name="objectsList">The list of objects to sample from.</param>
        /// <param name="weightsList">The list of corresponding weights, determining selection probabilities.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the number of objects does not match the number of weights, or if the list is empty.
        /// </exception>
        public AliasProbability(IList<TObject> objectsList, IList<float> weightsList)
        {
            if (objectsList.Count != weightsList.Count)
            {
                throw new ArgumentException(
                    $"[{nameof(AliasProbability<TObject>)}] The total count of {nameof(objectsList)} ({objectsList.Count}) must match the total count of {nameof(weightsList)} ({weightsList.Count})",
                    nameof(objectsList)
                );
            }

            if (objectsList.Count <= 0)
            {
                throw new ArgumentException(
                    $"[{nameof(AliasProbability<TObject>)}] At least one object and weight must be provided to construct a probability distribution.",
                    nameof(objectsList)
                );
            }
            
            var totalItems = objectsList.Count;
            
            aliasTable = new int[totalItems];
            probabilityTable = new float[totalItems];
            selectionPool = objectsList.ToArray();
            
            // Calculate the average weight of the items in order to calculate the probability of index per element
            var totalWeight = 0f;

            for (var itemIndex = 0; itemIndex < totalItems; itemIndex++)
            {
                totalWeight += weightsList[itemIndex];
            }
            
            var averageWeight = totalWeight / totalItems;
            
            // Create additional groups for categorization during alias and probability table generation
            var overfullGroup = new Queue<int>();
            var underfullGroup = new Queue<int>();
            
            for (var itemIndex = 0; itemIndex < totalItems; itemIndex++)
            {
                probabilityTable[itemIndex] = weightsList[itemIndex] / averageWeight;
                
                ClassifyProbability(itemIndex, ref overfullGroup, ref underfullGroup);
            }
            
            // Create the alias and probability tables
            while (0 < overfullGroup.Count && 0 < underfullGroup.Count)
            {
                var overfullEntry = overfullGroup.Dequeue();
                var underfullEntry = underfullGroup.Dequeue();
                
                aliasTable[underfullEntry] = overfullEntry;
                probabilityTable[overfullEntry] = probabilityTable[overfullEntry] + probabilityTable[underfullEntry] - 1;

                ClassifyProbability(overfullEntry, ref overfullGroup, ref underfullGroup);
            }
        }

        /// <summary>
        /// Classifies the probability of an item as either <b>overfull</b> or <b>underfull</b> and enqueues it to the corresponding group.
        /// </summary>
        /// <param name="probabilityIndex">The index of the probability to be classified.</param>
        /// <param name="overfullGroup">Queue of indices with a probability greater than (>) 1.</param>
        /// <param name="underfullGroup">Queue of indices with a probability less than or equal to (≤) 1.</param>
        private void ClassifyProbability(int probabilityIndex, ref Queue<int> overfullGroup, ref Queue<int> underfullGroup)
        {
            if (1f < probabilityTable[probabilityIndex])
            {
                overfullGroup.Enqueue(probabilityIndex);
            }
            else
            {
                underfullGroup.Enqueue(probabilityIndex);
            }
        }

        /// <summary>
        /// Selects and returns a random object from the <see cref="selectionPool"/> based on the alias method tables.
        /// </summary>
        /// <returns>A randomly chosen <typeparamref name="TObject"/> weighted by its probability.</returns>
        public TObject Sample()
        {
            var randomIndex = UnityEngine.Random.Range(0, selectionPool.Length);
            var randomValue = UnityEngine.Random.value;
            
            return randomValue < probabilityTable[randomIndex] ? selectionPool[randomIndex] : selectionPool[aliasTable[randomIndex]];
        }
    }
}