using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Code.Scripts.Utils.SerializableDictionary
{
    /// <summary>
    /// The base template for serializable dictionary implementations.
    /// </summary>
    public abstract class SerializableDictionaryTemplate
    {
        /// <summary>
        /// The internal dictionary implementation used for storage and serialization.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        protected class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>
        {
            /// <summary>
            /// Initializes an empty dictionary.
            /// </summary>
            public Dictionary() { }
            
            /// <summary>
            /// Initializes the dictionary using the contents of an existing dictionary.
            /// </summary>
            /// <param name="baseDictionary">The dictionary whose elements are copied into the new instance.</param>
            public Dictionary(IDictionary<TKey, TValue> baseDictionary) : base(baseDictionary) { }
            
            /// <summary>
            /// Initializes the dictionary during deserialization.
            /// </summary>
            /// <param name="serializationInfo">The object that contains the serialized data.</param>
            /// <param name="streamingContext">The source and destination context of the serialized stream.</param>
            public Dictionary(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        }
        
        /// <summary>
        /// The base type for value cache containers used during serialization.
        /// </summary>
        public abstract class Cache { }
    }
    
    /// <summary>
    /// A serializable implementation of <see cref="Dictionary{TKey,TValue}"/> compatible with Unity's serialization system.
    /// </summary>
    /// <typeparam name="TKey">The type of key to use in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of value to use in the dictionary.</typeparam>
    /// <typeparam name="TValueCache">The cache type used to serialize and deserialize dictionary values.</typeparam>
    /// <remarks>
    /// Based on an implementation by <see href="https://github.com/JDSherbert/Unity-Serializable-Dictionary">JDSherbert on GitHub</see>.
    /// </remarks>
    [Serializable]
    public abstract class SerializableDictionaryTemplate<TKey, TValue, TValueCache> : SerializableDictionaryTemplate, IDictionary, IDictionary<TKey, TValue>, IDeserializationCallback, ISerializable, ISerializationCallbackReceiver
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
        private TValueCache[] dictionaryValues;
        
        /// <summary>
        /// The internal runtime dictionary reconstructed after deserialization.
        /// </summary>
        private Dictionary<TKey, TValue> internalDictionary;

        /// <summary>
        /// Retrieves the collection of keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)internalDictionary).Keys;
        
        /// <summary>
        /// Retrieves a non-generic collection containing the keys of the dictionary.
        /// </summary>
        ICollection IDictionary.Keys => ((IDictionary)internalDictionary).Keys;
        
        /// <summary>
        /// Retrieves the collection of values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)internalDictionary).Values;
        
        
        /// <summary>
        /// Retrieves a non-generic collection containing the values of the dictionary.
        /// </summary>
        ICollection IDictionary.Values => ((IDictionary)internalDictionary).Values;
        
        /// <summary>
        /// Retrieves the number of <see cref="KeyValuePair{TKey,TValue}"/> contained in the dictionary.
        /// </summary>
        public int Count => ((IDictionary<TKey, TValue>)internalDictionary).Count;
        
        /// <summary>
        /// Retrieves a value indicating whether the dictionary is read-only.
        /// </summary>
        public bool IsReadOnly => ((IDictionary<TKey, TValue>)internalDictionary).IsReadOnly;
        
        /// <summary>
        /// Retrieves a value indicating whether the dictionary is a fixed size.
        /// </summary>
        public bool IsFixedSize => ((IDictionary)internalDictionary).IsFixedSize;
        
        /// <summary>
        /// Retrieves a value indicating whether access to the dictionary is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized => ((IDictionary)internalDictionary).IsSynchronized;
        
        /// <summary>
        /// Retrieves an object that can be used to synchronize access to the dictionary.
        /// </summary>
        public object SyncRoot => ((IDictionary)internalDictionary).SyncRoot;
        
        /// <summary>
        /// Initializes an empty <see cref="SerializableDictionaryTemplate{TKey,TValue,TValueCache}"/>.
        /// </summary>
        protected SerializableDictionaryTemplate()
        {
            internalDictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a <see cref="SerializableDictionaryTemplate{TKey,TValue,TValueCache}"/> with the contents of an existing dictionary.
        /// </summary>
        /// <param name="dictionaryInstance"></param>
        protected SerializableDictionaryTemplate(IDictionary<TKey, TValue> dictionaryInstance)
        {
            internalDictionary = new Dictionary<TKey, TValue>(dictionaryInstance);
        }

        /// <summary>
        /// Initializes the dictionary during deserialization.
        /// </summary>
        /// <param name="serializationInfo">The object that contains the serialized data.</param>
        /// <param name="streamingContext">The source and destination context of the serialized stream.</param>
        protected SerializableDictionaryTemplate(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            internalDictionary = new Dictionary<TKey, TValue>(serializationInfo, streamingContext);
        }
        
        /// <summary>
        /// Stores a value into the value cache during serialization.
        /// </summary>
        /// <param name="itemIndex">The index associated with the key/value pair being serialized.</param>
        /// <param name="itemValue">The value to store in the cache.</param>
        /// <param name="valueCache">The cache array used to persist serialized values.</param>
        protected abstract void SetValue(int itemIndex, TValue itemValue, TValueCache[] valueCache);
        
        /// <summary>
        /// Retrieves a value from the value cache during deserialization.
        /// </summary>
        /// <param name="itemIndex">The index associated with the key/value pair being deserialized.</param>
        /// <param name="valueCache">The cache array containing serialized values.</param>
        /// <returns>The reconstructed value for the given index.</returns>
        protected abstract TValue RetrieveValue(int itemIndex, TValueCache[] valueCache);

        /// <summary>
        /// Called before Unity serializes the object.
        /// Currently unused, but required by <see cref="ISerializationCallbackReceiver"/>
        /// </summary>
        public virtual void OnBeforeSerialize() { }

        /// <summary>
        /// Called after Unity deserializes the object.
        /// Rebuilds the internal dictionary from the <see cref="dictionaryKeys"/> and <see cref="dictionaryValues"/>.
        /// </summary>
        public virtual void OnAfterDeserialize()
        {
            if (dictionaryKeys == null || dictionaryValues == null || dictionaryKeys.Length != dictionaryValues.Length) return;

            internalDictionary ??= new Dictionary<TKey, TValue>();
            internalDictionary.Clear();
            
            for (var itemIndex = 0; itemIndex < dictionaryKeys.Length; itemIndex++)
            {
                var dictionaryKey = dictionaryKeys[itemIndex];
                var dictionaryValue = RetrieveValue(itemIndex, dictionaryValues);

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
        /// Retrieves or sets the value associated with the specific key.
        /// </summary>
        /// <param name="targetKey">The key whose value to retrieve or set.</param>
        public object this[object targetKey]
        {
            get => ((IDictionary)internalDictionary)[targetKey];
            set => ((IDictionary)internalDictionary)[targetKey] = value;
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
        /// Inserts the specified value into the dictionary under the specified key.
        /// </summary>
        /// <param name="targetKey">The key of the element to insert.</param>
        /// <param name="targetValue">The value of the element to insert.</param>
        public void Add(object targetKey, object targetValue)
        {
            ((IDictionary)internalDictionary).Add(targetKey, targetValue);
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
        public bool Contains(object targetKey)
        {
            return ((IDictionary)internalDictionary).Contains(targetKey);
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
        /// Copies the elements of the dictionary to an array, starting at a particular index.
        /// </summary>
        /// <param name="targetArray">The destination array.</param>
        /// <param name="targetIndex">The zero-based index in the array at which copying begins.</param>
        public void CopyTo(Array targetArray, int targetIndex)
        {
            ((IDictionary)internalDictionary).CopyTo(targetArray, targetIndex);
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
        /// Removes the element with the specified key from the dictionary.
        /// </summary>
        /// <param name="targetKey">The key of the element to remove.</param>
        /// <returns><c>true</c> if the element was removed successfully; otherwise, <c>false</c>.</returns>
        public void Remove(object targetKey)
        {
            ((IDictionary)internalDictionary).Remove(targetKey);
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

        IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)internalDictionary).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}