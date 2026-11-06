using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.Utilities;

public interface IOrderable 
{
	public (IOrderable target, bool after) Ordering => (Target, After);
	public IOrderable Target { get; set; }
	public bool After { get; set; }
}

public static class OrderableExtensions
{
	/// <summary> Orders everything in the <see cref="IList{T}"/> according to <see cref="IOrderable"/>. </summary>
	public static IList<TOrderable> GetOrdered<TOrderable>(this IList<TOrderable> orderableIList) where TOrderable : IOrderable
		=> orderableIList.GetOrdered<IList<TOrderable>, TOrderable>();

	/// <summary> Orders everything in the <see cref="IList{T}"/> according to <see cref="IOrderable"/>. </summary>
	public static TList GetOrdered<TList, TOrderable>(this TList orderableIList) where TOrderable : IOrderable where TList : IList<TOrderable>
	{
		Dictionary<object, List<TOrderable>> sortBefore = new();
		Dictionary<object, List<TOrderable>> sortAfter = new();
		List<TOrderable> baseOrder = new List<TOrderable>(orderableIList.Count);
		foreach (TOrderable orderable in orderableIList) {
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