using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleSolution : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ItemID.Sets.SortingPriorityTerraforming[Type] = 101; //One past dirt soulution
		}

		public override void SetDefaults() {
			Item.DefaultToSolution(ModContent.ProjectileType<ExampleSolutionProjectile>());
			return;
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
		}
	}

	public class ExampleSolutionProjectile : ModProjectile {

		public ref float Progress => ref Projectile.ai[0];
		// Solutions shot by the terraformer get an increase in conversion area size, indicated by the second AI parameter being set to 1
		public bool ShotFromTerraformer => Projectile.ai[1] == 1f;
		public static int ConversionType;

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
				short dustType = Main.rand.NextBool() ? DustID.DirtSpray : DustID.CrimsonSpray;

				if (Progress == spawnDustTreshold + 1)
					dustScale = 0.2f;
				else if (Progress ==spawnDustTreshold + 2)
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

	public class ExampleSolutionConversion : ModBiomeConversion {
		public override void SetStaticDefaults() {

			//Go over every tile and add a conversion to it for our conversion type if they're part of the list of usual conversion tiles
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Conversion.Dirt[i] ||
					TileID.Sets.Conversion.Grass[i] ||
					TileID.Sets.Conversion.GolfGrass[i] ||
					TileID.Sets.Conversion.Stone[i] ||
					TileID.Sets.Conversion.Sand[i])
					TileLoader.RegisterConversion(i, Type, HellifyTile);
			}

			//Manually register clay
			TileLoader.RegisterConversion(TileID.ClayBlock, Type, HellifyTile);

			//Manually register small plants to turn into hell plants
			TileLoader.RegisterConversion(TileID.Plants, Type, HellifyTile);
			TileLoader.RegisterConversion(TileID.Plants2, Type, HellifyTile);

			//Do the same for walls
			for (int i = 0; i < WallLoader.WallCount; i++) {
				if (WallID.Sets.Conversion.Dirt[i] ||
					WallID.Sets.Conversion.Grass[i] ||
					WallID.Sets.Conversion.Stone[i])
					WallLoader.RegisterConversion(i, Type, HellifyWall);
			}
		}

		public bool HellifyTile(int i, int j, int type, int conversionType) {

			//Turn tiles into hell-appropriate versions
			if (type == TileID.ClayBlock || TileID.Sets.Conversion.Stone[type] || TileID.Sets.Conversion.Dirt[type] || TileID.Sets.Conversion.Sand[type]) {
				WorldGen.ConvertTile(i, j, TileID.Ash, true);
				return false;
			}
			if (TileID.Sets.Conversion.Grass[type] || TileID.Sets.Conversion.GolfGrass[type]) {
				int tileAbove = -1;
				if (j > 1 && Main.tile[i, j - 1].HasTile)
					tileAbove = Main.tile[i, j - 1].TileType;

				// Convert trees above grass into hell trees
				if (tileAbove != -1 && TileID.Sets.IsATreeTrunk[tileAbove]) {
					int treeBottom = j;
					int treeTop = treeBottom - 1;
					int treeCenterX = i;

					// Check for if the tile is the tree's "trunk" or just the root tiles on the side / branches
					// This code was taken from Main.DrawTileCracks(), as branches and roots don't draw cracks, but simplified since we aren't interested in tree branches
					// Necessary because the "IsATreeTrunk" ID set doesn't care about the tile's frame and returns true even if the tile isnt the tree's "trunk"
					int treeFrameX = Main.tile[treeCenterX, treeTop].TileFrameX / 22;
					int treeFrameY = Main.tile[treeCenterX, treeTop].TileFrameY / 22;
					bool isTreeTrunk = (treeFrameX != 1 && treeFrameX != 2) || treeFrameY < 6;
					
					// If the tile above wasn't a tree trunk, check the adjacent two tiles to find it
					if (!isTreeTrunk) {
						for (int x = treeCenterX - 1; x < treeCenterX + 2; x += 2) {

							if (!Main.tile[x, treeTop].HasTile || !TileID.Sets.IsATreeTrunk[Main.tile[x, treeTop].TileType])
								continue;

							treeFrameX = Main.tile[x, treeTop].TileFrameX / 22;
							treeFrameY = Main.tile[x, treeTop].TileFrameY / 22;
							isTreeTrunk = (treeFrameX != 1 && treeFrameX != 2) || treeFrameY < 6;

							// We found our tree trunk center
							if (isTreeTrunk) {
								treeCenterX = x;
								break;
							}
						}
					}

					// Find the top of the tree by repeatedly going up
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

					// Turn the floor into grass (Extra code to let the side roots survive)
					// The framing will happen naturally when the floor tile below gets converted and frames the other tiles
					for (int x = treeCenterX - 1; x < treeCenterX + 2; x++) {
						Tile t = Main.tile[x, treeBottom];
						if (t.HasTile && TileID.Sets.Conversion.Grass[type] || TileID.Sets.Conversion.GolfGrass[type])
							t.TileType = TileID.AshGrass;
					}
				}

				//Convert plants above into ash plants
				if (tileAbove == TileID.Plants) {
					Tile t = Main.tile[i, j - 1];
					t.TileType = TileID.AshPlants;
				}

				WorldGen.ConvertTile(i, j, TileID.AshGrass);
				return false;
			}

			//Convert 1 tall plants and break 2 tall ones
			if (type == TileID.Plants) {

				//Convert the tile below so that the plants don't immediately destroy upon framing
				if (j < Main.maxTilesY - 1 && Main.tile[i, j + 1].HasTile && TileID.Sets.Conversion.Grass[Main.tile[i, j + 1].TileType]) {
					Tile t = Main.tile[i, j + 1];
					t.TileType = TileID.AshGrass;
				}

				WorldGen.ConvertTile(i, j, TileID.AshPlants);
				return false;
			}
			if (type == TileID.Plants2) {
				WorldGen.KillTile(i, j);
				if (Main.netMode != 0)
					NetMessage.SendTileSquare(-1, i, j);
				return false;
			}

			return true;
		}

		public bool HellifyWall(int i, int j, int type, int conversionType) {

			//Random pick of lava walls, except smouldering stone because that one looks too different
			WorldGen.ConvertWall(i, j, WallID.Lava1Echo + WorldGen.genRand.Next(3));
			return false;
		}
	}
}