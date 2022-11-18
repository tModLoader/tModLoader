using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Terraria.ModLoader.Engine;

internal static class EffectsTracker
{
	private abstract class KeyCache
	{
		public abstract void Reset();
		public static KeyCache Create<K, V>(IDictionary<K, V> dict) => new KeyCache<K, V>(dict);
	}

	private class KeyCache<K, V> : KeyCache
	{
		public IDictionary<K, V> dict;
		public HashSet<K> keys;

		public KeyCache(IDictionary<K, V> dict)
		{
			this.dict = dict;
			keys = new HashSet<K>(dict.Keys);
		}

		public override void Reset()
		{
			foreach (var k in dict.Keys.ToArray())
				if (!keys.Contains(k))
					dict.Remove(k);
		}
	}

	private static KeyCache[] KeyCaches;
	private static int vanillaArmorShaderCount;
	internal static int vanillaHairShaderCount;

	internal static void CacheVanillaState()
	{
		KeyCaches = new[] {
			KeyCache.Create(Filters.Scene._effects),
			KeyCache.Create(SkyManager.Instance._effects),
			KeyCache.Create(Overlays.Scene._effects),
			KeyCache.Create(Overlays.FilterFallback._effects),
			KeyCache.Create(GameShaders.Misc)
		};

		vanillaArmorShaderCount = GameShaders.Armor._shaderDataCount;
		vanillaHairShaderCount = GameShaders.Hair._shaderDataCount;
	}

	internal static void RemoveModEffects()
	{
		if (KeyCaches == null)
			return;

		foreach (var kc in KeyCaches)
			kc.Reset();
		KeyCaches = null;

		// bless Vanilla making identical classes with no parent
		ResetShaderDataSet(vanillaArmorShaderCount, ref GameShaders.Armor._shaderDataCount, ref GameShaders.Armor._shaderData, ref GameShaders.Armor._shaderLookupDictionary);
		ResetShaderDataSet(vanillaHairShaderCount, ref GameShaders.Hair._shaderDataCount, ref GameShaders.Hair._shaderData, ref GameShaders.Hair._shaderLookupDictionary);
	}

	// and of course, they have different byte/short/int fields for no discernable reason
	private static void ResetShaderDataSet<T, U, V>(int vanillaShaderCount, ref U shaderDataCount, ref List<T> shaderData, ref Dictionary<int, V> shaderLookupDictionary)
	{
		shaderDataCount = (U)Convert.ChangeType(vanillaShaderCount, typeof(U));
		shaderData.RemoveRange(vanillaShaderCount, shaderData.Count - vanillaShaderCount);

		var shaderLookupLimit = (V)Convert.ChangeType(vanillaShaderCount, typeof(V));
		foreach (var entry in shaderLookupDictionary.ToArray())
			if (Comparer<V>.Default.Compare(entry.Value, shaderLookupLimit) > 0)
				shaderLookupDictionary.Remove(entry.Key);
	}
}
