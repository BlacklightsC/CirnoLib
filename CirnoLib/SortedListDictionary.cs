using System;
using System.Linq;
using System.Collections.Generic;

namespace CirnoLib
{
    public class SortedListDictionary<TKey, TValue> : SortedDictionary<TKey, TValue>
    {
        /// <summary>
        /// 비어 있고 키 형식에 대해 기본적으로 구현된 <see cref="SortedListDictionary{TKey, TValue}"/> 사용하는
        /// 빈 <see cref="IComparer{T}"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public SortedListDictionary() : base() {}

        /// <summary>
        /// 지정된 <see cref="SortedListDictionary{TKey, TValue}"/> 구현을 사용하여 키를 비교하는 빈 <see cref="IComparer{T}"/>
        /// 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="comparer">
        /// 키를 비교할 때 사용할 <see cref="IComparer{T}"/> 구현을 지정하거나, 해당 키 형식에 기본 null을
        /// 사용하려면 <see cref="IComparer{T}"/>을 지정합니다.
        /// </param>
        public SortedListDictionary(IComparer<TKey> comparer) : base(comparer) { }

        /// <summary>
        /// 지정한 <see cref="SortedListDictionary{TKey, TValue}"/>에서 복사된 요소를 포함하고 키 형식에 대해 기본적으로
        /// 구현된 <see cref="IDictionary{TKey, TValue}"/>을 사용하는 <see cref="IComparer{T}"/>
        /// 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="dictionary">요소가 새 <see cref="IDictionary{TKey, TValue}"/>에 복사되는 <see cref="IDictionary{TKey, TValue}"/>입니다.</param>
        /// <exception cref="ArgumentNullException"> dictionary가 null인 경우</exception>
        /// <exception cref="ArgumentException">dictionary 하나 이상의 중복 된 키를 포함합니다.</exception>
        public SortedListDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

        /// <summary>
        /// 지정한 <see cref="SortedListDictionary{TKey, TValue}"/>에서 복사된 요소를 포함하고 지정한 <see cref="IDictionary{TKey, TValue}"/>
        /// 구현을 사용하여 키를 비교하는 <see cref="IComparer{T}"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="dictionary">요소가 새 <see cref="IDictionary{TKey, TValue}"/>에 복사되는 <see cref="SortedListDictionary{TKey, TValue}"/>입니다.</param>
        /// <param name="comparer">
        /// 키를 비교할 때 사용할 <see cref="IComparer{T}"/> 구현을 지정하거나, 해당 키 형식에 기본 null을
        /// 사용하려면 <see cref="IComparer{T}"/>을 지정합니다.
        /// </param>
        /// <exception cref="ArgumentNullException"> dictionary가 null인 경우</exception>
        /// <exception cref="ArgumentException">dictionary 하나 이상의 중복 된 키를 포함합니다.</exception>
        public SortedListDictionary(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer) : base(dictionary, comparer) { }

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
            return index != -1 ? Keys.ToArray()[index] : default;
        }

        public int FindIndex(TValue value)
        {
            TValue[] values = Values.ToArray();
            for (int i = 0; i < Count; i++)
                if (values[i].Equals(value)) return i;
            return -1;
        }

        /// <summary>
        /// <see cref="SortedListDictionary{TKey, TValue}"/>의 지정한 인덱스에서 요소를 제거합니다.
        /// </summary>
        /// <param name="index">제거할 요소의 인덱스(0부터 시작)입니다.</param>
        /// <exception cref="ArgumentOutOfRangeException"> index가 0보다 작은 경우 또는 index가 System.Collections.Generic.List`1.Count보다 크거나 같은 경우</exception>
        public void RemoveAt(int index)
        {
            Remove(Keys.ToArray()[index]);
        }
    }
}
