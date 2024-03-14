using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;

public static class CustomTreeGenerator
{
	static bool prevLeftBranch = false;
	static bool prevRightBranch = false;

	/// <summary>
	/// Tries to grow a tree at the given coordinates with the given settings, returns true on success
	/// </summary>
	public static bool GrowTree(int x, int y, CustomTreeGenerationSettings settings)
	{
		prevLeftBranch = false;
		prevRightBranch = false;

		int groundY = y;
		while (TileID.Sets.TreeSapling[Main.tile[x, groundY].TileType] || TileID.Sets.CommonSapling[Main.tile[x, groundY].TileType]) {
			groundY++;
		}
		if (Main.tile[x - 1, groundY - 1].LiquidAmount != 0 || Main.tile[x, groundY - 1].LiquidAmount != 0 || Main.tile[x + 1, groundY - 1].LiquidAmount != 0) {
			return false;
		}
		Tile ground = Main.tile[x, groundY];
		if (!ground.HasUnactuatedTile || ground.IsHalfBlock || ground.Slope != SlopeType.Solid) {
			return false;
		}
		if (!settings.GroundTypeCheck(ground.TileType) || !settings.WallTypeCheck(Main.tile[x, groundY - 1].WallType)) {
			return false;
		}

		Tile groundLeft = Main.tile[x - 1, groundY];
		Tile groundRight = Main.tile[x + 1, groundY];
		if (
			   (!groundLeft.HasTile || !settings.GroundTypeCheck(groundLeft.TileType))
			&& (!groundRight.HasTile || !settings.GroundTypeCheck(groundRight.TileType))) {
			return false;
		}
		byte color = Main.tile[x, groundY].TileColor;
		int maxTreeHeight = settings.MinHeight;
		while (maxTreeHeight < settings.MaxHeight) {
			if (!WorldGen.EmptyTileCheck(x - 2, x + 2, groundY - maxTreeHeight - settings.TopPaddingNeeded, groundY - 1, 20)) {
				if (maxTreeHeight == settings.MinHeight)
					return false;
				break;
			}
			maxTreeHeight++;
		}

		int treeHeight = Math.Min(WorldGen.genRand.Next(settings.MinHeight, maxTreeHeight + 1), settings.MaxHeight);
		int treeBottom = groundY - 1;
		int treeTop = treeBottom - treeHeight;

		for (int i = treeBottom; i >= treeTop; i--) {
			if (i == treeBottom)
				PlaceBottom(x, i, color, settings, treeHeight == 1);
			else if (i > treeTop)
				PlaceMiddle(x, i, color, settings);
			else
				PlaceTop(x, i, color, settings);
		}

		WorldGen.RangeFrame(x - 2, treeTop - 1, x + 2, treeBottom + 1);
		if (Main.netMode == NetmodeID.Server) {
			NetMessage.SendTileSquare(-1, x - 1, treeTop, 3, treeHeight, TileChangeType.None);
		}

		return true;
	}

	/// <summary>
	/// Generates tree botoom part at the given coordinates
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <param name="color">Tile color</param>
	/// <param name="settings">Tree settings</param>
	/// <param name="top">True only if thee height is 1, places <see cref="TreeTileType.TopWithRoots"/> instead of <see cref="TreeTileType.WithRoots"/></param>
	public static void PlaceBottom(int x, int y, byte color, CustomTreeGenerationSettings settings, bool top)
	{
		Tile groundRight = Framing.GetTileSafely(x + 1, y + 1);
		Tile groundLeft = Framing.GetTileSafely(x - 1, y + 1);

		bool rootRight = !WorldGen.genRand.NextBool(settings.NoRootChance)
			&& groundRight.HasUnactuatedTile
			&& !groundRight.IsHalfBlock
			&& groundRight.Slope == SlopeType.Solid;

		bool rootLeft = !WorldGen.genRand.NextBool(settings.NoRootChance)
			&& groundLeft.HasUnactuatedTile
			&& !groundLeft.IsHalfBlock
			&& groundLeft.Slope == SlopeType.Solid;

		int style = WorldGen.genRand.Next(3);

		if (rootRight)
			Place(x + 1, y, new(style, TreeTileSide.Right, TreeTileType.Root), color, settings);
		if (rootLeft)
			Place(x - 1, y, new(style, TreeTileSide.Left, TreeTileType.Root), color, settings);

		if (rootLeft || rootRight)
			Place(x, y, new(style, GetSide(rootLeft, rootRight), top ? TreeTileType.TopWithRoots : TreeTileType.WithRoots), color, settings);
		else
			PlaceNormal(x, y, color, settings);
	}

	/// <summary>
	/// Places middle tree part
	/// </summary>
	public static void PlaceMiddle(int x, int y, byte color, CustomTreeGenerationSettings settings)
	{
		bool branchRight = WorldGen.genRand.NextBool(settings.BranchChance);
		bool branchLeft = WorldGen.genRand.NextBool(settings.BranchChance);

		int style = WorldGen.genRand.Next(3);

		if (prevLeftBranch && branchLeft)
			branchLeft = false;
		if (prevRightBranch && branchRight)
			branchRight = false;

		prevLeftBranch = branchLeft;
		prevRightBranch = branchRight;

		if (branchRight)
			Place(x + 1, y,
				new(style, TreeTileSide.Right,
				WorldGen.genRand.NextBool(settings.NotLeafyBranchChance) ? TreeTileType.Branch : TreeTileType.LeafyBranch),
				color, settings);
		if (branchLeft)
			Place(x - 1, y,
				new(style, TreeTileSide.Left,
				WorldGen.genRand.NextBool(settings.NotLeafyBranchChance) ? TreeTileType.Branch : TreeTileType.LeafyBranch),
				color, settings);

		if (branchRight || branchLeft)
			Place(x, y, new(style, GetSide(branchLeft, branchRight), TreeTileType.WithBranches), color, settings);
		else
			PlaceNormal(x, y, color, settings);
	}

	/// <summary>
	/// Places tree top
	/// </summary>
	public static void PlaceTop(int x, int y, byte color, CustomTreeGenerationSettings settings)
	{
		if (WorldGen.genRand.NextBool(settings.BrokenTopChance))
			Place(x, y, new(TreeTileType.BrokenTop), color, settings);
		else
			Place(x, y, new(TreeTileType.LeafyTop), color, settings);
	}

	/// <summary>
	/// Places straight tree tile
	/// </summary>
	public static void PlaceNormal(int x, int y, byte color, CustomTreeGenerationSettings settings)
	{
		int bark = 0;

		if (WorldGen.genRand.NextBool(settings.LessBarkChance))
			bark--;
		if (WorldGen.genRand.NextBool(settings.MoreBarkChance))
			bark++;

		TreeTileSide side = WorldGen.genRand.NextBool() ? TreeTileSide.Left : TreeTileSide.Right;

		if (bark == 0)
			Place(x, y, new(TreeTileType.Normal), color, settings);
		else if (bark < 0)
			Place(x, y, new(side, TreeTileType.LessBark), color, settings);
		else
			Place(x, y, new(side, TreeTileType.MoreBark), color, settings);
	}

	/// <summary>
	/// Places tree tile
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="info"></param>
	/// <param name="color">Tile color</param>
	/// <param name="settings"></param>
	/// <param name="triggerTileframe">True for triggering TileFrame afterwards</param>
	public static void Place(int x, int y, TreeTileInfo info, byte color, CustomTreeGenerationSettings settings, bool triggerTileframe = false)
	{
		Tile t = Main.tile[x, y];

		t.HasTile = true;
		t.TileType = settings.TreeTileType;
		info.ApplyToTile(t);
		t.TileColor = color;

		if (triggerTileframe) {
			WorldGen.TileFrame(x - 1, y, false, false);
			WorldGen.TileFrame(x + 1, y, false, false);
			WorldGen.TileFrame(x, y - 1, false, false);
			WorldGen.TileFrame(x, y + 1, false, false);
		}
	}

	/// <summary>
	/// Construct <see cref="TreeTileSide"/> from left and right
	/// </summary>
	public static TreeTileSide GetSide(bool left, bool right)
	{
		if (left && right || !left && !right)
			return TreeTileSide.Center;
		if (left)
			return TreeTileSide.Left;
		return TreeTileSide.Right;
	}

	/// <summary>
	/// Deconstruct <see cref="TreeTileSide"/>
	/// </summary>
	public static void SetSide(TreeTileSide side, out bool left, out bool right)
	{
		left = right = false;

		if (side == TreeTileSide.Center)
			left = right = true;
		else if (side == TreeTileSide.Left)
			left = true;
		else
			right = true;
	}

	/// <summary>
	/// Gets tree statistics at the given coordinates
	/// </summary>
	public static TreeStats GetTreeStats(int x, int y)
	{
		TreeStats stats = new();

		stats.Top = new(x, y);
		stats.Bottom = new(x, y);

		foreach (PositionedTreeTile tile in EnumerateTreeTiles(x, y)) {
			stats.TotalBlocks++;

			switch (tile.Info.Type) {
				case TreeTileType.Branch:
					stats.TotalBranches++;
					break;

				case TreeTileType.LeafyBranch:
					stats.TotalBranches++;
					stats.LeafyBranches++;
					break;

				case TreeTileType.Root:
					if (tile.Info.Side == TreeTileSide.Left)
						stats.LeftRoot = true;
					else
						stats.RightRoot = true;
					break;

				case TreeTileType.BrokenTop:
					stats.HasTop = true;
					stats.BrokenTop = true;
					break;

				case TreeTileType.LeafyTop:
					stats.HasTop = true;
					break;
			}
			if (tile.Info.IsCenter) {
				stats.Bottom.X = tile.Pos.X;
				stats.Top.X = tile.Pos.X;

				stats.Top.Y = Math.Min(tile.Pos.Y, stats.Top.Y);
				stats.Bottom.Y = Math.Max(tile.Pos.Y, stats.Bottom.Y);
			}
		}
		stats.GroundType = Framing.GetTileSafely(stats.Bottom.X, stats.Bottom.Y + 1).TileType;
		return stats;
	}

	/// <summary>
	/// Find and enumerate throug all tiles of this tree
	/// </summary>
	public static IEnumerable<PositionedTreeTile> EnumerateTreeTiles(int x, int y)
	{
		HashSet<Point> done = new();
		Queue<Point> queue = new();

		queue.Enqueue(new Point(x, y));

		while (queue.Count > 0) {
			Point p = queue.Dequeue();
			if (done.Contains(p))
				continue;

			done.Add(p);

			Tile t = Main.tile[p.X, p.Y];
			if (!t.HasTile || !TileID.Sets.IsATreeTrunk[t.TileType])
				continue;

			TreeTileInfo info = TreeTileInfo.GetInfo(t);

			yield return new(p, info);

			bool left = false;
			bool right = false;
			bool up = true;
			bool down = true;

			switch (info.Type) {
				case TreeTileType.WithBranches:
					SetSide(info.Side, out left, out right);
					break;

				case TreeTileType.Branch:
					up = down = false;
					SetSide(info.Side, out right, out left);
					break;

				case TreeTileType.LeafyBranch:
					up = down = false;
					SetSide(info.Side, out right, out left);
					break;

				case TreeTileType.WithRoots:
					down = false;
					SetSide(info.Side, out left, out right);
					break;

				case TreeTileType.Root:
					up = down = false;
					SetSide(info.Side, out right, out left);
					break;

				case TreeTileType.BrokenTop:
					up = false;
					break;

				case TreeTileType.LeafyTop:
					up = false;
					break;
			}

			if (up)
				queue.Enqueue(new(p.X, p.Y - 1));
			if (down)
				queue.Enqueue(new(p.X, p.Y + 1));
			if (left)
				queue.Enqueue(new(p.X - 1, p.Y));
			if (right)
				queue.Enqueue(new(p.X + 1, p.Y));
		}
	}

	/// <summary>
	/// CustomTree variant of <see cref="WorldGen.CheckTree"/>
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="settings">Tree settings</param>
	/// <param name="breakTiles">True to validate and break tree tiles, for example if nothing is below vertical tile</param>
	/// <param name="fixFrames">True to fix tile frames, for example change <see cref="TreeTileType.WithBranches"/> to <see cref="TreeTileType.Normal"/> when all branches are cut off from tile</param>
	public static void CheckTree(int x, int y, CustomTreeGenerationSettings settings, bool breakTiles = true, bool fixFrames = true)
	{
		Tile t = Framing.GetTileSafely(x, y);
		if (t.TileType != settings.TreeTileType)
			return;

		TreeTileInfo info = TreeTileInfo.GetInfo(x, y);

		if (breakTiles) {
			bool groundCheck = info.Type != TreeTileType.Branch && info.Type != TreeTileType.LeafyBranch;
			if (groundCheck) {
				Tile ground = Framing.GetTileSafely(x, y + 1);
				if (!ground.HasTile || ground.TileType != settings.TreeTileType && !settings.GroundTypeCheck(ground.TileType)) {
					WorldGen.KillTile(x, y);
					return;
				}
			}

			if (info.Type == TreeTileType.Branch || info.Type == TreeTileType.LeafyBranch || info.Type == TreeTileType.Root) {
				int checkX = x;
				if (info.Side == TreeTileSide.Left)
					checkX++;
				else if (info.Side == TreeTileSide.Right)
					checkX--;

				if (Framing.GetTileSafely(checkX, y).TileType != settings.TreeTileType) {
					WorldGen.KillTile(x, y);
					return;
				}
			}
		}

		if (fixFrames) {
			bool top = Framing.GetTileSafely(x, y - 1).TileType == settings.TreeTileType;
			bool left = Framing.GetTileSafely(x - 1, y).TileType == settings.TreeTileType;
			bool right = Framing.GetTileSafely(x + 1, y).TileType == settings.TreeTileType;

			TreeTileInfo leftInfo = TreeTileInfo.GetInfo(x - 1, y);
			TreeTileInfo rightInfo = TreeTileInfo.GetInfo(x + 1, y);

			bool leftBranch = left && leftInfo.Type == TreeTileType.Branch || leftInfo.Type == TreeTileType.LeafyBranch;
			bool rightBranch = right && rightInfo.Type == TreeTileType.Branch || rightInfo.Type == TreeTileType.LeafyBranch;

			bool leftRoot = left && leftInfo.Type == TreeTileType.Root;
			bool rightRoot = right && rightInfo.Type == TreeTileType.Root;

			TreeTileInfo newInfo = info;

			if (newInfo.IsTop || newInfo.Type == TreeTileType.Branch || newInfo.Type == TreeTileType.LeafyBranch || newInfo.Type == TreeTileType.Root) {
			}
			else if (leftBranch || rightBranch) {
				newInfo.Type = top ? TreeTileType.WithBranches : TreeTileType.TopWithBranches;
				newInfo.Side = GetSide(leftBranch, rightBranch);
			}
			else if (leftRoot || rightRoot) {
				newInfo.Type = top ? TreeTileType.WithRoots : TreeTileType.TopWithRoots;
				newInfo.Side = GetSide(leftRoot, rightRoot);
			}
			else if (!top) {
				newInfo.Type = TreeTileType.Top;
			}
			else if (
				   newInfo.Type != TreeTileType.Normal
				&& newInfo.Type != TreeTileType.LessBark
				&& newInfo.Type != TreeTileType.MoreBark) {
				int bark = 0;

				if (WorldGen.genRand.NextBool(settings.LessBarkChance))
					bark--;
				if (WorldGen.genRand.NextBool(settings.MoreBarkChance))
					bark++;

				newInfo.Side = bark == 0 ? TreeTileSide.Center : WorldGen.genRand.NextBool() ? TreeTileSide.Left : TreeTileSide.Right;

				if (bark == 0)
					newInfo.Type = TreeTileType.Normal;
				else if (bark < 0)
					newInfo.Type = TreeTileType.LessBark;
				else
					newInfo.Type = TreeTileType.MoreBark;
			}
			if (info != newInfo) {
				newInfo.ApplyToTile(x, y);

				WorldGen.TileFrame(x - 1, y, false, false);
				WorldGen.TileFrame(x + 1, y, false, false);
				WorldGen.TileFrame(x, y - 1, false, false);
				WorldGen.TileFrame(x, y + 1, false, false);
			}
		}
	}

	/// <summary>
	/// Changes the given coordinates to point to tree bottom
	/// </summary>
	public static void GetTreeBottom(ref int x, ref int y)
	{
		TreeTileInfo info = TreeTileInfo.GetInfo(x, y);

		if (info.Type == TreeTileType.Root || info.Type == TreeTileType.Branch || info.Type == TreeTileType.LeafyBranch) {
			if (info.Side == TreeTileSide.Left)
				x++;
			else if (info.Side == TreeTileSide.Right)
				x--;
		}

		Tile t = Framing.GetTileSafely(x, y);
		while (y < Main.maxTilesY - 50 && (!t.HasTile || TileID.Sets.IsATreeTrunk[t.TileType] || t.TileType == 72)) {
			y++;
			t = Framing.GetTileSafely(x, y);
		}
	}
}
