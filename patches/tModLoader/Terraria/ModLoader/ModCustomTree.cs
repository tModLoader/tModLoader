using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader;

public static class CustomTreeLoader
{
	internal static Dictionary<ushort, ModCustomTree> byTileLookup = new();
	internal static Dictionary<ushort, ModCustomTree> bySaplingLookup = new();
	internal static Dictionary<int, ModCustomTree> byAcornLookup = new();
	internal static List<ModCustomTree> trees = new();

	internal static void Unload()
	{
		byTileLookup.Clear();
		bySaplingLookup.Clear();
		byAcornLookup.Clear();
		trees.Clear();
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
}

public abstract class ModCustomTree : ModType
{
	public virtual string TileTexture => (GetType().Namespace + "." + Name + "_Tile").Replace('.', '/');
	public virtual string SaplingTexture => (GetType().Namespace + "." + Name + "_Sapling").Replace('.', '/');
	public virtual string AcornTexture => (GetType().Namespace + "." + Name + "_Acorn").Replace('.', '/');
	public virtual string TopTexture => (GetType().Namespace + "." + Name + "_Top").Replace('.', '/');
	public virtual string BranchTexture => (GetType().Namespace + "." + Name + "_Branch").Replace('.', '/');

	public ushort TileType { get; internal set; }
	public ushort SaplingType { get; internal set; }
	public int AcornType { get; internal set; }

	public virtual ushort LoadTile()
	{
		CustomTreeDefaultTile tile = new();
		Mod.AddContent(tile);
		return tile.Type;
	}

	public virtual ushort LoadSapling()
	{
		CustomTreeDefaultSapling sapling = new();
		Mod.AddContent(sapling);
		return sapling.Type;
	}

	public virtual int LoadAcorn()
	{
		CustomTreeDefaultAcorn acorn = new();
		Mod.AddContent(acorn);
		return acorn.Type;
	}

	protected override void Register()
	{
		TileType = LoadTile();
		CustomTreeLoader.byTileLookup[TileType] = this;

		SaplingType = LoadSapling();
		CustomTreeLoader.bySaplingLookup[SaplingType] = this;

		AcornType = LoadAcorn();
		CustomTreeLoader.byAcornLookup[AcornType] = this;

		CustomTreeLoader.trees.Add(this);
	}
}

public static class CustomTreeDefaults
{
	public static void SetTileStaticDefaults(ushort type)
	{
		Main.tileAxe[type] = true;
		Main.tileFrameImportant[type] = true;

		TileID.Sets.IsATreeTrunk[type] = true;
		TileID.Sets.IsShakeable[type] = true;
		TileID.Sets.GetsDestroyedForMeteors[type] = true;
		TileID.Sets.GetsCheckedForLeaves[type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[type] = true;
		TileID.Sets.PreventsTileReplaceIfOnTopOfIt[type] = true;
	}

	public static void SetSaplingStaticDefaults(ushort type, int styleCount)
	{
		static bool HookCanPlace(int x, int y, int style, int dir, int alternate)
		{
			// TODO
			//return Tree.ValidWallType(Framing.GetTileSafely(x, y).WallType) && Tree.ValidGroundType(Framing.GetTileSafely(x, y + 1).TileType);
			return true;
		}

		TileID.Sets.CommonSapling[type] = true;
		TileID.Sets.SwaysInWindBasic[type] = true;

		Main.tileFrameImportant[type] = true;
		Main.tileNoAttach[type] = true;

		TileObjectData.newTile.Width = 1;
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.HookCheckIfCanPlace = new((x, y, type, style, dir, alt) => HookCanPlace(x, y, style, dir, alt) ? 1 : 0, 0, 0, true);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.DrawFlipHorizontal = true;
		TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.RandomStyleRange = styleCount;
		TileObjectData.newTile.StyleMultiplier = 1;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(type);
	}
}

[Autoload(false)]
internal class CustomTreeDefaultTile : ModTile
{
	// Should be always valid, no checks needed
	private ModCustomTree Tree => CustomTreeLoader.GetByTile(Type);

	public override string Texture => Tree.TileTexture;

	public override void SetStaticDefaults()
	{
		CustomTreeDefaults.SetTileStaticDefaults(Type);
	}

	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
		width = 20;
		height = 20;
		tileFrameX = (short)(tileFrameX / 18 * 22);
		tileFrameY = (short)(tileFrameY / 18 * 22);
	}
}

[Autoload(false)]
internal class CustomTreeDefaultSapling : ModTile
{
	// Should be always valid, no checks needed
	private ModCustomTree Tree => CustomTreeLoader.GetBySapling(Type);

	public override string Texture => Tree.SaplingTexture;

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		WorldGen.Check1x2(i, j, Type);
		return false;
	}

	public override void SetStaticDefaults()
	{
		CustomTreeDefaults.SetSaplingStaticDefaults(Type, /* TODO */ 1);
	}
}

[Autoload(false)]
internal class CustomTreeDefaultAcorn : ModItem
{
	// Should be always valid, no checks needed
	private ModCustomTree Tree => CustomTreeLoader.GetByAcorn(Type);

	public override string Texture => Tree.AcornTexture;

	public override void SetStaticDefaults()
	{
		// TODO: display name
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(Tree.SaplingType, 0);
	}
}