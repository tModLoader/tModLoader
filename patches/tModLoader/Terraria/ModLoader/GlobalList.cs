using System;
using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class GlobalList<TGlobal> where TGlobal : GlobalType<TGlobal>
{
	private static bool loadingFinished = false;
	private static List<TGlobal> _globals = new();
	private static List<TGlobal> _slotPerEntityGlobals = new();

	/// <summary>
	/// All registered globals. Empty until all globals have loaded
	/// </summary>
	public static TGlobal[] Globals { get; private set; } = Array.Empty<TGlobal>();

	/// <summary>
	/// All registered globals with <see cref="GlobalType{TGlobal}.SlotPerEntity"/> in order of their <see cref="GlobalIndex.PerEntityIndex"/>
	/// </summary>
	public static TGlobal[] SlotPerEntityGlobals { get; private set; } = Array.Empty<TGlobal>();

	internal static (short index, short perEntityIndex) Register(TGlobal global)
	{
		if (loadingFinished)
			throw new Exception("Loading has finished. Cannot add more globals");

		short index = (short)_globals.Count;
		short perEntityIndex = -1;

		_globals.Add(global);

		if (global.SlotPerEntity) {
			perEntityIndex = (short)_slotPerEntityGlobals.Count;
			_slotPerEntityGlobals.Add(global);
		}

		return (index, perEntityIndex);
	}

	/// <summary>
	/// Call during <see cref="ILoader.ResizeArrays"/>. Which runs after all <see cref="ILoadable.Load(Mod)"/> calls, but before any <see cref="ModType.SetupContent"/> calls
	/// </summary>
	public static void FinishLoading()
	{
		if (loadingFinished)
			throw new Exception($"{nameof(FinishLoading)} already called");

		loadingFinished = true;
		Globals = _globals.ToArray();
		SlotPerEntityGlobals = _slotPerEntityGlobals.ToArray();
	}

	/// <summary>
	/// Call during unloading, to clear the globals list
	/// </summary>
	public static void Reset()
	{
		loadingFinished = false;
		_globals.Clear();
		_slotPerEntityGlobals.Clear();
		Globals = Array.Empty<TGlobal>();
		SlotPerEntityGlobals = Array.Empty<TGlobal>();
	}
}
