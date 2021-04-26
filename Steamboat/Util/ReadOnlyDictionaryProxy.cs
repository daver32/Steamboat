using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Steamboat.Util
{
    internal static class ReadOnlyDictionaryProxy
    {
        public static ReadOnlyDictionaryProxy<TKey, TValue> From<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionaryProxy<TKey, TValue>(dictionary);
        }
    }
    
    /// <summary>
    /// A workaround for <see cref="IDictionary{TKey,TValue}"/>
    /// not implementing <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
    /// </summary>
    internal readonly struct ReadOnlyDictionaryProxy<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public ReadOnlyDictionaryProxy(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key] => _dictionary[key];

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values;
    }
}