// This is a template class that is a wrapper for a dictionary-within-a-dictionary
// where both dictionaries use the same key.
// It's useful because it has an accessor function that lets you assign values without
// having to create the second dimension first.
//
// For example, in a standard dictionary-of-dictionaries, if you want to assign an item
// to a slot that doesn't currently exists, you can't just write:
//   dict["uno"]["dos"] = someValue;
// Instead, you must do something like this:
//   if (!dict.ContainsKey("uno")) { dict.Add("uno", new Dictionary<string, ValueType>()); }
//   dict["uno"]["dos"] = someValue;
// But if you use DoubleDictionary, you can write:
//   dict["uno", "dos"] = someValue;
// without having to worry about creating the inside dictionary.
//
// It has enumerators such that you can still use nested foreach loops.
//--------------------------------------------------------------------------------------------------//

using System.Collections;
using System.Collections.Generic;

namespace AnimCooker
{

    public class DoubleDictionary<TKey, TValue> : IEnumerable
    {
        Dictionary<TKey, Dictionary<TKey, TValue>> m_dict = new Dictionary<TKey, Dictionary<TKey, TValue>>();

        public TValue this[TKey key1, TKey key2]
        {
            get {
                CheckKeys(key1, key2);
                return m_dict[key1][key2];
            }
            set {
                CheckKeys(key1, key2);
                m_dict[key1][key2] = value;
            }
        }

        public Dictionary<TKey, TValue> this[TKey key1]
        {
            get {
                CheckKey(key1);
                return m_dict[key1];
            }
            set {
                CheckKey(key1);
                m_dict[key1] = value;
            }
        }

        public bool TryGetValue(TKey key1, TKey key2, out TValue value)
        {
            if (!m_dict.TryGetValue(key1, out Dictionary<TKey, TValue> clipDictionary)) {
                value = default(TValue);
                return false;
            }
            return clipDictionary.TryGetValue(key2, out value);
        }

        public bool TryGetValue(TKey key, out Dictionary<TKey, TValue> value)
        {
            return m_dict.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key1)
        {
            return m_dict.ContainsKey(key1);
        }

        public IEnumerator<KeyValuePair<TKey, Dictionary<TKey, TValue>>> GetEnumerator()
        {
            return m_dict.GetEnumerator();
        }

        public int GetCount()
        {
            return m_dict.Count;
        }

        public int GetCount(TKey key1)
        {
            if (!m_dict.TryGetValue(key1, out var value)) { return 0; }
            return value.Count;
        }

        public void Add(TKey key1, TKey key2, TValue value)
        {
            CheckKeys(key1, key2);
            m_dict[key1][key2] = value;
        }

        public void Add(TKey key1, Dictionary<TKey, TValue> value)
        {
            CheckKey(key1);
            m_dict[key1] = value;
        }

        public void Clear() { m_dict.Clear(); }

        void CheckKeys(TKey key1, TKey key2)
        {
            if (!m_dict.ContainsKey(key1)) { m_dict[key1] = new Dictionary<TKey, TValue>(); }
            if (!m_dict[key1].ContainsKey(key2)) { m_dict[key1][key2] = default(TValue); } // TODO - may not need this line
        }

        void CheckKey(TKey key1)
        {
            if (!m_dict.ContainsKey(key1)) { m_dict[key1] = new Dictionary<TKey, TValue>(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_dict).GetEnumerator();
        }
    }
} // namespace