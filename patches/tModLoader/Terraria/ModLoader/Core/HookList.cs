using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Terraria.ModLoader.Core;

public class HookList<T> where T : class
{
	public readonly MethodInfo method;

	private int[] indices = Array.Empty<int>();
	private T[] defaultInstances = Array.Empty<T>();

	public HookList(MethodInfo method)
	{
		this.method = method;
	}

	public FilteredArrayEnumerator<T> Enumerate(T[] instances)
		=> new(instances, indices);

	public FilteredSpanEnumerator<T> Enumerate(ReadOnlySpan<T> instances)
		=> new(instances, indices);
	public FilteredSpanEnumerator<T> Enumerate(IEntityWithInstances<T> entity)
		=> Enumerate(entity.Instances);

	public IEnumerable<T> EnumerateSlow(IReadOnlyList<T> instances)
	{
		foreach (var i in indices)
			yield return instances[i];
	}

	// Sadly, returning ReadOnlySpan<T>.Enumerator from a GetEnumerator() method doesn't bring the same performance
	public ReadOnlySpan<T> Enumerate() => defaultInstances;

	public void Update(IReadOnlyList<T> allDefaultInstances)
	{
		if (allDefaultInstances is IReadOnlyList<IIndexed> indexed && !Validate(indexed))
			throw new ArgumentException($"{nameof(allDefaultInstances)} elements have missing or duplicate {nameof(IIndexed)}.{nameof(IIndexed.Index)}");

		var list = new List<T>(allDefaultInstances.Count);
		var inds = new List<int>();
		for (int i = 0; i < allDefaultInstances.Count; i++) {
			var inst = allDefaultInstances[i];
			if (LoaderUtils.HasOverride(inst.GetType(), method)) {
				list.Add(inst);
				inds.Add(i);
			}
		}

		defaultInstances = list.ToArray();
		indices = inds.ToArray();
	}

	private static bool Validate(IReadOnlyList<IIndexed> list)
	{
		for (int i = 0; i < list.Count; i++) {
			if (list[i].Index != i)
				return false;
		}

		return true;
	}

	public static HookList<T> Create(Expression<Func<T, Delegate>> expr) => Create<Delegate>(expr);
	public static HookList<T> Create<F>(Expression<Func<T, F>> expr) where F : Delegate
		=> new(expr.ToMethodInfo());
}