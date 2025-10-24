using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Code.Scripts.Utils.SerializableDictionary
{
    /// <summary>
    /// A serializable implementation of <see cref="Dictionary{TKey,TValue}"/> compatible with Unity's serialization system.
    /// </summary>
    /// <typeparam name="TKey">The type of key to use in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of value to use in the dictionary.</typeparam>
    /// <remarks>
    /// Based on an implementation by <see href="https://github.com/JDSherbert/Unity-Serializable-Dictionary">JDSherbert on GitHub</see>.
    /// </remarks>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDeserializationCallback, ISerializable, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The serialized array of dictionary keys. 
        /// </summary>
        [SerializeField]
        private TKey[] dictionaryKeys;
        
        /// <summary>
        /// The serialized array of dictionary values.
        /// </summary>
        [SerializeField]
        private TValue[] dictionaryValues;
        
        /// <summary>
        /// The internal runtime dictionary reconstructed after deserialization.
        /// </summary>
        private Dictionary<TKey, TValue> internalDictionary;

        /// <summary>
        /// Retrieves the collection of keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)internalDictionary).Keys;
        
        /// <summary>
        /// Retrieves the collection of values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)internalDictionary).Values;
        
        /// <summary>
        /// Retrieves the number of <see cref="KeyValuePair{TKey,TValue}"/> contained in the dictionary.
        /// </summary>
        public int Count => ((IDictionary<TKey, TValue>)internalDictionary).Count;
        
        /// <summary>
        /// Retrieves a value indicating whether the dictionary is read-only.
        /// </summary>
        public bool IsReadOnly => ((IDictionary<TKey, TValue>)internalDictionary).IsReadOnly;
        
        /// <summary>
        /// Initializes an empty <see cref="SerializableDictionary{TKey,TValue}"/>.
        /// </summary>
        public SerializableDictionary()
        {
            internalDictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a <see cref="SerializableDictionary{TKey,TValue}"/> with the contents of an existing dictionary.
        /// </summary>
        /// <param name="dictionaryInstance"></param>
        public SerializableDictionary(IDictionary<TKey, TValue> dictionaryInstance)
        {
            internalDictionary = new Dictionary<TKey, TValue>(dictionaryInstance);
        }
        
        /// <summary>
        /// Called before Unity serializes the object.
        /// Currently unused, but required by <see cref="ISerializationCallbackReceiver"/>
        /// </summary>
        public void OnBeforeSerialize() { }

        /// <summary>
        /// Called after Unity deserializes the object.
        /// Rebuilds the internal dictionary from the <see cref="dictionaryKeys"/> and <see cref="dictionaryValues"/>.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (dictionaryKeys == null || dictionaryValues == null || dictionaryKeys.Length != dictionaryValues.Length) return;

            internalDictionary ??= new Dictionary<TKey, TValue>();
            internalDictionary.Clear();
            
            for (var itemIndex = 0; itemIndex < dictionaryKeys.Length; itemIndex++)
            {
                var dictionaryKey = dictionaryKeys[itemIndex];
                var dictionaryValue = dictionaryValues[itemIndex];

                if (dictionaryKey == null) continue;

                internalDictionary[dictionaryKey] = dictionaryValue;
            }
        }
        
        /// <summary>
        /// Populates serialization data with dictionary contents.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="streamingContext">The destination context for the serialization.</param>
        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            ((ISerializable)internalDictionary).GetObjectData(serializationInfo, streamingContext);
        }
        
        /// <summary>
        /// Called when deserialization of the object graph is complete.
        /// </summary>
        /// <param name="deserializationContext">The source of the deserialization event.</param>
        public void OnDeserialization(object deserializationContext)
        {
            ((IDeserializationCallback)internalDictionary).OnDeserialization(deserializationContext);
        }
        
        /// <summary>
        /// Retrieves or sets the value associated with the specified key.
        /// </summary>
        /// <param name="targetKey">The key whose value is retrieved or set.</param>
        public TValue this[TKey targetKey]
        {
            get => ((IDictionary<TKey, TValue>)internalDictionary)[targetKey];
            set => ((IDictionary<TKey, TValue>)internalDictionary)[targetKey] = value;
        }
        
        /// <summary>
        /// Inserts the specified <see cref="KeyValuePair{TKey,TValue}"/> to the dictionary.
        /// </summary>
        /// <param name="targetItem">The <see cref="KeyValuePair{TKey,TValue}"/> to insert.</param>
        public void Add(KeyValuePair<TKey, TValue> targetItem)
        {
            ((IDictionary<TKey, TValue>)internalDictionary).Add(targetItem);
        }
        
        /// <summary>
        /// Inserts the specified value into the dictionary under the specified key.
        /// </summary>
        /// <param name="targetKey">The key of the element to insert.</param>
        /// <param name="targetValue">The value of the element to insert.</param>
        public void Add(TKey targetKey, TValue targetValue)
        {
            ((IDictionary<TKey, TValue>)internalDictionary).Add(targetKey, targetValue);
        }

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public void Clear()
        {
            ((IDictionary<TKey, TValue>)internalDictionary).Clear();
        }

        /// <summary>
        /// Determines whether the dictionary contains a specific <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        /// <param name="targetItem">The <see cref="KeyValuePair{TKey,TValue}"/> to locate.</param>
        /// <returns><c>true</c> if the pair exists; otherwise, <c>false</c>.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> targetItem)
        {
            return ((IDictionary<TKey, TValue>)internalDictionary).Contains(targetItem);
        }
        
        /// <summary>
        /// Determines whether the dictionary contains a specific key.
        /// </summary>
        /// <param name="targetKey">The key to locate.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(TKey targetKey)
        {
            return ((IDictionary<TKey, TValue>)internalDictionary).ContainsKey(targetKey);
        }

        /// <summary>
        /// Copies the elements of the dictionary to an array, starting at a particular index.
        /// </summary>
        /// <param name="targetArray">The destination array.</param>
        /// <param name="targetIndex">The zero-based index in the array at which copying begins.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] targetArray, int targetIndex)
        {
            ((IDictionary<TKey, TValue>)internalDictionary).CopyTo(targetArray, targetIndex);
        }

        /// <summary>
        /// Removes the element with the specified <see cref="KeyValuePair{TKey,TValue}"/> from the dictionary.
        /// </summary>
        /// <param name="targetItem">The <see cref="KeyValuePair{TKey,TValue}"/> of the element to remove.</param>
        /// <returns><c>true</c> if the element was removed successfully; otherwise, <c>false</c>.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> targetItem)
        {
            return ((IDictionary<TKey, TValue>)internalDictionary).Remove(targetItem);
        }
        
        /// <summary>
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="targetKey">The key of the element to remove.</param>
        /// <returns><c>true</c> if the element was removed successfully; otherwise, <c>false</c>.</returns>
        public bool Remove(TKey targetKey)
        {
            return ((IDictionary<TKey, TValue>)internalDictionary).Remove(targetKey);
        }
        
        /// <summary>
        /// Retrieves the value associated with the specified key.
        /// </summary>
        /// <param name="targetKey">The key of the value to retrieve.</param>
        /// <param name="targetValue">The value associated with the specified key, if found.</param>
        /// <returns><c>true</c> if the dictionary contains an element with the specified key; otherwise <c>false</c>.</returns>
        public bool TryGetValue(TKey targetKey, out TValue targetValue)
        {
            return ((IDictionary<TKey, TValue>)internalDictionary).TryGetValue(targetKey, out targetValue);
        }
        
        /// <summary>
        /// Returns an enumerator that iterates through the collection of key-frequency pairs.
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => internalDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}