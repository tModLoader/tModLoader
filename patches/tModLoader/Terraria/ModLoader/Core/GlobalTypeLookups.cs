using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Terraria.ModLoader.Core;

public static class GlobalTypeLookups<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	public static bool Initialized { get; private set; } = false;
	private static TGlobal[][] _globalsForType = null;
	private static AppliesToTypeSet[] _appliesToType = null;
	private static SortedDictionary<Memory<TGlobal>, TGlobal[]> _cache = new(new CachedArrayComparer());

	private static int EntityTypeCount => GlobalList<TGlobal>.EntityTypeCount;

	static GlobalTypeLookups()
	{
		TypeCaching.ResetStaticMembersOnClear(typeof(GlobalTypeLookups<TGlobal>));
	}

	public static void Init(TGlobal[][] globalsForType, AppliesToTypeSet[] appliesToTypeCache)
	{
		if (Initialized)
			throw new Exception($"{nameof(Init)} already called");

		Initialized = true;
		_globalsForType = globalsForType;
		_appliesToType = appliesToTypeCache;
	}

	public static TGlobal[] GetGlobalsForType(int type)
	{
		if (type == 0)
			return Array.Empty<TGlobal>();

		if (_globalsForType == null)
			throw new Exception("Cannot lookup globals by type until after PostSetupContent, consider moving the calling code to [Post]AddRecipes or later instead");

		return _globalsForType[type];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool AppliesToType(TGlobal global, int type)
	{
		return type > 0 && (!global.ConditionallyAppliesToEntities || AppliesToTypeCacheLookup(global.StaticIndex, type));
	}

	private static bool AppliesToTypeCacheLookup(int index, int type)
	{
		if (_appliesToType == null)
			throw new Exception("Cannot access conditional globals on an entity until after PostSetupContent, consider moving the calling code to [Post]AddRecipes or later, or accessing the global definition directly");

		return _appliesToType[index][type];
	}

	public static TGlobal[] CachedFilter(TGlobal[] arr, Predicate<TGlobal> filter)
	{
		var buf = ArrayPool<TGlobal>.Shared.Rent(arr.Length);
		try {
			int i = 0;
			foreach (var g in arr)
				if (filter(g))
					buf[i++] = g;

			if (i == arr.Length)
				return arr;

			lock (_cache) {
				var filtered = buf.AsMemory()[..i];
				if (_cache.TryGetValue(filtered, out var cached))
					return cached;

				var result = filtered.ToArray();
				_cache.Add(result, result);
				return result;
			}
		}
		finally {
			ArrayPool<TGlobal>.Shared.Return(buf, clearArray: true);
		}
	}

	public static TGlobal[][] BuildPerTypeGlobalLists(TGlobal[] arr)
	{
		// use the already de-duplicated per-type global lists as 'type profiles' to avoid doing extra work, effectively memoizing the CachedFilter call
		var dict = new Dictionary<TGlobal[], TGlobal[]>();

		var lookup = new TGlobal[EntityTypeCount][];
		for (int type = 0; type < lookup.Length; type++) {
			var typeProfile = GetGlobalsForType(type);
			if (!dict.TryGetValue(typeProfile, out var v))
				dict[typeProfile] = v = CachedFilter(arr, g => AppliesToType(g, type));

			lookup[type] = v;
		}

		return lookup;
	}

	public static void LogStats()
	{
		var globals = GlobalList<TGlobal>.Globals;
		int instanced = globals.Count(g => g.InstancePerEntity);
		int conditionalWithSlot = globals.Count(g => g.ConditionallyAppliesToEntities && g.SlotPerEntity);
		int conditionalByType = globals.Count(g => g.ConditionallyAppliesToEntities && !g.SlotPerEntity);
		int appliesToSingleType = _appliesToType.Count(s => s.SingleType > 0);
		int cacheEntries = _cache.Count;
		int cacheSize = _cache.Values.Sum(e => e.Length * 8 + 16);

		Logging.tML.Debug(
			$"{typeof(TGlobal).Name} registration stats. Count: {globals.Length}, Slots per Entity: {GlobalList<TGlobal>.SlotsPerEntity}\n\t" +
			$"Instanced: {instanced}, Conditional with slot: {conditionalWithSlot}, Conditional by type: {conditionalByType}, Applies to single type: {appliesToSingleType}\n\t" +
			$"List Permutations: {cacheEntries}, Est Memory Consumption: {cacheSize} bytes");
	}

	private class CachedArrayComparer : IComparer<Memory<TGlobal>>
	{
		public int Compare(Memory<TGlobal> m1, Memory<TGlobal> m2)
		{
			if (m1.Length.CompareTo(m2.Length) is int c && c != 0) return c;

			var s1 = m1.Span;
			var s2 = m2.Span;
			for (int i = 0; i < s1.Length; i++) {
				var g1 = s1[i];
				var g2 = s2[i];
				if (g1.StaticIndex.CompareTo(g2.StaticIndex) is int c2 && c2 != 0) return c2;

				if (g1 != g2)
					throw new Exception($"Two globals with the same static index in the cache! Is one of them instanced? ({g1},{g2})");
			}

			return 0;
		}
	}

	public struct AppliesToTypeSet
	{
		private struct BitSet
		{
			private long[] arr;
			public bool this[int i] => arr != null && (arr[i >> 6] & 1L << (i & 0x3F)) != 0;
			public bool IsEmpty => arr == null;

			public void Set(int i)
			{
				arr ??= new long[(EntityTypeCount + 0x3F) >> 6];
				arr[i >> 6] |= 1L << (i & 0x3F);
			}
		}

		public int SingleType { get; private set; }
		private BitSet _bitset;

		public bool this[int type] => type == SingleType || _bitset[type];

		public void Add(int type)
		{
			if (_bitset.IsEmpty && SingleType == 0) {
				SingleType = type;
				return;
			}

			if (SingleType > 0) {
				_bitset.Set(SingleType);
				SingleType = 0;
			}

			_bitset.Set(type);
		}
	}
}
