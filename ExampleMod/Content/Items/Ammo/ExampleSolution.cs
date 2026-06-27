using ExampleMod.Content.Dusts;
using ExampleMod.Content.Tiles;
using ExampleMod.Content.Tiles.Furniture;
using ExampleMod.Content.Walls;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleSolution : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ItemID.Sets.SortingPriorityTerraforming[Type] = 101; // One past dirt solution
		}

		public override void SetDefaults() {
			Item.DefaultToSolution(ModContent.ProjectileType<ExampleSolutionProjectile>());
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
		}
	}

	public class ExampleSolutionProjectile : ModProjectile
	{
		public static int ConversionType;

		public ref float Progress => ref Projectile.ai[0];
		// Solutions shot by the terraformer get an increase in conversion area size, indicated by the second AI parameter being set to 1
		public bool ShotFromTerraformer => Projectile.ai[1] == 1f;

		public override void SetStaticDefaults() {
			// Cache the conversion type here instead of repeately fetching it every frame
			ConversionType = ModContent.GetInstance<ExampleSolutionConversion>().Type;
		}

		public override void SetDefaults() {
			// This method quickly sets the projectile properties to match other sprays.
			Projectile.DefaultToSpray();
			Projectile.aiStyle = 0; // Here we set aiStyle back to 0 because we have custom AI code
		}

		public override bool? CanDamage() => false;

		public override void AI() {

			if (Projectile.timeLeft > 133)
				Projectile.timeLeft = 133;

			if (Projectile.owner == Main.myPlayer) {
				int size = ShotFromTerraformer ? 3 : 2;
				Point tileCenter = Projectile.Center.ToTileCoordinates();
				WorldGen.Convert(tileCenter.X, tileCenter.Y, ConversionType, size);
			}

			int spawnDustTreshold = 7;
			if (ShotFromTerraformer)
				spawnDustTreshold = 3;

			if (Progress > (float)spawnDustTreshold) {
				float dustScale = 1f;
				int dustType = ModContent.DustType<ExampleSolutionDust>();

				if (Progress == spawnDustTreshold + 1)
					dustScale = 0.2f;
				else if (Progress == spawnDustTreshold + 2)
					dustScale = 0.4f;
				else if (Progress == spawnDustTreshold + 3)
					dustScale = 0.6f;
				else if (Progress == spawnDustTreshold + 4)
					dustScale = 0.8f;

				int dustArea = 0;
				if (ShotFromTerraformer) {
					dustScale *= 1.2f;
					dustArea = (int)(12f * dustScale);
				}

				Dust sprayDust = Dust.NewDustDirect(new Vector2(Projectile.position.X - dustArea, Projectile.position.Y - dustArea), Projectile.width + dustArea * 2, Projectile.height + dustArea * 2, dustType, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 100);
				sprayDust.noGravity = true;
				sprayDust.scale *= 1.75f * dustScale;
			}

			Progress++;
			Projectile.rotation += 0.3f * Projectile.direction;
		}
	}

	public class ExampleSolutionConversion : ModBiomeConversion
	{
		public static int WallType;
		public static int UnsafeWallType;
		public static int StoneType;
		public static int SandType;
		public static int ChairType;
		public static int WorkbenchType;

		public override void PostSetupContent() {

			// Cache the conversion types.
			WallType = ModContent.WallType<ExampleWall>();
			UnsafeWallType = ModContent.WallType<ExampleWallUnsafe>();
			StoneType = ModContent.TileType<ExampleBlock>();
			SandType = ModContent.TileType<ExampleSand>();
			ChairType = ModContent.TileType<ExampleChair>();
			WorkbenchType = ModContent.TileType<ExampleWorkbench>();

			// Normally we'd just use WallLoader.RegisterSimpleConversion on the basic wall types and rely on the fallback system
			// but we want to convert safe walls to safe example walls and unsafe to unsafe, where vanilla convers safe walls to unsafe walls on all conversions
			for (int i = 0; i < WallLoader.WallCount; i++) {
				if (WallID.Sets.Conversion.Dirt[i] ||
					WallID.Sets.Conversion.Grass[i] ||
					WallID.Sets.Conversion.Stone[i] ||
					WallID.Sets.Conversion.Sandstone[i] ||
					WallID.Sets.Conversion.HardenedSand[i] ||
					WallID.Sets.Conversion.Ice[i] ||
					WallID.Sets.Conversion.NewWall1[i] || // NewWalls are the underground wall variants
					WallID.Sets.Conversion.NewWall2[i] ||
					WallID.Sets.Conversion.NewWall3[i] ||
					WallID.Sets.Conversion.NewWall4[i])
					WallLoader.RegisterConversion(i, Type, ConvertWalls);
			}
			WallLoader.RegisterConversionFallback(WallType, WallID.Dirt, Type);
			WallLoader.RegisterConversionFallback(UnsafeWallType, WallID.DirtUnsafe, Type);

			// This registers a conversion from Sand to ExampleSand, as well as a fallback from ExampleSand to Sand, so other solutions can convert ExampleSand (eg to Crimsand)
			TileLoader.RegisterSimpleConversion(TileID.Sand, Type, SandType);

			// We register a conversion method and fallback separately rather than using RegisterSimpleConversion, because ConvertStone has custom logic for converting trees on the tile above
			TileLoader.RegisterConversion(TileID.Stone, Type, ConvertStone);
			TileLoader.RegisterConversionFallback(StoneType, TileID.Stone, Type);

			// Chairs and Workbenches aren't normally converted by solutions, so there's no sensible fallback to register.
			// We could register a purifying conversion for these too if we wanted
			TileLoader.RegisterConversion(TileID.Chairs, Type, ConvertChairs);
			TileLoader.RegisterConversion(TileID.WorkBenches, Type, ConvertWorkbenches);
		}

		public bool ConvertStone(int i, int j, int type, int conversionType) {

			int tileTypeAbove = -1;
			if (j > 1 && Main.tile[i, j - 1].HasTile)
				tileTypeAbove = Main.tile[i, j - 1].TileType;

			// Convert gem trees above stone into exampletrees
			// If your tree is a ModTree and is meant to replace generic surface trees, we wouldn't really need this method, since ModTrees share an ID with the vanilla tree types.
			// ModTrees automatically convert based on the tile type they're planted on, and we're converting the ground tile.
			// In this case, we're converting stone, so we need this code to transform gem trees into regular vanilla tree types.
			// ...Admittedly, you'd still need this method since vanity trees (Sakura and Willow) aren't part of the vanilla tree types, so they would break on conversion if not accounted for
			FindAndConvertTree(i, j, tileTypeAbove);

			WorldGen.ConvertTile(i, j, StoneType);
			return false;
		}

		public void FindAndConvertTree(int i, int j, int tileTypeAbove) {

			if (tileTypeAbove == -1)
				return;

			if (!TileID.Sets.CountsAsGemTree[tileTypeAbove])
				return;

			int treeBottom = j;
			int treeTop = treeBottom - 1;
			int treeCenterX = i;

			// Check for if the tile is the tree's "trunk" or just the root tiles on the side
			// We do this by checking for the specific tile frame of the tree tile.
			// Necessary because the "CountsAsGemTree" ID set doesn't care about the tile's frame and returns true even if the tile isnt the tree's "trunk"
			int treeFrameX = Main.tile[treeCenterX, treeTop].TileFrameX / 22;
			int treeFrameY = Main.tile[treeCenterX, treeTop].TileFrameY / 22;
			bool isTreeTrunk = (treeFrameX != 1 && treeFrameX != 2) || treeFrameY < 6;

			// Niche edgecase check: If a stone block was placed under a tree's branch, it shouldnt be converted at all, as it is not actually attached to the stone tile below
			bool isTreeBranch = (treeFrameX == 3 && treeFrameY < 3) || (treeFrameX == 4 && treeFrameY >= 3 && treeFrameY < 6);
			if (isTreeBranch)
				return;

			// If the tile above wasn't a tree trunk but instead a root tile on the side, check the adjacent two tiles to find it
			if (!isTreeTrunk) {
				for (int x = treeCenterX - 1; x < treeCenterX + 2; x += 2) {

					Tile topTile = Main.tile[x, treeTop];
					if (!topTile.HasTile || !TileID.Sets.CountsAsGemTree[topTile.TileType])
						continue;

					// Check for tree trunk framing
					treeFrameX = topTile.TileFrameX / 22;
					treeFrameY = topTile.TileFrameY / 22;
					isTreeTrunk = (treeFrameX != 1 && treeFrameX != 2) || treeFrameY < 6;

					// We found our tree trunk center
					if (isTreeTrunk) {
						treeCenterX = x;
						break;
					}
				}
			}

			// Find the top of the tree by repeatedly going up until we don't find any more tree tiles
			while (treeTop >= 0 && Main.tile[treeCenterX, treeTop].HasTile && TileID.Sets.CountsAsGemTree[Main.tile[treeCenterX, treeTop].TileType])
				treeTop--;

			// Turn all the tiles around it into example trees
			// Because ExampleTree is a ModTree, it shares its tileID with the rest of the vanilla trees, and will automatically convert based on the floor type
			for (int x = treeCenterX - 1; x < treeCenterX + 2; x++) {
				for (int y = treeTop; y < treeBottom; y++) {
					Tile t = Main.tile[x, y];
					if (t.HasTile && TileID.Sets.CountsAsGemTree[t.TileType])
						t.TileType = TileID.Trees;
				}
			}

			// Turn the floor into example tiles (We have to convert the adjacent tiles, otherwise the side root tiles may get broken)
			// The framing will happen naturally when the floor tile below gets converted and frames the other adjacent tiles, so we don't need to use WorldGen.Convert here
			for (int x = treeCenterX - 1; x < treeCenterX + 2; x++) {
				Tile t = Main.tile[x, treeBottom];
				if (t.HasTile && t.TileType == TileID.Stone)
					t.TileType = (ushort)StoneType;
			}
		}

		public bool ConvertChairs(int i, int j, int type, int conversionType) {
			// Find the bottom of the chair
			if (Main.tile[i, j + 1].HasTile && Main.tile[i, j + 1].TileType == TileID.Chairs)
				j++;

			Tile tileTop = Main.tile[i, j - 1];
			Tile tileBottom = Main.tile[i, j];

			// Manually convert the top part of the chair, and then the bottom half through WorldGen.Convert so it automatically handles the framing and syncing
			tileTop.TileType = (ushort)ChairType;


			// Reset the Y frame to be within the bounds of examplechair's tilesheet
			tileTop.TileFrameY = 0;
			tileBottom.TileFrameY = 18;

			WorldGen.ConvertTile(i, j, ChairType);
			return false;
		}

		public bool ConvertWorkbenches(int i, int j, int type, int conversionType) {
			// Find the right of the workbench
			if (Main.tile[i + 1, j].HasTile && Main.tile[i + 1, j].TileType == TileID.WorkBenches)
				i++;

			Tile tileLeft = Main.tile[i - 1, j];
			Tile tileRight = Main.tile[i, j];

			// Manually convert the right part of the workbench, and then the right half through WorldGen.Convert so it automatically handles the framing and syncing
			tileLeft.TileType = (ushort)WorkbenchType;

			// Reset the X frame to be within the bounds of exampleworkbench's tilesheet
			tileLeft.TileFrameX = 0;
			tileRight.TileFrameX = 18;

			WorldGen.ConvertTile(i, j, WorkbenchType);
			return false;
		}

		public bool ConvertWalls(int i, int j, int type, int conversionType) {

			// Turn all walls into example walls or unsafe example walls, depending on if the original wall was safe or not (Main.wallHouse is what determines that)
			int wallType = Main.wallHouse[type] ? WallType : UnsafeWallType;
			WorldGen.ConvertWall(i, j, wallType);
			return false;
		}
	}
}
