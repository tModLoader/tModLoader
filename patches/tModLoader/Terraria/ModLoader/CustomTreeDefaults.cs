using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Terraria.ModLoader;

[Autoload(false)]
public abstract class CustomTreeAcorn : ModItem
{
	private ModCustomTree tree = null;
	public ModCustomTree Tree { get => tree ?? CustomTreeLoader.GetByAcorn(Type); internal protected set => tree = value; }

	public override string Texture => Tree.AcornTexture;

	public LocalizedText DefaultDisplayNameLocalization => Tree.GetLocalization("AcornDisplayName", () => Tree.PrettyPrintName() + " Acorn");
	public LocalizedText DefaultTooltipLocalization => Tree.GetLocalization("AcornTooltip", () => "");

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(Tree.Sapling.Type);
	}
}

[Autoload(false)]
public abstract class CustomTreeLeaf : ModGore
{
	private ModCustomTree tree = null;
	public ModCustomTree Tree { get => tree ?? CustomTreeLoader.GetByCustomLeaf(Type); internal protected set => tree = value; }

	public override string Texture => Tree.LeafTexture ?? base.Texture;

	public override void SetStaticDefaults()
	{
		ChildSafety.SafeGore[Type] = true;
		GoreID.Sets.SpecialAI[Type] = 3;
		GoreID.Sets.PaintedFallingLeaf[Type] = true;
	}
}

[Autoload(false)]
public abstract class CustomTreeSapling : ModTile
{
	private ModCustomTree tree = null;
	public ModCustomTree Tree { get => tree ?? CustomTreeLoader.GetBySapling(Type); internal protected set => tree = value; }

	public override string Texture => Tree.SaplingTexture;

	public LocalizedText DefaultMapNameLocalization => Tree.GetLocalization("SaplingMapName", () => Tree.PrettyPrintName() + " Sapling");

	public override void SetStaticDefaults()
	{
		static bool HookCanPlace(ushort type, int x, int y, int style, int dir, int alternate)
		{
			ModCustomTree tree = CustomTreeLoader.GetBySapling(type);
			if (tree is null)
				return true;
			else
				return tree.ValidWallType(Framing.GetTileSafely(x, y).WallType) && tree.ValidGroundType(Framing.GetTileSafely(x, y + 1).TileType);
		}

		TileID.Sets.TreeSapling[Type] = true;
		TileID.Sets.CommonSapling[Type] = true;
		TileID.Sets.SwaysInWindBasic[Type] = true;

		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;

		TileObjectData.newTile.Width = 1;
		TileObjectData.newTile.Height = 2;
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
		TileObjectData.newTile.CoordinateWidth = 16;
		TileObjectData.newTile.CoordinatePadding = 2;
		TileObjectData.newTile.UsesCustomCanPlace = true;
		TileObjectData.newTile.HookCheckIfCanPlace = new((x, y, _, style, dir, alt) => HookCanPlace(Type, x, y, style, dir, alt) ? 1 : 0, 0, 0, true);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.DrawFlipHorizontal = true;
		TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.RandomStyleRange = Tree.SaplingStyles;
		TileObjectData.newTile.StyleMultiplier = 1;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);
	}

	public override bool TileFrame(int x, int y, ref bool resetFrame, ref bool noBreak)
	{
		WorldGen.Check1x2(x, y, Type);
		return false;
	}

	public override void RandomUpdate(int x, int y)
	{
		ModCustomTree tree = Tree;
		if (Main.rand.Next(tree.GrowChance) == 0)
			tree.Grow(x, y);
	}
}

[Autoload(false)]
public abstract class CustomTreeTile : ModTile
{
	private ModCustomTree tree = null;
	public ModCustomTree Tree { get => tree ?? CustomTreeLoader.GetByTile(Type); internal protected set => tree = value; }

	public override string Texture => Tree.TileTexture;

	public LocalizedText DefaultMapNameLocalization => Tree.GetLocalization("TileMapName", Tree.PrettyPrintName);

	public override void SetStaticDefaults()
	{
		Main.tileAxe[Type] = true;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.IsATreeTrunk[Type] = true;
		TileID.Sets.IsShakeable[Type] = true;
		TileID.Sets.GetsDestroyedForMeteors[Type] = true;
		TileID.Sets.GetsCheckedForLeaves[Type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
	}

	public override void SetDrawPositions(int x, int y, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
		width = 20;
		height = 20;
	}

	public override bool TileFrame(int x, int y, ref bool resetFrame, ref bool noBreak)
	{
		CustomTreeGenerator.CheckTree(x, y, Tree.GetTreeSettings());
		return false;
	}
}