using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2
{
    /// <summary>
    /// Provides a dictionary, with the ability to remove from,
    /// while looping (Encalpsulates System.Collection.Generic.Dictionary)
    /// </summary>
    /// <typeparam name="TKey">The key-type</typeparam>
    /// <typeparam name="TElement">The element-type</typeparam>
    public class DictionaryV2<TKey, TElement> : Dictionary<TKey, TElement>
    {
        private TElement[] m_Values;
        public TElement[] EnumerableValues { get { return m_Values; } }
        public DictionaryV2() : base()
        {
            m_Values = new TElement[0];
        }
        /// <summary>
        /// Sychoronizes the EnumerableValues array with the underlying Dictionary.
        /// </summary>
        public void SynchoronizeValues()
        {
            lock (this)
            {
                TElement[] temp = new TElement[this.Count];
                this.Values.CopyTo(temp, 0);
                m_Values = temp;
            }
        }
        /// <summary>
        /// [ThreadSafe] Adds an element to the collection.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value to add</param>
        public new void Add(TKey key, TElement value)
        {
            lock (this)
            {
                base.Add(key, value);
                SynchoronizeValues();
            }
        }
        /// <summary>
        /// [ThreadSafe] Overrides an existing value/key combination, otherwise adds it to the collection.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Override(TKey key, TElement value)
        {
            if (ContainsKey(key))
            {
                this[key] = value;
            }
            else
            {
                Add(key, value);
            }
            SynchoronizeValues();
        }
        /// <summary>
        /// [ThreadSafe] Removes an element from the collection
        /// </summary>
        /// <param name="key">The key of the element</param>
        public new bool Remove(TKey key)
        {
            return Remove(key, true);
        }
        /// <summary>
        /// [ThreadSafe] Removes an element from the collection
        /// </summary>
        /// <param name="key">The key of the element</param>
        /// <param name="ReSync">Whether to call SynchoronizeValues() or not.</param>
        public bool Remove(TKey key, bool ReSync)
        {
            bool result = false;
            lock (this)
            {
                if (base.Remove(key))
                {
                    if (ReSync)
                        SynchoronizeValues();
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Clears the collection and resyncs the values.
        /// </summary>
        /// <returns></returns>
        public new void Clear()
        {
            base.Clear();
            SynchoronizeValues();
        }
    }
}
