using System.Collections;
using System.Collections.Generic;

namespace Steamboat.Util
{
    internal static class ReadOnlyListProxy
    {
        public static ReadOnlyListProxy<T> From<T>(IList<T> list)
        {
            return new ReadOnlyListProxy<T>(list);
        }
    }
    
    /// <summary>
    /// A workaround for <see cref="IList{T}"/>
    /// not implementing <see cref="IReadOnlyList{T}"/>.
    /// </summary>
    internal readonly struct ReadOnlyListProxy<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _list;

        public ReadOnlyListProxy(IList<T> list)
        {
            _list = list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }

        public int Count => _list.Count;

        public T this[int index] => _list[index];
    }
}