using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Enums;
using System;

namespace Terraria.ModLoader;

public interface IPlant : ILoadable
{
	public int PlantTileId { get; }
	public int VanillaCount { get; }
	public int[] GrowsOnTileId { get; set; }

	public abstract Asset<Texture2D> GetTexture();

	public abstract void SetStaticDefaults();

	void ILoadable.Load(Mod mod)
	{
		PlantLoader.plantList.Add(this);
	}

	void ILoadable.Unload() { }
}

public interface ITree : IPlant
{
	TreeTypes CountsAsTreeType { get; }

	int TreeLeaf();
	bool Shake(int x, int y, ref bool createLeaves);
}

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

	public static ITree GetTree(int type)
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

/// <summary>
/// This class represents a type of modded cactus.
/// This class encapsulates a function for retrieving the cactus's texture and an array for type of soil it grows on.
/// </summary>
public abstract class ModCactus : IPlant
{
	/// <summary>
	/// The cactus will share a tile ID with the vanilla cacti (80), so that the cacti can freely convert between each other if the sand below is converted.
	/// </summary>
	public int PlantTileId => TileID.Cactus;
	public int VanillaCount => 1;
	public int[] GrowsOnTileId { get; set; }
	public abstract void SetStaticDefaults();
	public abstract Asset<Texture2D> GetTexture();
	/// <summary> The fruit texture has a special layout that needs to be followed, see ExampleCactus_Fruit.png in ExampleMod for a template. </summary>
	public abstract Asset<Texture2D> GetFruitTexture();
}

/// <summary>
/// This class represents a type of modded tree.
/// The tree will share a tile ID with the vanilla trees (5), so that the trees can freely convert between each other if the soil below is converted.
/// This class encapsulates several functions that distinguish each type of tree from each other.
/// </summary>
public abstract class ModTree : ITree
{
	/// <summary>
	/// The tree will share a tile ID with the vanilla trees (5), so that the trees can freely convert between each other if the soil below is converted.
	/// </summary>
	public int PlantTileId => TileID.Trees;

	public const int VanillaStyleCount = 7;
	public int VanillaCount => VanillaStyleCount;
	public const int VanillaTopTextureCount = 100;

	public abstract TreePaintingSettings TreeShaderSettings { get; }

	public int[] GrowsOnTileId { get; set; }
	public abstract void SetStaticDefaults();
	public abstract Asset<Texture2D> GetTexture();

	/// <summary>
	/// <br>Used mostly for vanilla tree shake loot tables</br>
	/// <br>Defaults to <see cref="TreeTypes.Custom"/>. Set to <see cref="TreeTypes.None"/> to prevent the tree from being able to be shaken.</br>
	/// </summary>
	public virtual TreeTypes CountsAsTreeType => TreeTypes.Custom;

	/// <summary>
	/// Return the type of dust created when this tree is destroyed. Returns 7 by default.
	/// </summary>
	/// <returns></returns>
	public virtual int CreateDust()
	{
		return 7;
	}

	/// <summary>
	/// Return the type of gore created when the tree grow, being shook and falling leaves on windy days, returns -1 by default
	/// </summary>
	/// <returns></returns>
	public virtual int TreeLeaf()
	{
		return -1;
	}

	/// <summary>
	/// Executed on tree shake, return false to skip vanilla tree shake drops.<br/>
	/// The x and y coordinates correspond to the top of the tree, where items usually spawn.
	/// </summary>
	/// <returns></returns>
	public virtual bool Shake(int x, int y, ref bool createLeaves)
	{
		return true;
	}

	/// <summary>
	/// Whether or not this tree can drop acorns. Returns true by default.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanDropAcorn()
	{
		return true;
	}

	/// <summary>
	/// Defines the sapling that can eventually grow into a tree. The type of the sapling should be returned here. Returns 20 and style 0 by default.
	/// The style parameter will determine which sapling is chosen if multiple sapling types share the same ID;
	/// even if you only have a single sapling in an ID, you must still set this to 0.
	/// </summary>
	/// <param name="style"></param>
	/// <returns></returns>
	public virtual int SaplingGrowthType(ref int style)
	{
		style = 0;
		return 20;
	}

	/// <summary>
	/// The ID of the item that is dropped in bulk when this tree is destroyed.
	/// </summary>
	/// <returns></returns>
	public abstract int DropWood();

	/// <summary>
	/// Allows customization of various tree foliage drawing parameters. These control how leafy branches and the tree top are drawn at the <paramref name="i"/> and <paramref name="j"/> coordinates.
	/// <br/><br/> <paramref name="treeFrame"/> indicates which option for tree top or branch will be drawn. Hallow trees use this to provide many more options (various colors) than the default 3 options by adjusting based on <paramref name="i"/>.
	/// <br/><br/> <paramref name="floorY"/> is the y tile coordinate of the tile this tree is grown on.
	/// <br/><br/> <paramref name="topTextureFrameWidth"/> and <paramref name="topTextureFrameHeight"/> dictate the size of the sprite for the tree top. The tree top will be drawn centered above the top tile of the tree. These default to 80 and should be consistent between each tree top option. Branches are 40x40 and are not customizable.
	/// <br/><br/> <paramref name="xoffset"/> will be 0 for tree tops, 1 for a left branch, and -1 for a right branch. Can be combined with <paramref name="i"/> and <paramref name="floorY"/> to calculate the coordinates of the tile this tree is grown on.
	/// </summary>
	public virtual void SetTreeFoliageSettings(int i, int j, Tile tile, int xoffset, ref int treeFrame, int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
	{
	}

	[Obsolete("Use the updated hook signature")]
	public virtual void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
	{
	}

	/// <summary>
	/// Return the texture containing the possible tree tops that can be drawn above this tree.
	/// The framing was determined under <cref>SetTreeFoliageSettings</cref>
	/// </summary>
	public abstract Asset<Texture2D> GetTopTextures();

	/// <summary>
	/// Return the texture containing the possible tree branches that can be drawn next to this tree.
	/// The framing was determined under <cref>SetTreeFoliageSettings</cref>
	/// </summary>
	public abstract Asset<Texture2D> GetBranchTextures();
}

/// <summary>
/// This class represents a type of modded palm tree.
/// The palm tree will share a tile ID with the vanilla palm trees (323), so that the trees can freely convert between each other if the sand below is converted.
/// This class encapsulates several functions that distinguish each type of palm tree from each other.
/// </summary>
public abstract class ModPalmTree : ITree
{
	/// <summary>
	/// The tree will share a tile ID with the vanilla palm trees (323), so that the trees can freely convert between each other if the sand below is converted.
	/// </summary>
	public int PlantTileId => TileID.PalmTree;
	public int VanillaCount => VanillaStyleCount;
	public const int VanillaStyleCount = 8;

	public abstract TreePaintingSettings TreeShaderSettings { get; }

	public int[] GrowsOnTileId { get; set; }
	public abstract void SetStaticDefaults();
	public abstract Asset<Texture2D> GetTexture();

	/// <summary>
	/// <br>Used mostly for vanilla tree shake loot tables</br>
	/// <br>Defaults to <see cref="TreeTypes.Custom"/>. Set to <see cref="TreeTypes.None"/> to prevent the tree from being able to be shaken.</br>
	/// </summary>
	public virtual TreeTypes CountsAsTreeType => TreeTypes.Custom;

	/// <summary>
	/// Return the type of dust created when this palm tree is destroyed. Returns 215 by default.
	/// </summary>
	/// <returns></returns>
	public virtual int CreateDust()
	{
		return 215;
	}

	/// <summary>
	/// Return the type of gore created when the tree grow, being shook and falling leaves on windy days, returns -1 by default
	/// </summary>
	/// <returns></returns>
	public virtual int TreeLeaf()
	{
		return -1;
	}

	/// <summary>
	/// Executed on tree shake, return false to skip vanilla tree shake drops
	/// </summary>
	/// <returns></returns>
	public virtual bool Shake(int x, int y, ref bool createLeaves)
	{
		return true;
	}

	/// <summary>
	/// Defines the sapling that can eventually grow into a tree. The type of the sapling should be returned here. Returns 20 and style 0 by default.
	/// The style parameter will determine which sapling is chosen if multiple sapling types share the same ID;
	/// even if you only have a single sapling in an ID, you must still set this to 0.
	/// </summary>
	/// <param name="style"></param>
	/// <returns></returns>
	public virtual int SaplingGrowthType(ref int style)
	{
		style = 1;
		return 20;
	}

	/// <summary>
	/// The ID of the item that is dropped in bulk when this palm tree is destroyed.
	/// </summary>
	/// <returns></returns>
	public abstract int DropWood();

	/// <summary>
	/// Return the texture containing the possible tree tops that can be drawn above this palm tree.
	/// </summary>
	/// <returns></returns>
	public abstract Asset<Texture2D> GetTopTextures();

	/// <summary>
	/// Return the texture containing the possible tree tops that can be drawn above this palm tree.
	/// </summary>
	/// <returns></returns>
	public abstract Asset<Texture2D> GetOasisTopTextures();
}
