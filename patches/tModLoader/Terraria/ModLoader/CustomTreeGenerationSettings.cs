using System;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// CustomTrees growing settings
/// </summary>
public struct CustomTreeGenerationSettings
{
	/// <summary>
	/// Tree tile type
	/// </summary>
	public ushort TreeTileType;

	/// <summary>
	/// Check if ground tile type is valid to grow on
	/// </summary>
	public Func<int, bool> GroundTypeCheck;

	/// <summary>
	/// Check if behind wall type is valid to grow
	/// </summary>
	public Func<int, bool> WallTypeCheck;

	/// <summary>
	/// Minimum tree height
	/// </summary>
	public int MinHeight;

	/// <summary>
	/// Maximum tree height
	/// </summary>
	public int MaxHeight;

	/// <summary>
	/// Tree top leaves height in tiles (4 for vanilla)
	/// </summary>
	public int TopPaddingNeeded;

	/// <summary>
	/// 1 in X chance for not generating root
	/// </summary>
	public int NoRootChance;

	/// <summary>
	/// 1 in X chance for generating tile with less bark
	/// </summary>
	public int LessBarkChance;

	/// <summary>
	/// 1 in X chance for generating tile with more bark
	/// </summary>
	public int MoreBarkChance;

	/// <summary>
	/// 1 in X chance for generating a branch
	/// </summary>
	public int BranchChance;

	/// <summary>
	/// 1 in X chance that generated branch will be without leaves
	/// </summary>
	public int NotLeafyBranchChance;

	/// <summary>
	/// 1 in X chance that thee top will be broken
	/// </summary>
	public int BrokenTopChance;

	/// <summary/>
	public CustomTreeGenerationSettings(WorldGen.GrowTreeSettings vanillaSettings)
	{
		TreeTileType = vanillaSettings.TreeTileType;

		GroundTypeCheck = (t) => vanillaSettings.GroundTest(t);
		WallTypeCheck = (t) => vanillaSettings.WallTest(t);

		MinHeight = vanillaSettings.TreeHeightMin;
		MaxHeight = vanillaSettings.TreeHeightMax;

		TopPaddingNeeded = vanillaSettings.TreeTopPaddingNeeded;

		NoRootChance = 3;
		LessBarkChance = 7;
		MoreBarkChance = 7;
		BranchChance = 4;
		NotLeafyBranchChance = 3;
		BrokenTopChance = 13;
	}

	/// <summary>
	/// Settings for vanilla common tree
	/// </summary>
	public static CustomTreeGenerationSettings VanillaCommonTree = new() {
		TreeTileType = TileID.Trees,

		GroundTypeCheck = (t) => WorldGen.IsTileTypeFitForTree((ushort)t),
		WallTypeCheck = WorldGen.DefaultTreeWallTest,

		MinHeight = 5,
		MaxHeight = 17,

		TopPaddingNeeded = 4,
		BranchChance = 4,
		MoreBarkChance = 7,
		LessBarkChance = 7,
		NotLeafyBranchChance = 3,
		BrokenTopChance = 13,
		NoRootChance = 3,
	};
}
