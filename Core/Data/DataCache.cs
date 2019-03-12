﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trivial.Data
{
    /// <summary>
    /// The collection for data cache.
    /// </summary>
    /// <typeparam name="T">The type of data model.</typeparam>
    public class DataCacheCollection<T> : ICollection<DataCacheCollection<T>.ItemInfo>
    {
        /// <summary>
        /// The cache item info.
        /// </summary>
        public class ItemInfo
        {
            /// <summary>
            /// Initializes a new instance of the DataCacheCollection.ItemInfo class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="value">The value.</param>
            /// <param name="expiration">The optional expiration to override current policy.</param>
            public ItemInfo(string id, T value, TimeSpan? expiration = null)
            {
                Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
                Value = value;
                Expiration = expiration;
            }

            /// <summary>
            /// Initializes a new instance of the DataCacheCollection.ItemInfo class.
            /// </summary>
            /// <param name="idPrefix">The prefix of the identifier for resource group.</param>
            /// <param name="id">The identifier in the resource group.</param>
            /// <param name="value">The value.</param>
            /// <param name="expiration">The optional expiration to override current policy.</param>
            public ItemInfo(string idPrefix, string id, T value, TimeSpan? expiration = null) : this(id, value, expiration)
            {
                Prefix = !string.IsNullOrWhiteSpace(idPrefix) ? idPrefix : null;
            }

            /// <summary>
            /// Gets or sets the prefix.
            /// </summary>
            public string Prefix { get; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Gets or sets the creation date.
            /// </summary>
            public DateTime CreationDate { get; internal set; } = DateTime.Now;

            /// <summary>
            /// Gets or sets the update date.
            /// </summary>
            public DateTime UpdateDate { get; } = DateTime.Now;

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public T Value { get; }

            /// <summary>
            /// Gets the optional expiration.
            /// </summary>
            public TimeSpan? Expiration { get; }

            /// <summary>
            /// Gets or sets the expired date.
            /// </summary>
            /// <param name="expiration">The expiration time span.</param>
            /// <returns>The expired date.</returns>
            public DateTime GetExpiredDate(TimeSpan expiration)
            {
                return UpdateDate + (Expiration ?? expiration);
            }

            /// <summary>
            /// Tests if the item is expired.
            /// </summary>
            /// <param name="expiration">The expiration time span.</param>
            /// <returns>true if expired; otherwise, false.</returns>
            public bool IsExpired(TimeSpan? expiration)
            {
                if (!expiration.HasValue && !Expiration.HasValue) return false;
                return DateTime.Now >= (UpdateDate + (Expiration ?? expiration).Value);
            }
        }

        /// <summary>
        /// The cache data list.
        /// </summary>
        private readonly List<ItemInfo> items = new List<ItemInfo>();

        /// <summary>
        /// Gets the maxinum count of the elements contained in the cache item collection.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets the optional expiration.
        /// </summary>
        public TimeSpan? Expiration { get; set; }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is readonly.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than the count.</exception>
        public ItemInfo this[int index] => items[index];

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">id was null, empty or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">The identifier does not exist.</exception>
        public T this[string id]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id should not be null, empty or consists only of white-space characters.");
                return items.Last(ele => ele.Prefix == null && ele.Id == id).Value;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id should not be null, empty or consists only of white-space characters.");
                if (value == null) Remove(value);
                else Add(new ItemInfo(id, value));
            }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="idPrefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">id was null, empty or consists only of white-space characters.</exception>
        /// <exception cref="InvalidOperationException">The identifier does not exist.</exception>
        public T this[string idPrefix, string id]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id should not be null, empty or consists only of white-space characters.");
                return items.Last(ele => ele.Prefix == idPrefix && ele.Id == id).Value;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "id should not be null, empty or consists only of white-space characters.");
                if (value == null) Remove(value);
                else Add(new ItemInfo(idPrefix, id, value));
            }
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire collection, if found; otherwise, –1.</returns>
        public int IndexOf(ItemInfo item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire collection, if found; otherwise, –1.</returns>
        public int IndexOf(T item)
        {
            var i = -1;
            if (item == null) return i;
            foreach (var ele in items)
            {
                i++;
                if (item.Equals(ele)) return i;
            }

            return i;
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire collection, if found; otherwise, –1.</returns>
        public int IndexOf(string id)
        {
            return IndexOf(null, id);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// <param name="prefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire collection, if found; otherwise, –1.</returns>
        public int IndexOf(string prefix, string id)
        {
            var i = -1;
            foreach (var ele in items)
            {
                i++;
                if (ele.Prefix == prefix && ele.Id == id && !ele.IsExpired(Expiration)) return i;
            }

            return i;
        }

        /// <summary>
        /// Gets the cache item info.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The cache item info.</returns>
        public ItemInfo GetInfo(string id)
        {
            return GetInfo(null, id);
        }

        /// <summary>
        /// Gets the cache item info.
        /// </summary>
        /// <param name="prefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <returns>The cache item info.</returns>
        public ItemInfo GetInfo(string prefix, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var info = items.LastOrDefault(ele => ele.Prefix == prefix && ele.Id == id);
            if (info == null) return null;
            if (info.Value == null || info.IsExpired(Expiration))
            {
                RemoveExpired();
                return null;
            }

            return info;
        }

        /// <summary>
        /// Tries to get the cache item info.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="result">The output result.</param>
        /// <returns>true if has the info and it is not expired; otherwise, false.</returns>
        public bool TryGetInfo(string id, out ItemInfo result)
        {
            return TryGetInfo(null, id, out result);
        }

        /// <summary>
        /// Tries to get the cache item info.
        /// </summary>
        /// <param name="prefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <param name="result">The output result.</param>
        /// <returns>true if has the info and it is not expired; otherwise, false.</returns>
        public bool TryGetInfo(string prefix, string id, out ItemInfo result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(id)) return false;
            var info = items.LastOrDefault(ele => ele.Prefix == null && ele.Id == id);
            result = info;
            return info != null && info.Value != null && !info.IsExpired(Expiration);
        }

        /// <summary>
        /// Adds an object to the end of the collection.
        /// </summary>
        /// <param name="item">The object to be added to the end of the collection.</param>
        public void Add(ItemInfo item)
        {
            if (item == null || item.Value == null || item.IsExpired(Expiration)) return;
            Remove(item.Value);
            if (TryGetInfo(item.Prefix, item.Id, out ItemInfo info))
            {
                if (info.UpdateDate > item.UpdateDate && !info.IsExpired(Expiration)) return;
                if (info.CreationDate < item.CreationDate) item.CreationDate = info.CreationDate;
                Remove(info);
            }

            items.Add(item);
        }

        /// <summary>
        /// Adds an object to the end of the collection.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The optional expiration to override current policy.</param>
        public void Add(string id, T value, TimeSpan? expiration = null)
        {
            Add(new ItemInfo(null, id, value, expiration));
        }

        /// <summary>
        /// Adds an object to the end of the collection.
        /// </summary>
        /// <param name="idPrefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The optional expiration to override current policy.</param>
        public void Add(string idPrefix, string id, T value, TimeSpan? expiration = null)
        {
            Add(new ItemInfo(idPrefix, id, value, expiration));
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Determines whether an element is in the collection.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(ItemInfo item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Determines whether an element is in the collection.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(T item)
        {
            if (item == null) return false;
            foreach (var ele in items)
            {
                if (item.Equals(ele)) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether an element is in the collection.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(string id)
        {
            return Contains(null, id);
        }

        /// <summary>
        /// Determines whether an element is in the collection.
        /// </summary>
        /// <param name="prefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <returns>true if item is found in the collection; otherwise, false.</returns>
        public bool Contains(string prefix, string id)
        {
            foreach (var ele in items)
            {
                if (ele.Prefix == prefix && ele.Id == id && !ele.IsExpired(Expiration)) return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from System.Collections.Generic.List`1. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array was null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex was less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.</exception>
        public void CopyTo(ItemInfo[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied from System.Collections.Generic.List`1. The System.Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array was null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex was less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            items.Select(ele => ele.Value).ToList().CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the collection.</returns>
        public bool Remove(ItemInfo item)
        {
            if (item == null) return false;
            if (item.Value == null)
            {
                return Remove(item.Prefix, item.Id);
            }

            var result = items.RemoveAll(ele => item.Value.Equals(ele.Value) || (item.Prefix == ele.Prefix && item.Id == ele.Id)) > 0;
            RemoveExpired();
            return result;
        }

        /// <summary>
        /// Removes the occurrence of a specific value from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the collection.</returns>
        public bool Remove(T item)
        {
            if (item == null) return false;
            var result = items.RemoveAll(ele => item.Equals(ele.Value)) > 0;
            RemoveExpired();
            return result;
        }

        /// <summary>
        /// Removes the occurrence with the specific identifier from the collection.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the collection.</returns>
        public bool Remove(string id)
        {
            return Remove(null, id);
        }

        /// <summary>
        /// Removes the occurrence with the specific identifier from the collection.
        /// </summary>
        /// <param name="prefix">The prefix of the identifier for resource group.</param>
        /// <param name="id">The identifier in the resource group.</param>
        /// <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the collection.</returns>
        public bool Remove(string prefix, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            var result = items.RemoveAll(ele => ele.Prefix == prefix && ele.Id == id) > 0;
            RemoveExpired();
            return result;
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than the count.</exception>
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
            RemoveExpired();
        }

        /// <summary>
        /// Removes the elements expired.
        /// </summary>
        public void RemoveExpired()
        {
            items.RemoveAll(ele => ele.IsExpired(Expiration));
            if (!MaxCount.HasValue) return;
            var maxCount = MaxCount.Value;
            if (maxCount < 1)
            {
                items.Clear();
            }
            else
            {
                while (items.Count > maxCount) items.RemoveAt(0);
            }
        }

        /// <summary>
        /// Tests if the item is expired.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns>true if expired; otherwise, false.</returns>
        public bool IsExpired(ItemInfo item)
        {
            return item.IsExpired(Expiration);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<ItemInfo> GetEnumerator()
        {
            return ((IList<ItemInfo>)items).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<ItemInfo>)items).GetEnumerator();
        }
    }
}