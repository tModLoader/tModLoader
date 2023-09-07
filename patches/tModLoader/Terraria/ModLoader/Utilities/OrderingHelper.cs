using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Utilities;
public interface IOrderable<TSelf> where TSelf : IOrderable<TSelf>
{
	public (TSelf target, bool after) Ordering { get; private protected set; }
}

public static class OrderableExtensions
{
	/// <summary> Orders everything in the enumerable according to their Ordering. </summary>
	public static IEnumerable<TOrderable> GetOrdered<TOrderable>(this IEnumerable<TOrderable> orderables) where TOrderable : IOrderable<TOrderable>
	{
		TOrderable[] orderablesArray = orderables.ToArray();

		// first-pass, collect sortBefore and sortAfter
		Dictionary<TOrderable, List<TOrderable>> sortBefore = new();
		Dictionary<TOrderable, List<TOrderable>> sortAfter = new();
		List<TOrderable> baseOrder = new List<TOrderable>(orderablesArray.Length);
		foreach (TOrderable orderable in orderablesArray) {
			switch (orderable.Ordering) {
				case (null, _):
					baseOrder.Add(orderable);
					break;

				case (var sortBeforeEntry, false): // sortBefore
					if (!sortBefore.TryGetValue(sortBeforeEntry, out List<TOrderable> before))
						before = sortBefore[sortBeforeEntry] = new();

					before.Add(orderable);
					break;

				case (var sortAfterEntry, true): // sortAfter
					if (!sortAfter.TryGetValue(sortAfterEntry, out List<TOrderable> after))
						after = sortAfter[sortAfterEntry] = new();

					after.Add(orderable);
					break;
			}
		}

		if (!sortBefore.Any() && !sortAfter.Any())
			return orderablesArray;

		// define sort function
		int i = 0;
		void Sort(TOrderable r)
		{
			if (sortBefore.TryGetValue(r, out List<TOrderable> before))
				foreach (TOrderable c in before)
					Sort(c);

			orderablesArray[i++] = r;

			if (sortAfter.TryGetValue(r, out List<TOrderable> after))
				foreach (TOrderable c in after)
					Sort(c);
		}

		// second pass, sort!
		foreach (TOrderable r in baseOrder)
			Sort(r);

		if (i != orderablesArray.Length)
			throw new Exception("Sorting code is broken?");
		return orderablesArray;
	}
}