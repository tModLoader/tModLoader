using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Utilities;

internal static class SortBeforeAfter
{
	public static List<T> Sort<T>(IEnumerable<T> values, Func<T, (T target, bool after)> ordering)
	{
		var baseOrder = new List<T>();
		var sortBefore = new Dictionary<T, List<T>>();
		var sortAfter = new Dictionary<T, List<T>>();
		int count = 0;

		foreach (var r in values) {
			count++;
			switch (ordering(r)) {
				case (null, _):
					baseOrder.Add(r);
					break;
				case (var target, false):
					if (!sortBefore.TryGetValue(target, out var before))
						before = sortBefore[target] = new();

					before.Add(r);
					break;
				case (var target, true):
					if (!sortAfter.TryGetValue(target, out var after))
						after = sortAfter[target] = new();

					after.Add(r);
					break;
			}
		}

		if ((sortBefore.Count + sortAfter.Count) == 0)
			return values.ToList();

		var sorted = new List<T>(count);
		void Visit(T r)
		{
			if (sortBefore.TryGetValue(r, out var before))
				foreach (var c in before)
					Visit(c);

			sorted.Add(r);

			if (sortAfter.TryGetValue(r, out var after))
				foreach (var c in after)
					Visit(c);
		}

		foreach (var r in baseOrder) {
			Visit(r);
		}

		if (sorted.Count != count) {
			var all = values.ToHashSet();
			var placed = sorted.ToHashSet();
			var chain = new List<T> { values.First(r => !placed.Contains(r)) };
			while (true) {
				var target = ordering(chain[^1]).target;
				if (!all.Contains(target))
					throw new InvalidOperationException($"{chain[^1]} is sorted relative to {target}, which is not in the list");

				int i = chain.IndexOf(target);
				if (i >= 0)
					throw new InvalidOperationException("Ordering loop: " + string.Join(" -> ", chain.Skip(i).Append(target)));

				chain.Add(target);
			}
		}

		return sorted;
	}
}
