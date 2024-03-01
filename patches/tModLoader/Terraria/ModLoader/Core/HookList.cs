using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Terraria.ModLoader.Core;

public class HookList<T> where T : class, IIndexed
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
		defaultInstances = allDefaultInstances.WhereMethodIsOverridden(method).ToArray();
		indices = defaultInstances.Select(g => (int)g.Index).ToArray();
	}

	public static HookList<T> Create(Expression<Func<T, Delegate>> expr) => Create<Delegate>(expr);
	public static HookList<T> Create<F>(Expression<Func<T, F>> expr) where F : Delegate
		=> new(expr.ToMethodInfo());
}