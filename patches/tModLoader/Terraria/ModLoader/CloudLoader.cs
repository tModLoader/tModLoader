using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static class CloudLoader
{
	internal static readonly IDictionary<int, ModCloud> clouds = new Dictionary<int, ModCloud>();

	public static int CloudCount { get; private set; } = CloudID.Count;

	public static bool cloudLoaded = false;

	/// <summary> Registers a new cloud with the provided texture, spawn chance, and indication if the cloud belongs to the "rare cloud" category. Typically used with <see cref="SimpleModCloud"/> as TCloud for cloud with no additional logic.
	/// <para/> Use this to manually load a modded cloud rather than making a <see cref="ModCloud"/> class or autoloading the cloud through <see cref="Mod.CloudAutoloadingEnabled"/> logic.
	/// </summary>
	public static bool AddCloudFromTexture<TCloud>(Mod mod, string texture, float spawnChance = 1f, bool rareCloud = false) where TCloud : ModCloud, new()
	{
		if (mod == null)
			throw new ArgumentNullException(nameof(mod));

		if (texture == null)
			throw new ArgumentNullException(nameof(texture));

		if (!mod.loading)
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorNotLoading"));

		return mod.AddContent(new TCloud {
			nameOverride = Path.GetFileNameWithoutExtension(texture),
			textureOverride = texture,
			spawnChance = spawnChance,
			rareCloud = rareCloud
		});
	}

	/// <summary> Registers a new cloud with the provided texture, spawn chance, and indication if the cloud belongs to the "rare cloud" category.
	/// <para/> Use this to manually load a modded cloud rather than making a <see cref="ModCloud"/> class or autoloading the cloud through <see cref="Mod.CloudAutoloadingEnabled"/> logic.
	/// </summary>
	public static bool AddCloudFromTexture(Mod mod, string texture, float spawnChance = 1f, bool rareCloud = false) => AddCloudFromTexture<SimpleModCloud>(mod, texture, spawnChance, rareCloud);

	// Called by ModCloud.Register
	internal static void RegisterModCloud(ModCloud modCloud)
	{
		int id = CloudCount++;

		modCloud.Type = id;

		clouds[id] = modCloud;
	}

	internal static void AutoloadClouds(Mod mod)
	{
		foreach (string fullTexturePath in mod.RootContentSource.EnumerateAssets().Where(t => t.Contains("Clouds/"))) {
			string texturePath = Path.ChangeExtension(fullTexturePath, null);

			// ModCloud clouds will already be loaded at this point.
			if (!mod.TryFind<ModCloud>(Path.GetFileName(texturePath), out _)) {
				string textureKey = $"{mod.Name}/{texturePath}";

				AddCloudFromTexture<SimpleModCloud>(mod, textureKey);
			}
		}
	}

	public static int? ChooseCloud(float vanillaPool, bool rare)
	{
		if (!cloudLoaded)
			return 0;
		IDictionary<int, float> pool = new Dictionary<int, float>();
		pool[0] = vanillaPool;
		foreach (ModCloud cloud in clouds.Values) {
			if (rare != cloud.RareCloud)
				continue;
			float weight = cloud.SpawnChance();
			if (weight > 0f) {
				pool[cloud.Type] = weight;
			}
		}
		float totalWeight = 0f;
		foreach (int type in pool.Keys) {
			if (pool[type] < 0f) {
				pool[type] = 0f;
			}
			totalWeight += pool[type];
		}
		float choice = (float)Main.rand.NextDouble() * totalWeight;
		foreach (int type in pool.Keys) {
			float weight = pool[type];
			if (choice < weight) {
				return type;
			}
			choice -= weight;
		}
		return null;
	}

	internal static void ResizeAndFillArrays(bool unloading = false)
	{
		// Textures
		Array.Resize(ref TextureAssets.Cloud, CloudCount);

		// Sets
		LoaderUtils.ResetStaticMembers(typeof(CloudID));

		foreach (var pair in clouds) {
			TextureAssets.Cloud[pair.Key] = ModContent.Request<Texture2D>(pair.Value.Texture);
		}

		if (!unloading)
			cloudLoaded = true;
	}

	internal static void Unload()
	{
		cloudLoaded = false;

		CloudCount = CloudID.Count;

		clouds.Clear();

		Cloud.SwapOutModdedClouds();
	}

	internal static ModCloud GetModCloud(int type)
	{
		clouds.TryGetValue(type, out var modCloud);

		return modCloud;
	}
}
