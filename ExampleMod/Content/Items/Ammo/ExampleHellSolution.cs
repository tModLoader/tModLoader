using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Ammo
{
	public class ExampleHellSolution : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ItemID.Sets.SortingPriorityTerraforming[Type] = 101; // One past dirt solution
		}

		public override void SetDefaults() {
			Item.DefaultToSolution(ModContent.ProjectileType<ExampleHellSolutionProjectile>());
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
		}
	}

	public class ExampleHellSolutionProjectile : ModProjectile
	{
		public static int ConversionType;

		public ref float Progress => ref Projectile.ai[0];
		// Solutions shot by the terraformer get an increase in conversion area size, indicated by the second AI parameter being set to 1
		public bool ShotFromTerraformer => Projectile.ai[1] == 1f;

		public override void SetStaticDefaults() {
			// Cache the conversion type here instead of repeately fetching it every frame
			ConversionType = ModContent.GetInstance<ExampleHellSolutionConversion>().Type;
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
				short dustType = Main.rand.NextBool() ? DustID.DirtSpray : DustID.CrimsonSpray;

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

	public class ExampleHellSolutionConversion : ModBiomeConversion
	{
		public override void PostSetupContent() {

			TileLoader.RegisterConversion(TileID.Grass, Type, HellifyGrass);
			TileLoader.RegisterConversion(TileID.GolfGrass, Type, HellifyGrass);

			TileLoader.RegisterConversion(TileID.Dirt, Type, TileID.Ash);
			TileLoader.RegisterConversion(TileID.Stone, Type, TileID.Ash);
			TileLoader.RegisterConversion(TileID.Sand, Type, TileID.Ash);

			// Manually register clay
			TileLoader.RegisterConversion(TileID.ClayBlock, Type, TileID.Ash);

			WallLoader.RegisterConversion(WallID.DirtUnsafe, Type, HellifyWall);
			WallLoader.RegisterConversion(WallID.GrassUnsafe, Type, HellifyWall);
			WallLoader.RegisterConversion(WallID.Stone, Type, HellifyWall);
		}

		public bool HellifyGrass(int i, int j, int type, int conversionType) {

			int tileTypeAbove = -1;
			if (j > 1 && Main.tile[i, j - 1].HasTile)
				tileTypeAbove = Main.tile[i, j - 1].TileType;

			// Convert trees above grass into hell trees
			// Most vanilla trees usually automatically turn into the type that matches the grass below, but not ashwood trees
			FindAndConvertTree(i, j, tileTypeAbove);

			// Convert plants above grass into ash plants
			// Do note, for most other conversions this wouldn't be necessary, as plants convert automatically into the type that matches the grass they're on when framed
			// Sadly for us, hell plants don't share this behavior, so we will have to do it ourselves otherwise they'll break
			// If you were making a modded biome with its own plants, you'll need to mimic this code as well to handle conversion from a vanilla grass tile into your modded grass block
			// To handle conversion from your modded plants back to vanilla ones, you can include checks for floor tile in ModTile.TileFrame() and then convert it to the appropriate vanilla plant from there
			if (tileTypeAbove == TileID.Plants ||
				tileTypeAbove == TileID.CorruptPlants ||
				tileTypeAbove == TileID.CrimsonPlants ||
				tileTypeAbove == TileID.HallowedPlants ||
				tileTypeAbove == TileID.Plants2 ||
				tileTypeAbove == TileID.HallowedPlants2) {

				Tile tileAbove = Main.tile[i, j - 1];

				// Trim the top of 2 tall plants, since hell plants only have 1 tall plants
				// If you were making a modded biome and had 2 tall plants you could ignore this part and convert 2 tall plants into the appropriate modded 2 tall plant
				if (j > 2 && (tileTypeAbove == TileID.Plants2 || tileTypeAbove == TileID.HallowedPlants2)) {
					Tile tileTwiceAbove = Main.tile[i, j - 2];
					if (tileTwiceAbove.TileType == tileTypeAbove) {
						tileTwiceAbove.HasTile = false;
						tileTwiceAbove.TileType = 0;
					}

					// Reset the Y frame, since the bottom half of tall plants would have a higher Y frame
					tileAbove.TileFrameY = 0;
				}

				// Doing the change by directly setting the TileType without using WorldGen.Convert, otherwise the plant would break immediately as we haven't converted the grass below yet
				tileAbove.TileType = TileID.AshPlants;
				// We also reset the X frame, since hell plants have much less variants compared to other biome plants (only 10 compared to forest plant's 44!)
				tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
			}

			WorldGen.ConvertTile(i, j, TileID.AshGrass);
			return false;
		}

		public void FindAndConvertTree(int i, int j, int tileTypeAbove) {

			if (tileTypeAbove == -1)
				return;

			if (!TileID.Sets.IsATreeTrunk[tileTypeAbove])
				return;

			int treeBottom = j;
			int treeTop = treeBottom - 1;
			int treeCenterX = i;

			// Check for if the tile is the tree's "trunk" or just the root tiles on the side
			// We do this by checking for the specific tile frame of the tree tile.
			// Necessary because the "IsATreeTrunk" ID set doesn't care about the tile's frame and returns true even if the tile isnt the tree's "trunk"
			int treeFrameX = Main.tile[treeCenterX, treeTop].TileFrameX / 22;
			int treeFrameY = Main.tile[treeCenterX, treeTop].TileFrameY / 22;
			bool isTreeTrunk = (treeFrameX != 1 && treeFrameX != 2) || treeFrameY < 6;

			// Niche edgecase check: If a grass block was placed under a tree's branch, it shouldnt be converted at all, as it is not actually attached to the grass tile below
			bool isTreeBranch = (treeFrameX == 3 && treeFrameY < 3) || (treeFrameX == 4 && treeFrameY >= 3 && treeFrameY < 6);
			if (isTreeBranch)
				return;

			// If the tile above wasn't a tree trunk but instead a root tile on the side, check the adjacent two tiles to find it
			if (!isTreeTrunk) {
				for (int x = treeCenterX - 1; x < treeCenterX + 2; x += 2) {

					Tile topTile = Main.tile[x, treeTop];
					if (!topTile.HasTile || !TileID.Sets.IsATreeTrunk[topTile.TileType])
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
			while (treeTop >= 0 && Main.tile[treeCenterX, treeTop].HasTile && TileID.Sets.IsATreeTrunk[Main.tile[treeCenterX, treeTop].TileType])
				treeTop--;

			// Turn all the tiles around it into hell trees
			for (int x = treeCenterX - 1; x < treeCenterX + 2; x++) {
				for (int y = treeTop; y < treeBottom; y++) {
					Tile t = Main.tile[x, y];
					if (t.HasTile && TileID.Sets.IsATreeTrunk[t.TileType])
						t.TileType = TileID.TreeAsh;
				}
			}

			// Turn the floor into ash grass (We have to convert the adjacent tiles, otherwise the side root tiles may get broken)
			// The framing will happen naturally when the floor tile below gets converted and frames the other adjacent tiles, so we don't need to use WorldGen.Convert here
			for (int x = treeCenterX - 1; x < treeCenterX + 2; x++) {
				Tile t = Main.tile[x, treeBottom];
				if (t.HasTile && TileID.Sets.Conversion.Grass[t.TileType] || TileID.Sets.Conversion.GolfGrass[t.TileType])
					t.TileType = TileID.AshGrass;
			}
		}

		public bool HellifyWall(int i, int j, int type, int conversionType) {

			// Random pick of lava walls, except smouldering stone because that one looks too different
			WorldGen.ConvertWall(i, j, WallID.Lava1Echo + WorldGen.genRand.Next(3));
			return false;
		}
	}
}