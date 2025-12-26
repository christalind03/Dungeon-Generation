using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Code.Scripts.Utils.SerializableDictionary
{
    /// <summary>
    /// Provides shared cache-related types for serializable dictionary implementations.
    /// </summary>
    public abstract class SerializableDictionary
    {
        /// <summary>
        /// The base cache container used to serialize and deserialize dictionary values.
        /// </summary>
        /// <typeparam name="TObject">The runtime value type being cached.</typeparam>
        public abstract class Cache<TObject> : SerializableDictionaryTemplate.Cache
        {
            public TObject CacheData;
        }
    }

    /// <summary>
    /// A serializable implementation of <see cref="Dictionary{TKey,TValue}"/> compatible with Unity's serialization system.
    /// </summary>
    /// <typeparam name="TKey">The type of key to use in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of value to use in the dictionary.</typeparam>
    /// <remarks>
    /// Based on an implementation by <see href="https://github.com/JDSherbert/Unity-Serializable-Dictionary">JDSherbert on GitHub</see>.
    /// </remarks>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionaryTemplate<TKey, TValue, TValue>
    {
        /// <summary>
        /// Initializes an empty dictionary.
        /// </summary>
        public SerializableDictionary() { }
        
        /// <summary>
        /// Initializes the dictionary using the contents of an existing dictionary.
        /// </summary>
        /// <param name="baseDictionary">The dictionary whose elements are copied into the new instance.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> baseDictionary) : base(baseDictionary) { }
        
        /// <summary>
        /// Initializes the dictionary during deserialization.
        /// </summary>
        /// <param name="serializationInfo">The object that contains the serialized data.</param>
        /// <param name="streamingContext">The source and destination context of the serialized stream.</param>
        protected SerializableDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        
        /// <summary>
        /// Stores a value into the value cache during serialization.
        /// </summary>
        /// <param name="itemIndex">The index associated with the key/value pair being serialized.</param>
        /// <param name="itemValue">The value to store in the cache.</param>
        /// <param name="valueCache">The cache array used to persist serialized values.</param>
        protected override void SetValue(int itemIndex, TValue itemValue, TValue[] valueCache)
        {
            valueCache[itemIndex] = itemValue;
        }

        /// <summary>
        /// Retrieves a value from the value cache during deserialization.
        /// </summary>
        /// <param name="itemIndex">The index associated with the key/value pair being deserialized.</param>
        /// <param name="valueCache">The cache array containing serialized values.</param>
        /// <returns>The reconstructed value for the given index.</returns>
        protected override TValue RetrieveValue(int itemIndex, TValue[] valueCache)
        {
            return valueCache[itemIndex];
        }
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
    public class SerializableDictionary<TKey, TValue, TValueCache> : SerializableDictionaryTemplate<TKey, TValue, TValueCache> where TValueCache : SerializableDictionary.Cache<TValue>, new()
    {
        /// <summary>
        /// Initializes an empty dictionary.
        /// </summary>
        public SerializableDictionary() { }
        
        /// <summary>
        /// Initializes the dictionary using the contents of an existing dictionary.
        /// </summary>
        /// <param name="baseDictionary">The dictionary whose elements are copied into the new instance.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> baseDictionary) : base(baseDictionary) { }
        
        /// <summary>
        /// Initializes the dictionary during deserialization.
        /// </summary>
        /// <param name="serializationInfo">The object that contains the serialized data.</param>
        /// <param name="streamingContext">The source and destination context of the serialized stream.</param>
        protected SerializableDictionary(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        
        /// <summary>
        /// Stores a value into the value cache during serialization.
        /// </summary>
        /// <param name="itemIndex">The index associated with the key/value pair being serialized.</param>
        /// <param name="itemValue">The value to store in the cache.</param>
        /// <param name="valueCache">The cache array used to persist serialized values.</param>
        protected override void SetValue(int itemIndex, TValue itemValue, TValueCache[] valueCache)
        {
            valueCache[itemIndex] = new TValueCache
            {
                CacheData = itemValue
            };
        }

        /// <summary>
        /// Retrieves a value from the value cache during deserialization.
        /// </summary>
        /// <param name="itemIndex">The index associated with the key/value pair being deserialized.</param>
        /// <param name="valueCache">The cache array containing serialized values.</param>
        /// <returns>The reconstructed value for the given index.</returns>
        protected override TValue RetrieveValue(int itemIndex, TValueCache[] valueCache)
        {
            return valueCache[itemIndex].CacheData;
        }
    }
}