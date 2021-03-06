﻿#if UNITY

using System;
using System.Collections;
using System.Collections.Generic;

namespace Mond
{
    static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }

    class Tuple<T1, T2>
    {
        public T1 Item1 { get; private set; }
        public T2 Item2 { get; private set; }

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    class Lazy<T>
    {
        private readonly Func<T> _factory; 
        private bool _created;
        private T _value;

        public Lazy(Func<T> valueFactory)
        {
            _factory = valueFactory;
            _created = false;
        }

        public T Value
        {
            get
            {
                if (_created)
                    return _value;

                _value = _factory();
                _created = true;
                return _value;
            }
        }
    }

    public interface IReadOnlyCollection<T> : IEnumerable<T>
    {
        int Count { get; }
    }

    public interface IReadOnlyList<T> : IReadOnlyCollection<T>
    {
        T this[int index] { get; }
    }

    public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
    }

    public class ReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
         
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (_dictionary == null)
                throw new ArgumentNullException("dictionary");

            _dictionary = dictionary;
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
        }

        public IEnumerable<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

#endif
