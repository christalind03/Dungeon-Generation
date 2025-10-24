using Code.Scripts.Utils.SerializableDictionary;
using System;
using System.Collections.Generic;

namespace Code.Scripts.Utils
{
    /// <summary>
    /// Represents a specialized <see cref="Dictionary{TKey,TValue}"/> that tracks the frequency of occurrences for a given key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys used to index frequency counts.</typeparam>
    [Serializable]
    public class FrequencyDictionary<TKey> : SerializableDictionary<TKey, int>
    {
        /// <summary>
        /// Retrieves the total count of all frequency values across all keys, rather than the number of unique keys.
        /// </summary>
        public new int Count { get; private set; }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Additionally updates the aggregate <see cref="Count"/> value when setting elements.
        /// </summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        public new int this[TKey targetKey]
        {
            get => base[targetKey];
            set
            {
                if (TryGetValue(targetKey, out var targetValue))
                {
                    Count -= targetValue;
                }

                base[targetKey] = value;
                Count += value;
            }
        }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Recalculates the total <see cref="Count"/> value after the dictionary has been deserialized.
        /// </summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        public new void OnDeserialization(object deserializationContext)
        {
            base.OnDeserialization(deserializationContext);

            Count = 0;
            foreach (var currentItem in this)
            {
                Count += currentItem.Value;
            }
        }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Additionally updates the aggregate <see cref="Count"/> with the added value.
        /// </summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        public new void Add(KeyValuePair<TKey, int> targetItem)
        {
            Count += targetItem.Value;
            base.Add(targetItem);
        }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Additionally updates the aggregate <see cref="Count"/> with the added value.
        /// </summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        public new void Add(TKey targetKey, int targetValue)
        {
            Count += targetValue;
            base.Add(targetKey, targetValue);
        }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Additionally resets the aggregate <see cref="Count"/> to zero.
        /// </summary>
        public new void Clear()
        {
            Count = 0;
            base.Clear();
        }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Additionally subtracts its value from the aggregate <see cref="Count"/>.
        /// </summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        public new bool Remove(KeyValuePair<TKey, int> targetItem)
        {
            Count -= targetItem.Value;
            return base.Remove(targetItem);
        }

        /// <summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        /// Additionally subtracts its value from the aggregate <see cref="Count"/>.
        /// </summary>
        /// <inheritdoc cref="SerializableDictionary{TKey,TValue}"/>
        public new bool Remove(TKey targetKey)
        {
            Count -= this[targetKey];
            return base.Remove(targetKey);
        }
    }
}