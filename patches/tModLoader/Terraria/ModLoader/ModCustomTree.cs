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

	public static void GetStyle(int x, int y, ref int style)
	{
		Tile tile = Main.tile[x, y];
		if (!TryGetByTile(tile.TileType, out ModCustomTree tree))
			return;

		style = tree.GetStyle(x, y);
	}

	public static bool Shake(int x, int y, ref bool createLeaves)
	{
		Tile tile = Main.tile[x, y];
		if (!TryGetByTile(tile.TileType, out ModCustomTree tree))
			return true;

		return tree.Shake(x, y, ref createLeaves);
	}

	public static void GetTreeLeaf(int x, Tile topTile, Tile t, ref int treeHeight, ref int treeFrame, ref int passStyle)
	{
		if (!TryGetByTile(topTile.TileType, out ModCustomTree tree))
			return;

		tree.GetTreeLeaf(x, topTile, t, ref treeHeight, ref treeFrame, out passStyle);
	}

	public static bool Grow(ushort tileType, int x, int y)
	{
		if (!TryGetByTile(tileType, out ModCustomTree tree))
			return false;

		return tree.Grow(x, y);
	}

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
}

public abstract class ModCustomTree : ModType, ILocalizedModType
{
	public virtual string TileTexture => (GetType().Namespace + "." + Name + "_Tile").Replace('.', '/');
	public virtual string SaplingTexture => (GetType().Namespace + "." + Name + "_Sapling").Replace('.', '/');
	public virtual string AcornTexture => (GetType().Namespace + "." + Name + "_Acorn").Replace('.', '/');
	public virtual string TopTexture => (GetType().Namespace + "." + Name + "_Top").Replace('.', '/');
	public virtual string BranchTexture => (GetType().Namespace + "." + Name + "_Branch").Replace('.', '/');

	/// <summary>
	/// If this is provided, new leaf ModGore will be registered and used as LeafType
	/// </summary>
	public virtual string LeafTexture => null;

	public int TreeStyle { get; internal set; }
	public ushort TileType { get; internal set; }
	public ushort SaplingType { get; internal set; }
	public int AcornType { get; internal set; }
	public int LeafType { get; internal set; }

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

	public virtual ushort LoadTile()
	{
		CustomTreeDefaultTile tile = new(this);
		Mod.AddContent(tile);
		return tile.Type;
	}

	public virtual ushort LoadSapling()
	{
		CustomTreeDefaultSapling sapling = new(this);
		Mod.AddContent(sapling);
		return sapling.Type;
	}

	public virtual int LoadAcorn()
	{
		CustomTreeDefaultAcorn acorn = new(this);
		Mod.AddContent(acorn);
		return acorn.Type;
	}

	public virtual int LoadLeaf()
	{
		if (LeafTexture is null)
			return GoreID.TreeLeaf_Normal;

		CustomTreeDefaultLeaf leaf = new(this);
		Mod.AddContent(leaf);
		return leaf.Type;
	}

	public virtual void TileFrame(int x, int y)
	{
		CustomTreeGen.CheckTree(x, y, GetTreeSettings());
	}

	public virtual bool Grow(int x, int y)
	{
		if (CustomTreeGen.GrowTree(x, y, GetTreeSettings()) && WorldGen.PlayerLOS(x, y))
		{
			WorldGen.TreeGrowFXCheck(x, y);
			return true;
		}
		return false;
	}

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
	/// Randomly executed on every tree tile
	/// </summary>
	public virtual void RandomUpdate(int x, int y) { }

	/// <summary>
	/// Same as <see cref="ModTile.GetItemDrops"/>.
	/// Use TreeTileInfo.GetInfo to determine tile type
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
	/// Allows to change dust type created or to disable tile dust
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

	// TODO: style is always 0
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

	public override bool RightClick(int x, int y)
	{
		Tree.Grow(x, y);
		return true;
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

public static class CustomTreeGen
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

/// <summary>
/// Tree tile with position and its info
/// </summary>
public record struct PositionedTreeTile(Point Pos, TreeTileInfo Info);

/// <summary/>
public enum TreeFoliageType
{
	/// <summary/>
	Top,

	/// <summary/>
	LeftBranch,

	/// <summary/>
	RightBranch
}

/// <summary>
/// Tree statistics
/// </summary>
public struct TreeStats
{
	/// <summary>
	/// Total tree blocks
	/// </summary>
	public int TotalBlocks = 0;

	/// <summary>
	/// Total tree branches
	/// </summary>
	public int TotalBranches = 0;

	/// <summary>
	/// Total leafy branches
	/// </summary>
	public int LeafyBranches = 0;

	/// <summary>
	/// True if tree top exisis and not broken
	/// </summary>
	public bool HasTop = false;

	/// <summary>
	/// True if tree top exisis and broken
	/// </summary>
	public bool BrokenTop = false;

	/// <summary>
	/// True if thee have left root
	/// </summary>
	public bool LeftRoot = false;

	/// <summary>
	/// True if thee have right root
	/// </summary>
	public bool RightRoot = false;

	/// <summary>
	/// Tree top position
	/// </summary>
	public Point Top = new(0, int.MaxValue);

	/// <summary>
	/// Tree bottom position
	/// </summary>
	public Point Bottom = new(0, 0);

	/// <summary>
	/// Tree ground tile type
	/// </summary>
	public ushort GroundType = 0;

	public TreeStats()
	{
	}

	/// <summary/>
	public override string ToString()
	{
		return $"T:{TotalBlocks} " +
			$"B:{TotalBranches} " +
			$"BL:{LeafyBranches} " +
			$"{(HasTop ? BrokenTop ? "TB " : "TL " : "")}" +
			$"{(LeftRoot ? "Rl " : "")}" +
			$"{(RightRoot ? "Rr " : "")}" +
			$"X:{Top.X} Y:{Top.Y}-{Bottom.Y} G:{GroundType}";
	}
}

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
		//if (CustomTreeLoader.byTileLookup.ContainsKey(t.TileType))
		//	frameSize = 18;

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
		//if (CustomTreeLoader.byTileLookup.ContainsKey(t.TileType))
		//	frameSize = 18;

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
