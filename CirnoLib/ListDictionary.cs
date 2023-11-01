using System.Linq;
using System.Collections.Generic;

namespace CirnoLib
{
    public class ListDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new TValue this[TKey key] {
            get {
                TryGetValue(key, out TValue value);
                return value;
            }
            set {
                if (ContainsKey(key))
                    this[key] = value;
                else Add(key, value);
            }
        }

        public TValue this[int index] {
            get => this[Keys.ElementAt(index)];
            set => this[Keys.ElementAt(index)] = value;
        }

        public TKey FindKey(TValue value)
        {
            int index = FindIndex(value);
            return index != -1 ? Keys.ElementAt(index) : default;
        }

        public int FindIndex(TValue value)
        {
            TValue[] values = Values.ToArray();
            for (int i = 0; i < Count; i++)
                if (values[i].Equals(value)) return i;
            return -1;
        }

        public void RemoveAt(int index)
        {
            Remove(Keys.ElementAt(index));
        }
    }
}
