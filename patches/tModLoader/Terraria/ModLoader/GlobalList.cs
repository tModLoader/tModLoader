using System;
using System.Collections.Generic;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static class GlobalList<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	private static bool loadingFinished = false;
	private static List<TGlobal> _globals = new();

	/// <summary>
	/// All registered globals. Empty until all globals have loaded
	/// </summary>
	public static TGlobal[] Globals { get; private set; } = Array.Empty<TGlobal>();

	public static int SlotsPerEntity { get; private set; }

	public static int EntityTypeCount { get; private set; }

	internal static (short index, short perEntityIndex) Register(TGlobal global)
	{
		if (loadingFinished)
			throw new Exception("Loading has finished. Cannot add more globals");

		short index = (short)_globals.Count;
		short perEntityIndex = (short)(global.SlotPerEntity ? SlotsPerEntity++ : -1);

		_globals.Add(global);

		return (index, perEntityIndex);
	}

	/// <summary>
	/// Call during <see cref="ILoader.ResizeArrays"/>. Which runs after all <see cref="ILoadable.Load(Mod)"/> calls, but before any <see cref="ModType.SetupContent"/> calls
	/// </summary>
	public static void FinishLoading(int typeCount)
	{
		if (loadingFinished)
			throw new Exception($"{nameof(FinishLoading)} already called");

		loadingFinished = true;
		Globals = _globals.ToArray();
		EntityTypeCount = typeCount;
	}

	/// <summary>
	/// Call during unloading, to clear the globals list
	/// </summary>
	public static void Reset()
	{
		LoaderUtils.ResetStaticMembers(typeof(GlobalList<TGlobal>));
	}
}