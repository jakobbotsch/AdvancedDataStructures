using System;
using System.Linq;
using AdvancedDataStructures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	[TestClass]
	public class PriorityQueueTests
	{
		[TestMethod]
		public void TestSort()
		{
			Random rand = new Random(12345678);
			int[] values = Enumerable.Range(0, 10000).Select(i => rand.Next()).ToArray();

			PriorityQueue<int> pq = new PriorityQueue<int>();
			foreach (int i in values)
				pq.Add(i);

			int[] sorted = new int[values.Length];
			int index = pq.Count - 1;
			while (index >= 0)
				sorted[index--] = pq.ExtractMax();

			for (int i = 0; i < sorted.Length - 1; i++)
			{
				Assert.IsTrue(sorted[i] <= sorted[i + 1], $"{sorted[i]} is not <= {sorted[i+1]} @ {i}");
			}
		}
	}
}
