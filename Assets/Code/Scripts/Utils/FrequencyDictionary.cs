using System.Collections;
using System.Collections.Generic;

namespace Code.Scripts.Utils
{
    public class FrequencyDictionary<TKey> : IEnumerable<KeyValuePair<TKey, int>>
    {
        // TODO: Rename this later...
        private readonly Dictionary<TKey, int> internalDict = new();
        
        public int Count { get; private set; }
        
        public int this[TKey dictKey]
        {
            get => internalDict.TryGetValue(dictKey, out var dictValue) ? dictValue : 0;
            set
            {
                if (internalDict.TryGetValue(dictKey, out var dictValue))
                {
                    Count -= dictValue;
                }
                
                internalDict[dictKey] = value;
                Count += value;
            }
        }

        public void Clear()
        {
            internalDict.Clear();
            Count = 0;
        }
        
        public bool ContainsKey(TKey dictKey) => internalDict.ContainsKey(dictKey);
        
        public void Increment(TKey dictKey)
        {
            if (internalDict.TryGetValue(dictKey, out var dictValue))
            {
                internalDict[dictKey] = dictValue + 1;
            }
            else
            {
                internalDict[dictKey] = 1;
            }

            Count++;
        }

        public void Decrement(TKey dictKey)
        {
            if (internalDict.TryGetValue(dictKey, out var dictValue))
            {
                internalDict[dictKey] = dictValue - 1;
                Count--;

                if (internalDict[dictKey] <= 0)
                {
                    internalDict.Remove(dictKey);
                }
            }
        }

        public bool Remove(TKey dictKey)
        {
            if (internalDict.TryGetValue(dictKey, out var dictValue))
            {
                Count -= dictValue;
                return internalDict.Remove(dictKey);
            }

            return false;
        }
        
        public IEnumerable<TKey> Keys => internalDict.Keys;

        public IEnumerable<int> Values => internalDict.Values;
        
        public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator() => internalDict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}