using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

public struct TreeTileInfo
{
	/// <summary>
	/// Tree tile style, range is [0, 2]
	/// </summary>
	public int Style;

	/// <summary>
	/// Tile side, Left, Center or Right<br/>
	/// In case of any tile type with branches or roots Center means that there is branches or roots on both sides
	/// </summary>
	public TreeTileSide Side;

	/// <summary>
	/// Tree tile type
	/// </summary>
	public TreeTileType Type;

	/// <summary>
	/// True if tile is a leafy top or leafy branch
	/// </summary>
	public bool IsLeafy => Type == TreeTileType.LeafyBranch || Type == TreeTileType.LeafyTop;

	/// <summary>
	/// True if tile is not a leafy top or leafy branch
	/// </summary>
	public bool IsWoody => !IsLeafy;

	/// <summary>
	/// True if tile is a tree center tile (not root or branch)
	/// </summary>
	public bool IsCenter => Type != TreeTileType.Branch && Type != TreeTileType.LeafyBranch && Type != TreeTileType.Root;

	/// <summary>
	/// True if this tile can have branches
	/// </summary>
	public bool WithBranches => Type == TreeTileType.WithBranches || Type == TreeTileType.TopWithBranches;

	/// <summary>
	/// True if this tile can have roots
	/// </summary>
	public bool WithRoots => Type == TreeTileType.WithRoots || Type == TreeTileType.TopWithRoots;

	/// <summary>
	/// True if tile is any tree top
	/// </summary>
	public bool IsTop =>
		   Type == TreeTileType.Top
		|| Type == TreeTileType.TopWithBranches
		|| Type == TreeTileType.TopWithRoots
		|| Type == TreeTileType.BrokenTop
		|| Type == TreeTileType.LeafyTop;

	/// <summary/>
	public TreeTileInfo(int style, TreeTileSide side, TreeTileType type)
	{
		Style = style;
		Side = side;
		Type = type;
	}

	/// <summary/>
	public TreeTileInfo(TreeTileSide side, TreeTileType type)
	{
		Style = WorldGen.genRand.Next(3);
		Side = side;
		Type = type;
	}

	/// <summary/>
	public TreeTileInfo(TreeTileType type)
	{
		Style = WorldGen.genRand.Next(3);
		Side = TreeTileSide.Center;
		Type = type;
	}

	/// <summary>
	/// Gets the info about tree tile at given coordinates
	/// </summary>
	public static TreeTileInfo GetInfo(int x, int y) => GetInfo(Framing.GetTileSafely(x, y));

	/// <summary>
	/// Gets the info about given tree tile
	/// </summary>
	public static TreeTileInfo GetInfo(Tile t)
	{
		Point frame = new(t.TileFrameX, t.TileFrameY);

		int frameSize = 22;

		int style = (frame.Y & frameSize * 3) / frameSize % 3;
		frame.Y /= frameSize * 3;
		frame.X /= frameSize;

		switch (frame.X) {
			case 0:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Center, TreeTileType.Normal);
					case 1:
						return new(style, TreeTileSide.Left, TreeTileType.LessBark);
					case 2:
						return new(style, TreeTileSide.Right, TreeTileType.WithRoots);
					case 3:
						return new(style, TreeTileSide.Center, TreeTileType.BrokenTop);
				}
				break;

			case 1:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Right, TreeTileType.LessBark);
					case 1:
						return new(style, TreeTileSide.Right, TreeTileType.MoreBark);
					case 2:
						return new(style, TreeTileSide.Right, TreeTileType.Root);
					case 3:
						return new(style, TreeTileSide.Center, TreeTileType.LeafyTop);
				}
				break;

			case 2:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Right, TreeTileType.WithBranches);
					case 1:
						return new(style, TreeTileSide.Left, TreeTileType.MoreBark);
					case 2:
						return new(style, TreeTileSide.Left, TreeTileType.Root);
					case 3:
						return new(style, TreeTileSide.Left, TreeTileType.LeafyBranch);
				}
				break;

			case 3:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Left, TreeTileType.Branch);
					case 1:
						return new(style, TreeTileSide.Right, TreeTileType.WithBranches);
					case 2:
						return new(style, TreeTileSide.Left, TreeTileType.WithRoots);
					case 3:
						return new(style, TreeTileSide.Right, TreeTileType.LeafyBranch);
				}
				break;

			case 4:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Left, TreeTileType.WithBranches);
					case 1:
						return new(style, TreeTileSide.Right, TreeTileType.Branch);
					case 2:
						return new(style, TreeTileSide.Center, TreeTileType.WithRoots);
				}
				break;

			case 5:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Center, TreeTileType.Top);
					case 1:
						return new(style, TreeTileSide.Center, TreeTileType.WithBranches);
				}
				break;

			case 6:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Left, TreeTileType.TopWithBranches);
					case 1:
						return new(style, TreeTileSide.Right, TreeTileType.TopWithBranches);
					case 2:
						return new(style, TreeTileSide.Center, TreeTileType.TopWithBranches);
				}
				break;

			case 7:
				switch (frame.Y) {
					case 0:
						return new(style, TreeTileSide.Left, TreeTileType.TopWithRoots);
					case 1:
						return new(style, TreeTileSide.Right, TreeTileType.TopWithRoots);
					case 2:
						return new(style, TreeTileSide.Center, TreeTileType.TopWithRoots);
				}
				break;
		}

		return new(style, TreeTileSide.Center, TreeTileType.None);
	}

	/// <summary>
	/// Applies this info to tree tile at given coordinates
	/// </summary>
	public void ApplyToTile(int x, int y) => ApplyToTile(Framing.GetTileSafely(x, y));

	/// <summary>
	/// Applies this info to this tree tile
	/// </summary>
	public void ApplyToTile(Tile t)
	{
		Point frame = default;

		switch (Type) {
			case TreeTileType.LessBark:
				if (Side == TreeTileSide.Left)
					frame = new(0, 3);
				else
					frame = new(1, 0);
				break;

			case TreeTileType.Branch:
				if (Side == TreeTileSide.Left)
					frame = new(3, 0);
				else
					frame = new(4, 3);
				break;

			case TreeTileType.BrokenTop:
				frame = new(0, 9);
				break;

			case TreeTileType.MoreBark:
				if (Side == TreeTileSide.Left)
					frame = new(1, 3);
				else
					frame = new(2, 3);
				break;

			case TreeTileType.WithBranches:
				if (Side == TreeTileSide.Left)
					frame = new(4, 0);
				else if (Side == TreeTileSide.Right)
					frame = new(3, 3);
				else
					frame = new(5, 3);
				break;

			case TreeTileType.LeafyBranch:
				if (Side == TreeTileSide.Left)
					frame = new(2, 9);
				else
					frame = new(3, 9);
				break;

			case TreeTileType.WithRoots:
				if (Side == TreeTileSide.Left)
					frame = new(3, 6);
				else if (Side == TreeTileSide.Right)
					frame = new(0, 6);
				else
					frame = new(4, 6);
				break;

			case TreeTileType.Root:
				if (Side == TreeTileSide.Left)
					frame = new(2, 6);
				else
					frame = new(1, 6);
				break;

			case TreeTileType.Top:
				frame = new(5, 0);
				break;

			case TreeTileType.TopWithBranches:
				if (Side == TreeTileSide.Left)
					frame = new(6, 0);
				else if (Side == TreeTileSide.Right)
					frame = new(6, 3);
				else
					frame = new(6, 6);
				break;

			case TreeTileType.TopWithRoots:
				if (Side == TreeTileSide.Left)
					frame = new(7, 0);
				else if (Side == TreeTileSide.Right)
					frame = new(7, 3);
				else
					frame = new(7, 6);
				break;

			case TreeTileType.LeafyTop:
				frame = new(1, 9);
				break;
		}

		frame.Y += Style;

		int frameSize = 22;
		t.TileFrameX = (short)(frame.X * frameSize);
		t.TileFrameY = (short)(frame.Y * frameSize);
	}

	/// <summary/>
	public override string ToString()
	{
		return $"{Side} {Type} ({Style})";
	}

	/// <summary/>
	public override bool Equals(object obj)
	{
		return obj is TreeTileInfo info &&
			   Style == info.Style &&
			   Side == info.Side &&
			   Type == info.Type;
	}

	/// <summary/>
	public override int GetHashCode()
	{
		return HashCode.Combine(Style, Side, Type);
	}

	/// <summary/>
	public static bool operator ==(TreeTileInfo a, TreeTileInfo b)
	{
		if (a.Type != b.Type)
			return false;
		if (a.Side != b.Side)
			return false;
		return a.Style == b.Style;
	}

	/// <summary/>
	public static bool operator !=(TreeTileInfo a, TreeTileInfo b)
	{
		return a.Type != b.Type || a.Side != b.Side || a.Style != b.Style;
	}
}

/// <summary>
/// Tree tile with position and its info
/// </summary>
public record struct PositionedTreeTile(Point Pos, TreeTileInfo Info);

/// <summary/>
public enum TreeTileType
{
	/// <summary>
	/// Unspecified tree tile
	/// </summary>
	None,

	/// <summary>
	/// Straight tree tile
	/// </summary>
	Normal,

	/// <summary>
	/// Tile with less bark on <see cref="TreeTileInfo.Side"/> (Left/Right)
	/// </summary>
	LessBark,

	/// <summary>
	/// Tile with more bark on <see cref="TreeTileInfo.Side"/> (Left/Right)
	/// </summary>
	MoreBark,

	/// <summary>
	/// Tile with branches (All <see cref="TreeTileSide"/>s)
	/// </summary>
	WithBranches,

	/// <summary>
	/// Left or Right branch
	/// </summary>
	Branch,

	/// <summary>
	/// Left or Right branch with leaves
	/// </summary>
	LeafyBranch,

	/// <summary>
	/// Bottom tile with roots (All <see cref="TreeTileSide"/>s)
	/// </summary>
	WithRoots,

	/// <summary>
	/// Left or Right root
	/// </summary>
	Root,

	/// <summary>
	/// Cutted off top tile
	/// </summary>
	Top,

	/// <summary>
	/// Cutted off top with branches
	/// </summary>
	TopWithBranches,

	/// <summary>
	/// Cutted off top with roots
	/// </summary>
	TopWithRoots,

	/// <summary>
	/// Broken tree top
	/// </summary>
	BrokenTop,

	/// <summary>
	/// Tree top with leaves
	/// </summary>
	LeafyTop
}

/// <summary/>
public enum TreeTileSide
{
	/// <summary>
	/// If tile is left sided, or have branches or roots to the left
	/// </summary>
	Left,

	/// <summary>
	/// If tile have branches or roots at both sides, default value
	/// </summary>
	Center,

	/// <summary>
	/// If tile is right sided, or have branches or roots to the right
	/// </summary>
	Right
}
