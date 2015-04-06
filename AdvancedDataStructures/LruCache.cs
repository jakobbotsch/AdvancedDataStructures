using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdvancedDataStructures
{
	/// <summary>
	/// Represents a least-recently-used cache.
	/// </summary>
	/// <typeparam name="TKey">The key type.</typeparam>
	/// <typeparam name="TValue">The value type.</typeparam>
	// Todo: Unit tests
	public sealed class LruCache<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> _comparer;
		private int[] _buckets;
		private Entry[] _entries;
		private int _headUsed; // The most recently used entry
		private int _tailUsed; // The least recently used entry

		public LruCache(int capacity, IEqualityComparer<TKey> comparer = null)
		{
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException("capacity");

			Capacity = capacity;
			_comparer = comparer ?? EqualityComparer<TKey>.Default;
		}

		public int Capacity { get; private set; }
		// The count is the number of used entries in the _entries list.
		public int Count { get; private set; }

		public TValue this[TKey key]
		{
			get
			{
				int index = FindEntry(key);
				if (index != -1)
					return _entries[index].Value;

				throw new KeyNotFoundException();
			}
			set
			{
				Insert(key, value);
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int index = FindEntry(key);
			if (index != -1)
			{
				value = _entries[index].Value;
				return true;
			}

			value = default(TValue);
			return false;
		}

		private int FindEntry(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (_buckets == null)
				return -1;

			uint hashCode = unchecked((uint)_comparer.GetHashCode(key));
			for (int i = _buckets[hashCode % unchecked((uint)_buckets.Length)]; i != -1; i = _entries[i].Next)
			{
				if (_entries[i].HashCode != hashCode || !_comparer.Equals(_entries[i].Key, key))
					continue;

				UpdateUsage(i);
				return i;
			}

			return -1;
		}

		private void UpdateUsage(int entryIndex)
		{
			// Visual Basic defeats C# here...
			UpdateUsage(entryIndex, ref _entries[entryIndex]);
		}

		private void UpdateUsage(int entryIndex, ref Entry entry)
		{
			Debug.Assert(_headUsed != -1 && _tailUsed != -1);
			// If we do not have a previous, we are the head.
			// If we do not have a next, we are the tail.
			// The idea of this subroutine is to move the entry index to the head of the list.

			int previousUsed = entry.PreviousUsed;
			int nextUsed = entry.NextUsed;

			if (previousUsed != -1)
			{
				// We have a previous. Link that to the next instead
				_entries[previousUsed].NextUsed = nextUsed;
			}
			else
			{
				// This is already the head.
				Debug.Assert(_headUsed == entryIndex);
				return;
			}

			if (nextUsed != -1)
			{
				// We have a next.
				_entries[nextUsed].PreviousUsed = previousUsed;
			}
			else
			{
				Debug.Assert(_tailUsed == entryIndex);
				// This is the tail. Fix that.
				_tailUsed = previousUsed;
			}

			// This will become head, so we will not have a previous.
			entry.PreviousUsed = -1;
			// Finally update head as well.
			entry.NextUsed = _headUsed;
			if (_headUsed != -1)
				_entries[_headUsed].PreviousUsed = entryIndex;

			// Finally update head.
			_headUsed = entryIndex;
		}

		private void Insert(TKey key, TValue value)
		{
			if (_buckets == null)
			{
				Initialize();
				Debug.Assert(_buckets != null);
			}

			uint hashCode = unchecked((uint)_comparer.GetHashCode(key));

			uint bucket = hashCode % unchecked((uint)_buckets.Length);
			// Update value?
			for (int i = _buckets[bucket]; i != -1; i = _entries[i].Next)
			{
				if (_entries[i].HashCode != hashCode || !_comparer.Equals(_entries[i].Key, key))
					continue;

				_entries[i].Value = value;
				UpdateUsage(i);
				return;
			}

			// Must add new one.
			int entryIndex;
			if (Count == Capacity)
			{
				// LRU cache is full. Remove the tail.
				entryIndex = FreeTail();
			}
			else
			{
				if (Count == _entries.Length)
				{
					Resize();
					// Bucket has changed since the count has changed!
					bucket = hashCode % unchecked((uint)_buckets.Length);
				}

				entryIndex = Count;
			}

			UpdateEntryForInsert(entryIndex, ref _entries[entryIndex], bucket, hashCode, key, value);
			Count++;
		}

		private int FreeTail()
		{
			Debug.Assert(_tailUsed != -1);

			int freed = _tailUsed;

			if (Count != 1)
			{
				Debug.Assert(_headUsed != -1 && _tailUsed != -1 && _tailUsed != _headUsed,
					"When the list has more than 1 element, tail and head should not be equal to each other or -1.");

				// There are more elements than 1, and we have filled the cache.
				// Remove the tail, and reuse its entry. Since we are removing the tail,
				// we only need to worry about the previous one (next should be -1).
				Debug.Assert(_entries[_tailUsed].NextUsed == -1, "Tail->Next should be -1");
				int previous = _entries[_tailUsed].PreviousUsed;

				// Previous is the tail now.
				_entries[previous].NextUsed = -1;
				_tailUsed = previous;
				// The entry will be updated later, when it's moved to be the head.
			}
			else
			{
				Debug.Assert(_headUsed != -1 && _headUsed == _tailUsed);

				_headUsed = -1;
				_tailUsed = -1;
			}

			// Remove from bucket
			uint bucket = _entries[freed].HashCode % unchecked((uint)_buckets.Length);

			int last = -1;
			for (int i = _buckets[bucket]; i != -1; last = i, i = _entries[i].Next)
			{
				if (i != freed)
					continue;

				if (last == -1)
				{
					// Was in _buckets[bucket]
					_buckets[bucket] = _entries[i].Next;
				}
				else
				{
					_entries[last].Next = _entries[i].Next;
				}

				break;
			}

			// We "removed" one.
			Count--;

			return freed;
		}

		private void UpdateEntryForInsert(int index, ref Entry entry, uint bucket, uint hashCode, TKey key, TValue value)
		{
			// Update entry.
			entry.HashCode = hashCode;
			entry.Next = _buckets[bucket];
			entry.Key = key;
			entry.Value = value;

			// Move to head - this was the most recently used.
			int nextUsed = _headUsed;
			entry.NextUsed = nextUsed;
			// Point next back to this.
			if (nextUsed != -1)
				_entries[nextUsed].PreviousUsed = index;

			// We are the head, so we have no previous.
			entry.PreviousUsed = -1;
			_headUsed = index;

			// This might also be tail.
			if (_tailUsed == -1)
			{
				Debug.Assert(Count == 0);
				_tailUsed = index;
			}

			// Finally point the bucket to this entry, since it is the first in this bucket.
			_buckets[bucket] = index;
		}

		private void Initialize()
		{
			int capacity = LruCacheHelpers.Primes.First();

			_buckets = new int[capacity];
			for (int i = 0; i < _buckets.Length; i++)
				_buckets[i] = -1;

			_entries = new Entry[capacity];

			_headUsed = -1;
			_tailUsed = -1;
		}

		private void Resize()
		{
			uint newCapacity = GetResizeCapacity();

			int[] newBuckets = new int[newCapacity];

			for (int i = 0; i < newBuckets.Length; i++)
				newBuckets[i] = -1;

			Entry[] newEntries = new Entry[newCapacity];
			Array.Copy(_entries, newEntries, Count);

			// Fix up new buckets; since the size has changed, all entries will start from
			// different buckets. However, since Entry.NextUsed/PreviousUsed point directly into
			// _entries, those need not be updated.
			for (int i = 0; i < Count; i++)
			{
				uint newBucket = newEntries[i].HashCode % newCapacity;
				newEntries[i].Next = newBuckets[newBucket];
				newBuckets[newBucket] = i;
			}

			_buckets = newBuckets;
			_entries = newEntries;
		}

		private uint GetResizeCapacity()
		{
			int growSize = Math.Min(Capacity, Count * 2);
			int prime = LruCacheHelpers.ExpandToPrime(growSize);
			if (prime < 0)
				throw new InvalidOperationException("Capacity of LRU cache is too big");

			return unchecked((uint)prime);
		}

		// The LRU cache is implemented like a normal chained hash table
		// For cache coherency, the entries are stored sequentially in an array.
		// This trick is picked up from the implementation of Dictionary<TKey, TValue>.
		private struct Entry
		{
			public uint HashCode;
			public int Next; // Index of next entry inside _entries.
			public TKey Key;
			public TValue Value;
			public int PreviousUsed; // The previous used in LRU list.
			public int NextUsed; // The next used in LRU list.
		}
	}
}
