using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Utilities;

public interface IOrderable<out T> where T : class, IOrderable<T>
{
	public T Target { get; }

	public bool After { get; }
}

public static class OrderableExtensions
{
	/// <summary> Orders everything in the <see cref="IList{T}"/> according to <see cref="IOrderable{T}"/>. </summary>
	public static IEnumerable<TOrderable> GetOrdered<TOrderable>(this IList<TOrderable> orderableIList) where TOrderable : class, IOrderable<TOrderable>
	{
		Dictionary<object, List<TOrderable>> sortBefore = new();
		Dictionary<object, List<TOrderable>> sortAfter = new();
		List<TOrderable> baseOrder = new List<TOrderable>(orderableIList.Count);
		foreach (TOrderable orderable in orderableIList) {
			switch ((orderable.Target, orderable.After)) {
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
			return orderableIList;

		// define sort function
		int i = 0;
		void Sort(TOrderable r)
		{
			if (sortBefore.TryGetValue(r, out List<TOrderable> before))
				foreach (TOrderable c in before)
					Sort(c);

			orderableIList[i++] = r;

			if (sortAfter.TryGetValue(r, out List<TOrderable> after))
				foreach (TOrderable c in after)
					Sort(c);
		}

		// second pass, sort!
		foreach (TOrderable r in baseOrder)
			Sort(r);

		if (i != orderableIList.Count)
			throw new Exception("Sorting code is broken?");
		return orderableIList;
	}
}