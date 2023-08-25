using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Terraria.ModLoader;

public abstract class ModCustomTree : ModType, ILocalizedModType
{
	/// <summary>
	/// Texture path for tree tile
	/// </summary>
	public virtual string TileTexture => (GetType().Namespace + "." + Name + "_Tile").Replace('.', '/');

	/// <summary>
	/// Texture path for tree sapling tile.<br/>
	/// Can contain multiple styles of saplings, style count is set by <see cref="SaplingStyles"/>
	/// </summary>
	public virtual string SaplingTexture => (GetType().Namespace + "." + Name + "_Sapling").Replace('.', '/');

	/// <summary>
	/// Texture path for tree acorn item
	/// </summary>
	public virtual string AcornTexture => (GetType().Namespace + "." + Name + "_Acorn").Replace('.', '/');

	/// <summary>
	/// Texture path for tree top<br/>
	/// Must contain 3 variants of tops<br/>
	/// Sprite size is defined in <see cref="GetTreeFoliageData"/>
	/// </summary>
	public virtual string TopTexture => (GetType().Namespace + "." + Name + "_Top").Replace('.', '/');

	/// <summary>
	/// Texture path for tree top<br/>
	/// Must contain 3 variants of branches on both sides<br/>
	/// </summary>
	public virtual string BranchTexture => (GetType().Namespace + "." + Name + "_Branch").Replace('.', '/');

	/// <summary>
	/// If this is provided, new leaf ModGore will be registered and used as LeafType
	/// </summary>
	public virtual string LeafTexture => null;

	/// <summary>
	/// Tree style type, unique for each tree
	/// </summary>
	public int TreeStyle { get; internal set; }

	/// <summary>
	/// Tree tile type
	/// </summary>
	public ushort TileType { get; internal set; }

	/// <summary>
	/// Tree sapling type
	/// </summary>
	public ushort SaplingType { get; internal set; }

	/// <summary>
	/// Tree acorn item type
	/// </summary>
	public int AcornType { get; internal set; }

	/// <summary>
	/// Tree leaf type
	/// </summary>
	public int LeafType { get; internal set; }

	/// <summary>
	/// Tree shader settings for painting
	/// </summary>
	public TreePaintingSettings PaintingSettings = new TreePaintingSettings {
		UseSpecialGroups = true,
		SpecialGroupMinimalHueValue = 1f / 6f,
		SpecialGroupMaximumHueValue = 5f / 6f,
		SpecialGroupMinimumSaturationValue = 0f,
		SpecialGroupMaximumSaturationValue = 1f
	};

	/// <summary>
	/// Tree type for vanilla tree shake system
	/// </summary>
	public virtual TreeTypes TreeType { get; set; } = TreeTypes.Forest;

	/// <summary>
	/// Used for sapling and tree ground tile type checks
	/// </summary>
	public virtual int[] ValidGroundTiles { get; } = new int[] { 2 };

	/// <summary>
	/// Used for sapling wall type checks
	/// </summary>
	public virtual int[] ValidWalls { get; } = new int[] { 0 };

	/// <summary>
	/// 1 in X chance of tree growing from sapling per random tick
	/// </summary>
	public virtual int GrowChance { get; set; } = 5;

	/// <summary>
	/// Generation weight among other custom trees
	/// </summary>
	public virtual int GenerationWeight { get; set; } = 10;

	/// <summary>
	/// Chance that this tree will try to generate if chosen by weight
	/// </summary>
	public virtual int GenerationChance { get; set; } = 5;

	/// <summary>
	/// 1 in X chance of not generating roots
	/// </summary>
	public virtual int NoRootChance { get; set; } = 3;

	/// <summary>
	/// 1 in X chance of generating more bark on tile
	/// </summary>
	public virtual int MoreBarkChance { get; set; } = 7;

	/// <summary>
	/// 1 in X chance of generating less bark on tile
	/// </summary>
	public virtual int LessBarkChance { get; set; } = 7;

	/// <summary>
	/// 1 in X chance of generating branch
	/// </summary>
	public virtual int BranchChance { get; set; } = 4;

	/// <summary>
	/// 1 in X chance that generated branch will not have leaves
	/// </summary>
	public virtual int NotLeafyBranchChance { get; set; } = 3;

	/// <summary>
	/// 1 in X chance that generated top will be broken
	/// </summary>
	public virtual int BrokenTopChance { get; set; } = 13;

	/// <summary>
	/// Minimum tree height for growing from sapling (not growing over time)
	/// </summary>
	public virtual int MinHeight { get; set; } = 5;

	/// <summary>
	/// Maximum tree height for growing from sapling (not growing over time)
	/// </summary>
	public virtual int MaxHeight { get; set; } = 12;

	/// <summary>
	/// How many tiles after tree top tile are additionally checked if empty
	/// </summary>
	public virtual int TopPadding { get; set; } = 4;

	/// <summary>
	/// How many styles sapling tile has
	/// </summary>
	public virtual int SaplingStyles { get; set; } = 1;

	/// <summary>
	/// Tree tile map color
	/// </summary>
	public virtual Color? TileMapColor { get; set; } = null;

	/// <summary>
	/// Tree tile map name
	/// Defaults to localization key of {ModName}.CustomTrees.{TreeName}.TileMapName
	/// </summary>
	public virtual LocalizedText TileMapName { get; set; } = null;

	/// <summary>
	/// Sapling tile map color
	/// </summary>
	public virtual Color? SaplingMapColor { get; set; } = null;

	/// <summary>
	/// Sapling tile map name
	/// Defaults to localization key of {ModName}.CustomTrees.{TreeName}.SaplingMapName
	/// </summary>
	public virtual LocalizedText SaplingMapName { get; set; } = null;

	/// <summary>
	/// Default name for acorn item
	/// Defaults to localization key of {ModName}.CustomTrees.{TreeName}.AcornDisplayName
	/// </summary>
	public virtual LocalizedText AcornDisplayName { get; set; } = null;

	/// <summary>
	/// Tooltip for acorn item
	/// Defaults to localization key of {ModName}.CustomTrees.{TreeName}.AcornTooltip
	/// </summary>
	public virtual LocalizedText AcornTooltip { get; set; } = null;

	public string LocalizationCategory => "CustomTrees";

	private List<Asset<Texture2D>> FoliageTextureCache = new();

	/// <summary>
	/// Override for loading custom tile for this tree
	/// </summary>
	/// <returns></returns>
	public virtual ushort LoadTile()
	{
		CustomTreeDefaultTile tile = new(this);
		Mod.AddContent(tile);
		return tile.Type;
	}

	/// <summary>
	/// Override for loading custom sapling tile for this tree
	/// </summary>
	/// <returns></returns>
	public virtual ushort LoadSapling()
	{
		CustomTreeDefaultSapling sapling = new(this);
		Mod.AddContent(sapling);
		return sapling.Type;
	}

	/// <summary>
	/// Override for loading custom acorn item for this tree
	/// </summary>
	/// <returns></returns>
	public virtual int LoadAcorn()
	{
		CustomTreeDefaultAcorn acorn = new(this);
		Mod.AddContent(acorn);
		return acorn.Type;
	}

	/// <summary>
	/// Override for loading custom leaf gore for this tree
	/// </summary>
	/// <returns></returns>
	public virtual int LoadLeaf()
	{
		if (LeafTexture is null)
			return GoreID.TreeLeaf_Normal;

		CustomTreeDefaultLeaf leaf = new(this);
		Mod.AddContent(leaf);
		return leaf.Type;
	}

	/// <summary>
	/// Ran for each tile's <see cref="ModTile.TileFrame"/><br/>
	/// Called from tree's ModTile. You'll need to call it manually when overriding tree tile with <see cref="LoadTile"/>.
	/// </summary>
	public virtual void TileFrame(int x, int y)
	{
		CustomTreeGenerator.CheckTree(x, y, GetTreeSettings());
	}

	/// <summary>
	/// Called when tree tries to grow from sapling<br/>
	/// Called from tree's sapling ModTile on <see cref="ModBlockType.RandomUpdate"/>. You'll need to call it manually when overriding tree tile with <see cref="LoadSapling"/>.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public virtual bool Grow(int x, int y)
	{
		if (CustomTreeGenerator.GrowTree(x, y, GetTreeSettings()) && WorldGen.PlayerLOS(x, y))
		{
			WorldGen.TreeGrowFXCheck(x, y);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Called from tree's ModTile's <see cref="ModBlockType.SetStaticDefaults"/>. You'll need to call it manually when overriding tree tile with <see cref="LoadTile"/>.<br/>
	/// Static values default to values from <see cref="CustomTreeDefaults.SetTileStaticDefaults"/>, so:
	/// <code>
	/// Main.tileAxe[type] = true;
	/// Main.tileFrameImportant[type] = true;
	/// TileID.Sets.IsATreeTrunk[type] = true;
	/// TileID.Sets.IsShakeable[type] = true;
	/// TileID.Sets.GetsDestroyedForMeteors[type] = true;
	/// TileID.Sets.GetsCheckedForLeaves[type] = true;
	/// TileID.Sets.PreventsTileRemovalIfOnTopOfIt[type] = true;
	/// TileID.Sets.PreventsTileReplaceIfOnTopOfIt[type] = true;
	/// </code>
	/// </summary>
	/// <param name="tile"></param>
	/// <returns></returns>
	public virtual void SetTileStaticDefaults(ModTile tile) { }

	/// <summary>
	/// Gets current tree settings
	/// </summary>
	public CustomTreeGenerationSettings GetTreeSettings()
	{
		return new() {
			MaxHeight = MaxHeight,
			MinHeight = MinHeight,

			TreeTileType = TileType,

			GroundTypeCheck = ValidGroundType,
			WallTypeCheck = ValidWallType,

			TopPaddingNeeded = TopPadding,

			BranchChance = BranchChance,
			BrokenTopChance = BrokenTopChance,
			LessBarkChance = LessBarkChance,
			MoreBarkChance = MoreBarkChance,
			NotLeafyBranchChance = NotLeafyBranchChance,
			NoRootChance = NoRootChance,
		};
	}

	/// <summary>
	/// Return true if tile type is valid for tree to grow on
	/// </summary>
	public virtual bool ValidGroundType(int tile) => ValidGroundTiles.Contains(tile);

	/// <summary>
	/// Return true if wall type is valid for tree
	/// </summary>
	public virtual bool ValidWallType(int tile) => ValidWalls.Contains(tile);

	/// <summary>
	/// Randomly executed on every tree tile<br/>
	/// Called from tree's ModTile. You'll need to call it manually when overriding tree tile with <see cref="LoadTile"/>.
	/// </summary>
	public virtual void RandomUpdate(int x, int y) { }

	/// <summary>
	/// Same as <see cref="ModTile.GetItemDrops"/>.
	/// Use TreeTileInfo.GetInfo to determine tile type.<br/>
	/// Called from tree's ModTile. You'll need to call it manually when overriding tree tile with <see cref="LoadTile"/>.
	/// </summary>
	public virtual IEnumerable<Item> GetItemDrops(int x, int y) => Array.Empty<Item>();

	/// <summary>
	/// This is executed when tree is being shook, return true to continue vanilla code for tree shaking
	/// </summary>
	public virtual bool Shake(int x, int y, ref bool createLeaves) => true;

	/// <summary>
	/// Gets tree leaf gore id and tree height for falling leaves
	/// </summary>
	public virtual void GetTreeLeaf(int x, Tile topTile, Tile t, ref int treeHeight, ref int treeFrame, out int passStyle)
	{
		passStyle = LeafType;
	}

	/// <summary>
	/// Woks the same as ModTile.CreateDust<br/>
	/// Allows to change dust type created or to disable tile dust<br/>
	/// Called from tree's ModTile. You'll need to call it manually when overriding tree tile with <see cref="LoadTile"/>.
	/// </summary>
	/// <param name="x">Tile X position</param>
	/// <param name="y">Tile Y position</param>
	/// <param name="dustType">Tile dust type</param>
	/// <returns>False to stop dust from creating</returns>
	public virtual bool CreateDust(int x, int y, ref int dustType)
	{
		return true;
	}

	/// <summary>
	/// Called in world generaion process
	/// Return true if tree was generated
	/// </summary>
	/// <param name="x">Ground tile X position</param>
	/// <param name="y">Ground tile Y position</param>
	/// <returns></returns>
	public virtual bool TryGenerate(int x, int y)
	{
		return false;
	}

	public virtual int GetStyle(int x, int y) => 0;

	/// <summary>
	/// Gets texture coordinates data for custom top texture
	/// </summary>
	public virtual bool GetTreeFoliageData(int x, int y, int xoffset, ref int treeFrame, out int floorY, out int topTextureFrameWidth, out int topTextureFrameHeight)
	{
		int v = 0;
		return WorldGen.GetCommonTreeFoliageData(x, y, xoffset, ref treeFrame, ref v, out floorY, out topTextureFrameWidth, out topTextureFrameHeight);
	}

	/// <summary>
	/// Gets foliage assets for tree tops and branches for each texture style<br/>
	/// Texture style is received from <see cref="GetStyle"/><br/>
	/// This method is called only once for each style
	/// </summary>
	/// <param name="style"></param>
	/// <param name="branch"></param>
	/// <returns></returns>
	public virtual Asset<Texture2D> GetFoliageTexture(int style, bool branch)
	{
		if (branch)
			return ModContent.Request<Texture2D>(BranchTexture);
		else
			return ModContent.Request<Texture2D>(TopTexture);
	}

	public Asset<Texture2D> GetFoliageTextureCached(int style, bool branch)
	{
		int cacheId = style * 2 + (branch ? 1 : 0);
		while (cacheId >= FoliageTextureCache.Count)
			FoliageTextureCache.Add(null);

		if (FoliageTextureCache[cacheId] is null) {
			FoliageTextureCache[cacheId] = GetFoliageTexture(style, branch);
		}

		return FoliageTextureCache[cacheId];
	}

	protected override void Register()
	{
		TreeStyle = CustomTreeLoader.NextTreeStyle();

		TileType = LoadTile();
		CustomTreeLoader.byTileLookup[TileType] = this;

		SaplingType = LoadSapling();
		CustomTreeLoader.bySaplingLookup[SaplingType] = this;

		AcornType = LoadAcorn();
		CustomTreeLoader.byAcornLookup[AcornType] = this;

		LeafType = LoadLeaf();
		if (LeafType >= GoreID.Count)
			CustomTreeLoader.byCustomLeafLookup[LeafType] = this;

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
		static bool HookCanPlace(ushort type, int x, int y, int style, int dir, int alternate)
		{
			ModCustomTree tree = CustomTreeLoader.GetBySapling(type);
			if (tree is null)
				return true;
			else 
				return tree.ValidWallType(Framing.GetTileSafely(x, y).WallType) && tree.ValidGroundType(Framing.GetTileSafely(x, y + 1).TileType);
		}

		TileID.Sets.TreeSapling[type] = true;
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
		TileObjectData.newTile.HookCheckIfCanPlace = new((x, y, _, style, dir, alt) => HookCanPlace(type, x, y, style, dir, alt) ? 1 : 0, 0, 0, true);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.DrawFlipHorizontal = true;
		TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.RandomStyleRange = styleCount;
		TileObjectData.newTile.StyleMultiplier = 1;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(type);
	}

	public static void SetLeafStaticDefaults(int type)
	{
		ChildSafety.SafeGore[type] = true;
		GoreID.Sets.SpecialAI[type] = 3;
		GoreID.Sets.PaintedFallingLeaf[type] = true;
	}
}

[Autoload(false)]
internal class CustomTreeDefaultTile : ModTile
{
	private ModCustomTree initTree = null;
	private ModCustomTree Tree => initTree ?? CustomTreeLoader.GetByTile(Type);

	public override string Name => Tree.Name + "Tile";

	public override string Texture => Tree.TileTexture;

	public CustomTreeDefaultTile(ModCustomTree tree)
	{
		initTree = tree;
	}
	public CustomTreeDefaultTile() { }

	public override void SetStaticDefaults()
	{
		CustomTreeDefaults.SetTileStaticDefaults(Type);

		ModCustomTree tree = Tree;
		Color? color = tree.TileMapColor;
		if (color is not null)
			AddMapEntry(color.Value, tree.TileMapName ?? tree.GetLocalization(nameof(ModCustomTree.TileMapName), tree.PrettyPrintName));

		tree.SetTileStaticDefaults(this);
	}

	public override void SetDrawPositions(int x, int y, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
	{
		width = 20;
		height = 20;
	}

	public override bool TileFrame(int x, int y, ref bool resetFrame, ref bool noBreak)
	{
		Tree.TileFrame(x, y);
		return false;
	}

	public override void RandomUpdate(int x, int y)
	{
		Tree.RandomUpdate(x, y);
	}

	public override bool CreateDust(int x, int y, ref int type)
	{
		return Tree.CreateDust(x, y, ref type);
	}

	public override IEnumerable<Item> GetItemDrops(int x, int y)
	{
		return Tree.GetItemDrops(x, y);
	}
}

[Autoload(false)]
internal class CustomTreeDefaultSapling : ModTile
{
	private ModCustomTree initTree = null;
	private ModCustomTree Tree => initTree ?? CustomTreeLoader.GetBySapling(Type);

	public override string Name => Tree.Name + "Sapling";

	public override string Texture => Tree.SaplingTexture;

	public CustomTreeDefaultSapling(ModCustomTree tree)
	{
		initTree = tree;
	}
	public CustomTreeDefaultSapling() { }

	public override bool TileFrame(int x, int y, ref bool resetFrame, ref bool noBreak)
	{
		WorldGen.Check1x2(x, y, Type);
		return false;
	}

	public override void SetStaticDefaults()
	{
		ModCustomTree tree = Tree;
		CustomTreeDefaults.SetSaplingStaticDefaults(Type, tree.SaplingStyles);
		Color? color = tree.SaplingMapColor;
		if (color is not null)
			AddMapEntry(color.Value, tree.SaplingMapName ?? tree.GetLocalization(nameof(ModCustomTree.SaplingMapName), () => tree.PrettyPrintName() + " Sapling"));
	}

	public override void RandomUpdate(int x, int y)
	{
		ModCustomTree tree = Tree;
		if (Main.rand.Next(tree.GrowChance) == 0)
			tree.Grow(x, y);
	}
}

[Autoload(false)]
internal class CustomTreeDefaultAcorn : ModItem
{
	private ModCustomTree initTree = null;
	private ModCustomTree Tree => initTree ?? CustomTreeLoader.GetByAcorn(Type);

	public override string Name => Tree.Name + "Acorn";

	public override string Texture => Tree.AcornTexture;

	public override LocalizedText DisplayName => Tree.AcornDisplayName ?? Tree.GetLocalization(nameof(ModCustomTree.AcornDisplayName), () => Tree.PrettyPrintName() + " Acorn");

	public override LocalizedText Tooltip => Tree.AcornTooltip ?? Tree.GetLocalization(nameof(ModCustomTree.AcornTooltip), () => "");

	public CustomTreeDefaultAcorn(ModCustomTree tree)
	{
		initTree = tree;
	}
	public CustomTreeDefaultAcorn() { }

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(Tree.SaplingType, 0);
	}
}

[Autoload(false)]
internal class CustomTreeDefaultLeaf : ModGore
{
	private ModCustomTree initTree = null;
	private ModCustomTree Tree => initTree ?? CustomTreeLoader.GetByCustomLeaf(Type);

	public override string Name => Tree.Name + "Leaf";

	public override string Texture => Tree.LeafTexture;

	public CustomTreeDefaultLeaf(ModCustomTree tree)
	{
		initTree = tree;
	}
	public CustomTreeDefaultLeaf() { }

	public override void SetStaticDefaults()
	{
		CustomTreeDefaults.SetLeafStaticDefaults(Type);
	}
}