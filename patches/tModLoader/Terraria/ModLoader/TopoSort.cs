using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.ModLoader;

public class TopoSort<T>
{
	public class SortingException : Exception
	{
		public ISet<T> set = new HashSet<T>();
		public IList<List<T>> cycles = new List<List<T>>();

		private string CycleToString(List<T> cycle) => "Dependency Cycle: " + string.Join(" -> ", cycle);
		public override string Message => string.Join(Environment.NewLine, cycles.Select(CycleToString));

		public void Add(List<T> cycle)
		{
			cycles.Add(cycle);
			foreach (var e in cycle)
				set.Add(e);
		}
	}

	public readonly ReadOnlyCollection<T> list;
	private IDictionary<T, List<T>> dependencyDict = new Dictionary<T, List<T>>();
	private IDictionary<T, List<T>> dependentDict = new Dictionary<T, List<T>>();

	public TopoSort(IEnumerable<T> elements, Func<T, IEnumerable<T>> dependencies = null, Func<T, IEnumerable<T>> dependents = null)
	{
		list = elements.ToList().AsReadOnly();
		if (dependencies != null)
			foreach (var t in list)
				foreach (var dependency in dependencies(t))
					AddEntry(dependency, t);

		if (dependents != null)
			foreach (var t in list)
				foreach (var dependent in dependents(t))
					AddEntry(t, dependent);
	}

	public void AddEntry(T dependency, T dependent)
	{
		if (!dependencyDict.TryGetValue(dependent, out List<T> list))
			dependencyDict[dependent] = list = new List<T>();
		list.Add(dependency);

		if (!dependentDict.TryGetValue(dependency, out list)) dependentDict[dependency] = list = new List<T>();
		list.Add(dependent);
	}

	private static void BuildSet(T t, IDictionary<T, List<T>> dict, ISet<T> set)
	{
		if (!dict.TryGetValue(t, out List<T> list))
			return;

		foreach (var entry in dict[t])
			if (set.Add(entry))
				BuildSet(entry, dict, set);
	}

	public List<T> Dependencies(T t)
	{
		return dependencyDict.TryGetValue(t, out List<T> list) ? list : new List<T>();
	}

	public List<T> Dependents(T t)
	{
		return dependentDict.TryGetValue(t, out List<T> list) ? list : new List<T>();
	}

	public ISet<T> AllDependencies(T t)
	{
		var set = new HashSet<T>();
		BuildSet(t, dependencyDict, set);
		return set;
	}

	public ISet<T> AllDependendents(T t)
	{
		var set = new HashSet<T>();
		BuildSet(t, dependentDict, set);
		return set;
	}

	public List<T> Sort()
	{
		var ex = new SortingException();
		var visiting = new Stack<T>();
		var sorted = new List<T>();

		Action<T> Visit = null;
		Visit = t => {
			if (sorted.Contains(t) || ex.set.Contains(t))
				return;

			visiting.Push(t);
			foreach (var dependency in Dependencies(t)) {
				if (visiting.Contains(dependency)) {//walk down the visiting stack to extract the dependency cycle
					var cycle = new List<T>();
					cycle.Add(dependency);
					cycle.AddRange(visiting.TakeWhile(entry => !EqualityComparer<T>.Default.Equals(entry, dependency)));
					cycle.Add(dependency);
					cycle.Reverse();//items at top of the stack (start of the list) are deepest in the dependency tree
					ex.Add(cycle);
					continue;
				}

				Visit(dependency);
			}
			visiting.Pop();
			sorted.Add(t);
		};

		foreach (var t in list)
			Visit(t);

		if (ex.set.Count > 0)
			throw ex;

		return sorted;
	}
}
