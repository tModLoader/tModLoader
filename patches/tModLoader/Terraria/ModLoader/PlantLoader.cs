using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Enums;

namespace Terraria.ModLoader;

public static class PlantLoader
{
	internal static Dictionary<Vector2, IPlant> plantLookup = new Dictionary<Vector2, IPlant>();
	internal static List<IPlant> plantList = new List<IPlant>();
	internal static Dictionary<int, int> plantIdToStyleLimit = new Dictionary<int, int>();

	internal static void FinishSetup()
	{
		foreach (var plant in plantList) {
			plant.SetStaticDefaults();

			for (int i = 0; i < plant.GrowsOnTileId.Length; i++) {
				var id = new Vector2(plant.PlantTileId, plant.GrowsOnTileId[i]);

				if (plantLookup.TryGetValue(id, out var existing)) {
					Logging.tML.Error($"The new plant {plant.GetType()} conflicts with the existing plant {existing.GetType()}. New plant not added");
					continue;
				}

				if (!plantIdToStyleLimit.ContainsKey((int)id.X))
					plantIdToStyleLimit.Add((int)id.X, plant.VanillaCount);

				plantLookup.Add(id, plant);
			}
		}
	}

	internal static void UnloadPlants()
	{
		plantList.Clear();
		plantLookup.Clear();
	}

	public static T Get<T>(int plantTileID, int growsOnTileID) where T : IPlant
	{
		if (!plantLookup.TryGetValue(new Vector2(plantTileID, growsOnTileID), out IPlant plant))
			return default(T);

		return (T)plant;
	}

	public static bool Exists(int plantTileID, int growsOnTileID) => plantLookup.ContainsKey(new Vector2(plantTileID, growsOnTileID));

	public static Asset<Texture2D> GetCactusFruitTexture(int type)
	{
		var tree = Get<ModCactus>(TileID.Cactus, type);
		if (tree == null)
			return null;

		return tree.GetFruitTexture();
	}

	public static Asset<Texture2D> GetTexture(int plantId, int tileType)
	{
		var plant = Get<IPlant>(plantId, tileType);
		if (plant == null)
			return null;

		return plant.GetTexture();
	}

	public static BaseTree GetTree(int type)
	{
		var tree = Get<ModTree>(TileID.Trees, type);
		if (tree is not null)
			return tree;

		var palm = Get<ModPalmTree>(TileID.PalmTree, type);
		if (palm is not null)
			return palm;

		return null;
	}

	public static TreeTypes GetModTreeType(int type)
	{
		return GetTree(type)?.CountsAsTreeType ?? TreeTypes.None;
	}

	public static bool ShakeTree(int x, int y, int type, ref bool createLeaves)
	{
		return GetTree(type)?.Shake(x, y, ref createLeaves) ?? true;
	}

	public static void GetTreeLeaf(int type, ref int leafGoreType)
	{
		var tree = GetTree(type);
		if (tree is not null)
			leafGoreType = tree.TreeLeaf();
	}

	public static void CheckAndInjectModSapling(int x, int y, ref int tileToCreate, ref int previewPlaceStyle)
	{
		// Added by TML
		if (tileToCreate == TileID.Saplings) {
			Tile soil = Main.tile[x, y + 1];

			if (soil.active())
				TileLoader.SaplingGrowthType(soil.type, ref tileToCreate, ref previewPlaceStyle);
		}
	}
}