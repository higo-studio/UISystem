using System;
using System.Collections;
using System.Collections.Generic;

namespace Higo.UI
{
    public class UnsafeList<T> : IList<T>
    {
        private T[] m_Array;
        private int m_Length;
        
        public UnsafeList(int capacity = 16)
        {
            m_Array = new T[capacity];
            m_Length = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < m_Length; i++)
            {
                yield return m_Array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < m_Length; i++)
            {
                yield return m_Array[i];
            }
        }
        
        static int NextPowerOf2(int n)
        {
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
         
            return n;
        }

        private void EnsureCapacity(int newCount = 1)
        {
            var newCapacity = NextPowerOf2(m_Length + newCount);
            if (newCapacity != m_Array.Length)
            {
                var newArray = new T[newCapacity];
                Array.Copy(m_Array, newArray, m_Length);
                m_Array = newArray;
            }
        }

        public void Add(T item)
        {
            EnsureCapacity();
            m_Array[m_Length++] = item;
        }

        public void Clear()
        {
            Array.Clear(m_Array, 0, m_Length);
            m_Length = 0;
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(m_Array, item, 0, m_Length) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var count = Math.Min(m_Length, array.Length - arrayIndex);
            for (var i = 0; i < count; i++)
            {
                array[i + arrayIndex] = m_Array[i];
            }
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public int Count => m_Length;
        public bool IsReadOnly => false;
        public int IndexOf(T item)
        {
            return Array.IndexOf(m_Array, item, 0, m_Length);
        }

        public void Insert(int index, T item)
        {
            EnsureCapacity();
            for (var i = index; i < m_Length; i++)
            {
                m_Array[i + 1] = m_Array[i];
            }
            m_Array[index] = item;
            m_Length++;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0)
            {
                for (var i = index; i < m_Length; i++)
                {
                    m_Array[i] = m_Array[i + 1];
                }
                m_Length--;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= m_Length) throw new IndexOutOfRangeException();
                return m_Array[index];
            }
            set
            {
                if (index < 0 || index >= m_Length) throw new IndexOutOfRangeException();
                m_Array[index] = value;
            }
        }
        
        public ref T ElementAt(int index)
        {
            if (index < 0 || index >= m_Length) throw new IndexOutOfRangeException();
            return ref m_Array[index];
        }
    }
}