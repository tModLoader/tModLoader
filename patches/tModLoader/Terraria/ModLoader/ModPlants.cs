using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.Enums;

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
	public abstract Asset<Texture2D> GetFruitTexture();
}

public abstract class ConvertibleTree : IPlant
{
	/// <summary>
	/// Used mostly for vanilla tree shake loot tables
	/// </summary>
	public TreeTypes CountsAsTreeType { get; }

	public int PlantTileId { get; }
	public int VanillaCount { get; }

	// Bulk Abstract requirements
	public int[] GrowsOnTileId { get; set; }
	public abstract void SetStaticDefaults();
	public abstract Asset<Texture2D> GetTexture();
	public abstract TreePaintingSettings TreeShaderSettings { get; }

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
	/// Return the type of gore created when the tree grow, being shook and falling leaves on windy days, returns -1 by default
	/// </summary>
	/// <returns></returns>
	public virtual int TreeLeaf()
	{
		return -1;
	}

	/// <summary>
	/// The ID of the item that is dropped in bulk when this tree is destroyed.
	/// </summary>
	/// <returns></returns>
	public abstract int DropWood();

	/// <summary>
	/// Defines the sapling that can eventually grow into a tree. The type of the sapling should be returned here. Returns 20 and style 0 by default.
	/// The style parameter will determine which sapling is chosen if multiple sapling types share the same ID;
	/// even if you only have a single sapling in an ID, you must still set this to 0.
	/// </summary>
	/// <param name="style"></param>
	/// <returns></returns>
	public virtual int SaplingGrowthType(ref int style)
	{
		style = CountsAsTreeType == TreeTypes.Palm ? 1 : 0;
		return TileID.Saplings;
	}

	/// <summary>
	/// Return the type of dust created when this tree is destroyed.
	/// Assigns the dust based on TreeTypes, using WoodFurniture by default.
	/// </summary>
	/// <returns></returns>
	public virtual int CreateDust()
	{
		return CountsAsTreeType switch {
			TreeTypes.Forest => DustID.WoodFurniture,
			TreeTypes.Palm => DustID.PalmWood,
			_ => DustID.WoodFurniture
		};
	}

	/// <summary>
	/// Return the texture containing the possible tree tops that can be drawn above this tree.
	/// </summary>
	/// <returns></returns>
	public abstract Asset<Texture2D> GetTopTextures();
}

/// <summary>
/// This class represents a type of modded tree.
/// The tree will share a tile ID with the vanilla trees (5), so that the trees can freely convert between each other if the soil below is converted.
/// This class encapsulates several functions that distinguish each type of tree from each other.
/// </summary>
public abstract class ModTree : ConvertibleTree
{
	// Default Properties of ModTree
	public const int VanillaTopTextureCount = 100;
	public const int VanillaStyleCount = 7;

	/// <summary>
	/// The tree will share a tile ID with the vanilla trees (5), so that the trees can freely convert between each other if the soil below is converted.
	/// </summary>
	public int PlantTileId => TileID.Trees;
	public int VanillaCount => VanillaStyleCount;
	public virtual TreeTypes CountsAsTreeType => TreeTypes.Forest;

	// Special Items for ModTree
	/// <summary>
	/// Whether or not this tree can drop acorns. Returns true by default.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanDropAcorn()
	{
		return true;
	}

	public abstract void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight);

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
public abstract class ModPalmTree : ConvertibleTree
{
	// Properties for ModPalmTree
	public const int VanillaStyleCount = 8;
	/// <summary>
	/// The tree will share a tile ID with the vanilla palm trees (323), so that the trees can freely convert between each other if the sand below is converted.
	/// </summary>
	public int PlantTileId => TileID.PalmTree;
	public int VanillaCount => VanillaStyleCount;
	public virtual TreeTypes CountsAsTreeType => TreeTypes.Palm;

	// Custom to ModPalmTree
	/// <summary>
	/// Return the texture containing the possible tree tops that can be drawn above this palm tree.
	/// </summary>
	/// <returns></returns>
	public abstract Asset<Texture2D> GetOasisTopTextures();
}
