using System;
using UnityEngine;

namespace Code.Scripts.Utils.SerializableDictionary
{
    /// <summary>
    /// Defines custom header labels for a <see cref="SerializableDictionary{TKey,TValue}"/> field in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializableDictionaryHeader : PropertyAttribute
    {
        /// <summary>
        /// The display label for the key column in the Inspector.
        /// </summary>
        public readonly string KeyLabel;
        
        /// <summary>
        /// The display label for the value column in the Inspector.
        /// </summary>
        public readonly string ValueLabel;

        /// <summary>
        /// Creates a new <see cref="SerializableDictionaryHeader"/> that defines custom display labels for the key and value columns in the Inspector.
        /// </summary>
        /// <param name="keyLabel">The label to display above the key column.</param>
        /// <param name="valueLabel">The label to display about the value column.</param>
        public SerializableDictionaryHeader(string keyLabel, string valueLabel)
        {
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
        }
    }
}