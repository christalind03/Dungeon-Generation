using System.Collections;
using System.Collections.Generic;

namespace Code.Scripts.Utils
{
    /// <summary>
    /// Represents a specialized <see cref="Dictionary{TKey,TValue}"/> that tracks the frequency of occurrences for a given key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys used to index frequency counts.</typeparam>
    public class FrequencyDictionary<TKey> : IEnumerable<KeyValuePair<TKey, int>>
    {
        /// <summary>
        /// The internal storage structure mapping keys to their corresponding frequency counts.
        /// </summary>
        private readonly Dictionary<TKey, int> internalDictionary = new();
        
        /// <summary>
        /// Retrieves the total count of all frequency values across all keys, rather than the number of unique keys.
        /// </summary>
        public int Count { get; private set; }
        
        /// <summary>
        /// Retrieves a collection containing all the keys currently stored in the dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys => internalDictionary.Keys;

        /// <summary>
        /// Retrieves a collection containing all the frequency values currently stored in the dictionary.
        /// </summary>
        public IEnumerable<int> Values => internalDictionary.Values;
        
        /// <summary>
        /// Assigns or retrieves the frequency value associated with the specified key.
        /// </summary>
        /// <param name="dictKey">The key whose frequency values are get or set.</param>
        /// <remarks>
        /// When setting a value, the total <see cref="Count"/> is adjusted accordingly.
        /// </remarks>
        public int this[TKey dictKey]
        {
            get => internalDictionary.TryGetValue(dictKey, out var dictValue) ? dictValue : 0;
            set
            {
                if (internalDictionary.TryGetValue(dictKey, out var dictValue))
                {
                    Count -= dictValue;
                }
                
                internalDictionary[dictKey] = value;
                Count += value;
            }
        }

        /// <summary>
        /// Removes all key-frequency pairs from the dictionary and resets the total count to zero.
        /// </summary>
        public void Clear()
        {
            internalDictionary.Clear();
            Count = 0;
        }
        
        /// <summary>
        /// Determines whether the dictionary contains the specified key,
        /// </summary>
        /// <param name="dictKey">The key to locate in the dictionary.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(TKey dictKey) => internalDictionary.ContainsKey(dictKey);
        
        /// <summary>
        /// Increments the frequency count associated with the specified key by one.
        /// </summary>
        /// <param name="dictKey">The key whose frequency should be incremented.</param>
        public void Increment(TKey dictKey)
        {
            if (internalDictionary.TryGetValue(dictKey, out var dictValue))
            {
                internalDictionary[dictKey] = dictValue + 1;
            }
            else
            {
                internalDictionary[dictKey] = 1;
            }

            Count++;
        }

        /// <summary>
        /// Decrements the frequency count associated with the specified key by one.
        /// </summary>
        /// <param name="dictKey">The key whose frequency should be decremented.</param>
        public void Decrement(TKey dictKey)
        {
            if (internalDictionary.TryGetValue(dictKey, out var dictValue))
            {
                internalDictionary[dictKey] = dictValue - 1;
                Count--;

                if (internalDictionary[dictKey] <= 0)
                {
                    internalDictionary.Remove(dictKey);
                }
            }
        }

        /// <summary>
        /// Removes the specified key and its associated frequency count from the dictionary.
        /// </summary>
        /// <param name="dictKey">The key to remove.</param>
        /// <returns><c>true</c> if they key was found and removed; otherwise, <c>false</c>.</returns>
        public bool Remove(TKey dictKey)
        {
            if (internalDictionary.TryGetValue(dictKey, out var dictValue))
            {
                Count -= dictValue;
                return internalDictionary.Remove(dictKey);
            }

            return false;
        }
        
        /// <summary>
        /// Returns an enumerator that iterates through the collection of key-frequency pairs.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() => internalDictionary.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}