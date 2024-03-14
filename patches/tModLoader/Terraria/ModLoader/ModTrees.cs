using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Enums;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// This class represents a type of modded tree.
/// The tree will share a tile ID with the vanilla trees (5), so that the trees can freely convert between each other if the soil below is converted.
/// This class encapsulates several functions that distinguish each type of tree from each other.
/// </summary>
public abstract class ModTree : BaseTree
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
public abstract class ModPalmTree : BaseTree
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
