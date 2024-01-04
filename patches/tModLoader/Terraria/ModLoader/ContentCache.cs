﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Terraria.ModLoader;

/// <summary>
/// Container object for <see cref="ILoadable"/> instances added for a <see cref="Mod"/>
/// </summary>
internal class ContentCache
{
	private class Cache<T> where T : ILoadable {
		private static Dictionary<string, Cache<T>> _instancesPerMod;

		public static Cache<T> GetOrCreate(ContentCache cache) {
			if (_instancesPerMod is null) {
				_instancesPerMod = new();
				OnUnload += Unload;
			}

			string key = cache._mod.Name;

			if (!_instancesPerMod.TryGetValue(key, out Cache<T> instance))
				_instancesPerMod[key] = instance = new Cache<T>(cache);

			return instance;
		}

		private static void Unload() {
			_instancesPerMod = null;
		}

		private readonly ContentCache _source;
		private IReadOnlyList<T> _cache;

		private Cache(ContentCache source) {
			_source = source;
		}

		public IEnumerable<T> GetOrCacheContent() {
			IEnumerable<T> lazyContent = _source._content.OfType<T>();

			if (_source._mod.loading) {
				// Content may not have fully loaded yet, so we can't rely on the cache.
				// Return a lazy enumerable instead.
				return lazyContent;
			}

			// Populate the cache if it's not already populated
			if (_cache is null) {
				_cache = lazyContent.ToList().AsReadOnly();
				_source.OnClear += Clear;
			}

			return _cache;
		}

		private void Clear() {
			_cache = null;
		}
	}

	private static event Action OnUnload;

	private static readonly Dictionary<string, ContentCache> _sourcesByMod = new();

	internal static ContentCache GetOrCreate(Mod mod) {
		string key = mod.Name;

		if (!_sourcesByMod.TryGetValue(key, out ContentCache source))
			_sourcesByMod[key] = source = new ContentCache(mod);

		return source;
	}

	internal static void Unload() {
		_sourcesByMod.Clear();
		Interlocked.Exchange(ref OnUnload, null)?.Invoke();
	}

	private readonly Mod _mod;
	private readonly IList<ILoadable> _content = new List<ILoadable>();
	private event Action OnClear;

	private ContentCache(Mod mod) {
		_mod = mod;
	}

	internal void Add(ILoadable loadable) {
		_content.Add(loadable);
	}

	internal void Clear() {
		_content.Clear();
		Interlocked.Exchange(ref OnClear, null)?.Invoke();
	}

	public IEnumerable<ILoadable> GetContent() => _content;

	public IEnumerable<T> GetContent<T>() where T : ILoadable => Cache<T>.GetOrCreate(this).GetOrCacheContent();

	public IEnumerable<ILoadable> Reverse() => _content.Reverse();
}
