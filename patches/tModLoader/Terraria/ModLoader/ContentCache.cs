using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Terraria.ModLoader;

/// <summary>
/// Container object for <see cref="ILoadable"/> instances added for a <see cref="Mod"/>
/// </summary>
internal class ContentCache
{
	private static event Action OnUnload;
	private static Dictionary<nint, IList> _cachedContentForAllMods = new();

	internal static bool hasLoadingStarted;
	internal static bool loadingContent;

	internal static void Unload() {
		Interlocked.Exchange(ref OnUnload, null)?.Invoke();
	}

	private static void UnloadStaticCache() {
		_cachedContentForAllMods.Clear();
	}

	public static IEnumerable<T> GetContentForAllMods<T>() where T : ILoadable {
		if (!hasLoadingStarted || loadingContent) {
			// Content has not fully loaded yet, so we can't rely on the cache.
			// Return a lazy enumerable instead.
			return ModLoader.Mods.SelectMany(static m => m.GetContent<T>());
		}

		// Check of the cache already exists
		// It will already be a ReadOnlyList<T>, so it just needs to be cast back to it
		nint handle = typeof(T).TypeHandle.Value;
		if (_cachedContentForAllMods.TryGetValue(handle, out IList cachedContent))
			return (IReadOnlyList<T>)cachedContent;

		// Construct the cache
		if (_cachedContentForAllMods.Count == 0)
			OnUnload += UnloadStaticCache;

		IReadOnlyList<T> content = ModLoader.Mods.SelectMany(static m => m.GetContent<T>()).ToList().AsReadOnly();
		_cachedContentForAllMods[handle] = (IList)content;
		return content;
	}

	private readonly Mod _mod;
	private readonly List<ILoadable> _content = new List<ILoadable>();
	// nint is used instead of Type because it will have no collisions, and is faster to compare
	private readonly Dictionary<nint, IList> _cachedContent = new();

	internal bool hasModLoadedYet;

	internal ContentCache(Mod mod) {
		_mod = mod;
	}

	internal void Add(ILoadable loadable) {
		_content.Add(loadable);
	}

	public IEnumerable<ILoadable> GetContent() => _content.AsReadOnly();  // Prevent exposing the list via hard cast

	public IEnumerable<T> GetContent<T>() where T : ILoadable {
		if (!hasModLoadedYet || _mod.loading) {
			// Content may not have fully loaded yet, so we can't rely on the cache.
			// Return a lazy enumerable instead.
			return _content.OfType<T>();
		}

		// Check of the cache already exists
		// It will already be a ReadOnlyList<T>, so it just needs to be cast back to it
		nint handle = typeof(T).TypeHandle.Value;
		if (_cachedContent.TryGetValue(handle, out IList cachedContent))
			return (IReadOnlyList<T>)cachedContent;

		// Construct the cache
		if (_cachedContent.Count == 0)
			OnUnload += UnloadInstanceCache;

		IReadOnlyList<T> content = _content.OfType<T>().ToList().AsReadOnly();
		_cachedContent[handle] = (IList)content;
		return content;
	}

	private void UnloadInstanceCache() {
		_content.Clear();
		_cachedContent.Clear();
	}
}
