using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader;

/// <summary>
/// Container object for <see cref="ILoadable"/> instances added for a <see cref="Mod"/>
/// </summary>
internal class ContentCache
{
	private static readonly Dictionary<Type, IList> _cachedContentForAllMods = new();

	internal static bool contentLoadingFinished;

	internal static void Unload() {
		contentLoadingFinished = false;
		_cachedContentForAllMods.Clear();
	}

	public static IEnumerable<T> GetContentForAllMods<T>() where T : ILoadable {
		// Check of the cache already exists
		// It will already be a ReadOnlyList<T>, so it just needs to be cast back to it
		if (_cachedContentForAllMods.TryGetValue(typeof(T), out IList cachedContent))
			return (IReadOnlyList<T>)cachedContent;

		var query = ModLoader.Mods.SelectMany(static m => m.GetContent<T>());

		if (!contentLoadingFinished) {
			// Content has not fully loaded yet, so we can't rely on the cache.
			// Return a lazy enumerable instead.
			return query;
		}

		// Construct the cache
		IReadOnlyList<T> content = query.ToList().AsReadOnly();
		_cachedContentForAllMods[typeof(T)] = (IList)content;
		return content;
	}

	private readonly Mod _mod;
	private readonly List<ILoadable> _content = new List<ILoadable>();
	private readonly Dictionary<Type, IList> _cachedContent = new();

	internal ContentCache(Mod mod) {
		_mod = mod;
	}

	internal void Add(ILoadable loadable) {
		_content.Add(loadable);
	}

	public IEnumerable<ILoadable> GetContent() => _content.AsReadOnly();  // Prevent exposing the list via hard cast

	public IEnumerable<T> GetContent<T>() where T : ILoadable {
		// Check of the cache already exists
		// It will already be a ReadOnlyList<T>, so it just needs to be cast back to it
		if (_cachedContent.TryGetValue(typeof(T), out IList cachedContent))
			return (IReadOnlyList<T>)cachedContent;

		if (_content.Count == 0) {
			// Mod has not loaded yet
			return Enumerable.Empty<T>();
		}

		var query = _content.OfType<T>();

		if (_mod.loading) {
			// Content may not have fully loaded yet, so we can't rely on the cache.
			// Return a lazy enumerable instead.
			return query;
		}

		// Construct the cache
		IReadOnlyList<T> content = query.ToList().AsReadOnly();
		_cachedContent[typeof(T)] = (IList)content;
		return content;
	}

	internal void Clear() {
		_content.Clear();
		_cachedContent.Clear();
	}
}
