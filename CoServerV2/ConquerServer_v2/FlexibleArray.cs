using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2
{
    public unsafe class FlexibleArray<T>
    {
        private int _size;
        private T[] _items;
        private void EnsureCapacity(int min)
        {
            if (this._items.Length < min)
            {
                int num = (this._items.Length == 0) ? 4 : (this._items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                T[] destinationArray = new T[num];
                Array.Copy(this._items, 0, destinationArray, 0, this._size);
                this._items = destinationArray;
            }
        }

        public FlexibleArray()
        {
            _size = 0;
            _items = new T[0];
        }
        public void Add(T Element)
        {
            lock (this)
            {
                if (this._size == this._items.Length)
                {
                    this.EnsureCapacity(this._size + 1);
                }
                this._items[this._size++] = Element;
            }
        }
        public void Remove(int index)
        {
            lock (this)
            {
                this._size--;
                if (index < this._size)
                {
                    Array.Copy(this._items, index + 1, this._items, index, this._size - index);
                }
                this._items[this._size] = default(T);
            }
        }
        public void SetCapacity(int value)
        {
            EnsureCapacity(value);
        }
        /// <summary>
        /// Note: What's returned here is NOT a trimmed aray, so when looping through it
        /// always use a for loop, with 'Length' of the FlexibleArray class as the end marker
        /// </summary>
        public T[] Elements { get { return _items; } set { _items = value; _size = value.Length; } }
        public int Length { get { return _size; } }
        public T[] ToTrimmedArray()
        {
            T[] trimmed = new T[_size];
            Array.Copy(_items, trimmed, _size);
            return trimmed;
        }
    }
}
