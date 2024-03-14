using System;
using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class CustomTreeLoader
{
	public const int TreeStyleBase = 1000;

	internal static Dictionary<ushort, ModCustomTree> byTileLookup = new();
	internal static Dictionary<ushort, ModCustomTree> bySaplingLookup = new();
	internal static Dictionary<int, ModCustomTree> byAcornLookup = new();
	internal static Dictionary<int, ModCustomTree> byCustomLeafLookup = new();

	internal static List<ModCustomTree> trees = new();

	internal static void Unload()
	{
		byTileLookup.Clear();
		bySaplingLookup.Clear();
		byAcornLookup.Clear();
		byCustomLeafLookup.Clear();
		trees.Clear();
	}

	internal static int NextTreeStyle()
	{
		return trees.Count + TreeStyleBase;
	}

	public static ModCustomTree GetByTile(ushort tile)
	{
		if (byTileLookup.TryGetValue(tile, out ModCustomTree tree))
			return tree;
		return null;
	}
    public static ModCustomTree GetBySapling(ushort sapling)
	{
		if (bySaplingLookup.TryGetValue(sapling, out ModCustomTree tree))
			return tree;
		return null;
	}
	public static ModCustomTree GetByAcorn(int acorn)
	{
		if (byAcornLookup.TryGetValue(acorn, out ModCustomTree tree))
			return tree;
		return null;
	}
	public static ModCustomTree GetByTreeStyle(int style)
	{
		if (style < TreeStyleBase)
			return null;
		style -= TreeStyleBase;
		if (style >= trees.Count)
			return null;

		return trees[style];
	}
	public static ModCustomTree GetByCustomLeaf(int leaf)
	{
		if (byCustomLeafLookup.TryGetValue(leaf, out ModCustomTree tree))
			return tree;
		return null;
	}

	public static bool TryGetByTile(ushort tile, out ModCustomTree tree)
	{
		return byTileLookup.TryGetValue(tile, out tree);
	}
    public static bool TryGetBySapling(ushort sapling, out ModCustomTree tree)
	{
		return bySaplingLookup.TryGetValue(sapling, out tree);
	}
	public static bool TryGetByAcorn(int acorn, out ModCustomTree tree)
	{
		return byAcornLookup.TryGetValue(acorn, out tree);
	}
	public static bool TryGetByTreeStyle(int style, out ModCustomTree tree)
	{
		tree = null;
		if (style < TreeStyleBase)
			return false;
		style -= TreeStyleBase;
		if (style >= trees.Count)
			return false;

		tree = trees[style];
		return true;
	}
	/// <summary>
	/// Gets tree foliage data for rendering
	/// </summary>
	/// <param name="x">World X coordinate</param>
	/// <param name="y">World Y coordinate</param>
	/// <param name="xoffset"></param>
	/// <param name="treeFrame">Tree frame (0, 1, 2)</param>
	/// <param name="treeStyle">Tree texture index</param>
	/// <param name="floorY"></param>
	/// <param name="topTextureFrameWidth">Tree top width</param>
	/// <param name="topTextureFrameHeight">Tree top height</param>
	/// <returns></returns>
	public static bool GetTreeFoliageData(int x, int y, int xoffset, ref int treeFrame, ref int treeStyle, out int floorY, out int topTextureFrameWidth, out int topTextureFrameHeight)
	{
		Tile tile = Main.tile[x, y];

		floorY = 0;
		topTextureFrameHeight = 0;
		topTextureFrameWidth = 0;

		if (!TryGetByTile(tile.TileType, out ModCustomTree tree))
			return false;

		treeStyle = tree.TreeStyle;
		return tree.GetTreeFoliageData(x, y, xoffset, ref treeFrame, out floorY, out topTextureFrameWidth, out topTextureFrameHeight);
	}

	/// <summary>
	/// Gets tree style (texture index)
	/// </summary>
	/// <param name="x">World X coordinate</param>
	/// <param name="y">World Y coordinate</param>
	/// <param name="style"></param>
	public static void GetStyle(int x, int y, ref int style)
	{
		Tile tile = Main.tile[x, y];
		if (!TryGetByTile(tile.TileType, out ModCustomTree tree))
			return;

		style = tree.GetStyle(x, y);
	}

	/// <summary>
	/// Called when tree was shook
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="createLeaves"></param>
	/// <returns></returns>
	public static bool Shake(int x, int y, ref bool createLeaves)
	{
		Tile tile = Main.tile[x, y];
		if (!TryGetByTile(tile.TileType, out ModCustomTree tree))
			return true;

		return tree.Shake(x, y, ref createLeaves);
	}

	/// <summary>
	/// Sets passStyle to tree leaf gore id
	/// </summary>
	/// <param name="x"></param>
	/// <param name="topTile"></param>
	/// <param name="t"></param>
	/// <param name="treeHeight"></param>
	/// <param name="treeFrame"></param>
	/// <param name="passStyle"></param>
	public static void GetTreeLeaf(int x, Tile topTile, Tile t, ref int treeHeight, ref int treeFrame, ref int passStyle)
	{
		if (!TryGetByTile(topTile.TileType, out ModCustomTree tree))
			return;

		tree.GetTreeLeaf(x, topTile, t, ref treeHeight, ref treeFrame, out passStyle);
	}

	/// <summary>
	/// Tries to grow tree by tile type
	/// </summary>
	/// <param name="tileType"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public static bool Grow(ushort tileType, int x, int y)
	{
		if (!TryGetByTile(tileType, out ModCustomTree tree))
			return false;

		return tree.Grow(x, y);
	}

	/// <summary>
	/// Tries to grow tree by sapling tile type
	/// </summary>
	/// <param name="tileType"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public static bool GrowFromSapling(ushort tileType, int x, int y)
	{
		if (!TryGetBySapling(tileType, out ModCustomTree tree))
			return false;

		return tree.Grow(x, y);
	}

	/// <summary>
	/// Tries to generate any ModCustomTree at given coordinates
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public static bool TryGenerate(int x, int y)
	{
		if (trees.Count == 0)
			return false;

		int tileType = Framing.GetTileSafely(x, y).TileType;

		int weight = 0;
		foreach (ModCustomTree tree in trees)
			if (tree.ValidGroundType(tileType))
				weight += Math.Abs(tree.GenerationWeight);

		if (weight <= 0)
			return false;

		int chosenWeight = WorldGen.genRand.Next(weight);
		foreach (ModCustomTree tree in trees) {
			if (!tree.ValidGroundType(tileType))
				continue;

			chosenWeight -= Math.Abs(tree.GenerationWeight);
			if (chosenWeight > 0)
				continue;

			if (WorldGen.genRand.Next(tree.GenerationChance) == 0) {
				return tree.TryGenerate(x, y);
			}

			return false;
		}
		return false;
	}

	/// <summary>
	/// Returns true if given acorn item type can grow on given ground
	/// </summary>
	/// <param name="acornItemType"></param>
	/// <param name="groundTileType"></param>
	/// <returns></returns>
	public static bool CanGrowAcorn(int acornItemType, ushort groundTileType) {
		if (!TryGetByAcorn(acornItemType, out ModCustomTree tree))
			return false;

		return tree.ValidGroundType(groundTileType);
	}
}
