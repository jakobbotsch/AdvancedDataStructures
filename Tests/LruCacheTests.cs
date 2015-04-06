using System;
using System.Collections.Generic;
using AdvancedDataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class LruCacheTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ZeroCapacityThrows()
		{
			LruCache<int, int> lru = new LruCache<int, int>(0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NegativeCapacityThrows()
		{
			LruCache<int, int> lru = new LruCache<int, int>(-1);
		}

		[TestMethod]
		[ExpectedException(typeof(KeyNotFoundException))]
		public void RetrieveNonExistingThrows()
		{
			LruCache<int, int> lru = new LruCache<int, int>(1);
			int test = lru[0];
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullKeyThrows()
		{
			LruCache<string, int> lru = new LruCache<string, int>(10);
			lru["hello"] = 10;
			int test = lru[null];
		}
		[TestMethod]
		public void LruProperties()
		{
			LruCache<int, int> lru = new LruCache<int, int>(10);
			Assert.AreEqual(0, lru.Count);
			Assert.AreEqual(10, lru.Capacity);

			for (int i = 0; i < 10; i++)
			{
				lru[i] = i * 5 + 17;
				Assert.AreEqual(1 + i, lru.Count);
				Assert.AreEqual(10, lru.Capacity);
			}

			for (int i = 10; i < 20; i++)
			{
				lru[i] = i * 5 + 17;
				Assert.AreEqual(10, lru.Count);
				Assert.AreEqual(10, lru.Capacity);
			}
		}

		[TestMethod]
		public void TryGetFails()
		{
			LruCache<int, int> lru = new LruCache<int, int>(1);
			int retrieved;
			Assert.IsFalse(lru.TryGetValue(0, out retrieved));
		}

		[TestMethod]
		public void AddingEvictsOld()
		{
			LruCache<int, int> lru = new LruCache<int, int>(1);

			lru[0] = 10;
			Assert.AreEqual(1, lru.Count);

			lru[1] = 15;
			Assert.AreEqual(1, lru.Capacity);
			int retrieved;
			Assert.IsFalse(lru.TryGetValue(0, out retrieved));
			Assert.IsTrue(lru.TryGetValue(1, out retrieved));
		}

		[TestMethod]
		public void AddingEvictsOld2()
		{
			LruCache<int, int> lru = new LruCache<int, int>(4);

			lru[0] = 1;
			lru[1] = 2;
			lru[2] = 3;
			lru[3] = 4;

			// 3 -> 2 -> 1 -> 0; 0 should be evicted if removing
			lru[4] = 5;
			int retrieved;
			Assert.IsFalse(lru.TryGetValue(0, out retrieved));

			retrieved = lru[2];
			retrieved = lru[3];
			retrieved = lru[1];
			// 1 -> 3 -> 2 -> 4; 4 evicted
			lru[5] = 6;
			Assert.IsFalse(lru.TryGetValue(4, out retrieved));

			// 5 -> 1 -> 3 -> 2
			retrieved = lru[2];
			// 2 -> 5 -> 1 -> 3; 3 evicted
			lru[6] = 7;
			Assert.IsFalse(lru.TryGetValue(3, out retrieved));
		}

		[TestMethod]
		public void TestBig()
		{
			
		}

		[TestMethod]
		public void UpdateValue()
		{
			LruCache<int, int> lru = new LruCache<int, int>(4);

			lru[0] = 10;
			lru[0] = 5;
			Assert.AreEqual(1, lru.Count);
			Assert.AreEqual(5, lru[0]);
		}

		[TestMethod]
		public void ValuesAreStored()
		{
			LruCache<int, int> lru = new LruCache<int, int>(4);
			List<KeyValuePair<int, int>> values = new List<KeyValuePair<int, int>>
			{
				new KeyValuePair<int, int>(0, 15),
				new KeyValuePair<int, int>(1, 3),
				new KeyValuePair<int, int>(8, 7),
				new KeyValuePair<int, int>(3, 100)
			};

			for (int i = 0; i < values.Count; i++)
			{
				lru[values[i].Key] = values[i].Value;

				for (int j = 0; j <= i; j++)
				{
					Assert.AreEqual(values[j].Value, lru[values[j].Key]);
					int retrieved;
					Assert.IsTrue(lru.TryGetValue(values[j].Key, out retrieved));
					Assert.AreEqual(values[j].Value, retrieved);
				}
			}
		}
	}
}
