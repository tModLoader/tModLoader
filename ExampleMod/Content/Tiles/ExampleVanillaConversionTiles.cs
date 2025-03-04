using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ExampleMod.Content.Walls;

namespace ExampleMod.Content.Tiles
{
	// These three classes showcase how to create tiles that act as corruption/crimson/hallow versions of vanilla tiles.
	// For this example, we will be making vanilla's desert fossil tiles convertible into the three spreading biomes
	public class HallowedFossilTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Hallow[Type] = true;
			TileID.Sets.HallowBiome[Type] = 1;
			TileID.Sets.HallowBiomeSight[Type] = true;
			TileID.Sets.HallowCountCollection.Add(Type);
			DustType = DustID.Pearlsand;
			AddMapEntry(new Color(157, 76, 152));
			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[TileID.HallowSandstone][Type] = true;

			//We need to register a conversion from the vanilla desert fossil into our modded variants., so our custom code can be called when the game attempts to convert the vanilla tile
			//We could register all three conversions in this class and reuse the same method for all three by checking for the conversionType there if we wanted to take up less space.
			TileLoader.RegisterConversion(TileID.DesertFossil, BiomeConversionID.Hallow, ConvertToHallow);
		}

		public bool ConvertToHallow(int i, int j, int type, int conversionType) {

			//This method is called whenever hallow biome conversion happens on a desert fossil tile, as per the RegisterConversion we called in SetStaticDefaults
			//We don't need to check the type or the conversionType as we only registered one conversion with this method, but the same method could be reused for multiple conversion types or tiles

			//We can use the ConvertTile utility method to change the fossil tile into our hallowed fossil tile, and it'll automatically handle tile frame updates and network syncing!
			WorldGen.ConvertTile(i, j, Type);
			return false;
		}

		//This code is called when the game attempts to convert our hallowed tile into a new biome
		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				// Purification powder doesn't convert hallow tiles back into purity, so we don't check for BiomeCoversionID.PurificationPowder
				case BiomeConversionID.Purity:
				case BiomeConversionID.Sand: // Yellow (desert) solution also converts evil/hallowed tiles back into purity, so don't forget that check!
					WorldGen.ConvertTile(i, j, TileID.DesertFossil);
					return;
				case BiomeConversionID.Corruption:
					WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptFossilTile>());
					return;
				case BiomeConversionID.Crimson:
					WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonFossilTile>());
					return;

				// This example showcases how to make hallow and evil biome conversion work, but you can extend this code to work for the other vanilla solutions.
				// Just don't forget to register the conversion type in SetStaticDefaults if you want a vanilla tile to turn into your new modded tile.
				// case BiomeConversionID.Snow:
				//		WorldGen.ConvertTile(i, j, TileID.Slush);
				//		return false;
			}
		}

		public override void RandomUpdate(int i, int j) {
			// Don't spread the biome if disabled by journey mode
			if (!WorldGen.AllowedToSpreadInfections)
				return;
			// Spreading tiles (besides grass) don't spread until hardmode. After plantera, the spread rate is also reduced, so we have to account for that
			if (!Main.hardMode || (NPC.downedPlantBoss && WorldGen.genRand.NextBool(2)))
				return;

			//Check a random nearby tile and try to convert it into hallow
			int testX = i + WorldGen.genRand.Next(-3, 4);
			int testY = j + WorldGen.genRand.Next(-3, 4);
			if (!WorldGen.InWorld(testX, testY, 10))
				return;

			//Chlorophyte prevents and repels biome spread, so we should account for that
			if (WorldGen.nearbyChlorophyte(testX, testY)) {
				WorldGen.ChlorophyteDefense(testX, testY);
				return;
			}

			//Sunflowers prevent spread in a very limited area
			if (WorldGen.CountNearBlocksTypes(testX, testY, 2, 1, TileID.Sunflower) <= 0)
				return;

			WorldGen.Convert(testX, testY, BiomeConversionID.Hallow, 1);
		}

		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			//We use this method to set the merge values of the adjacent tiles to -2 if the tile nearby is a pearlsandstone block
			//-2 is what terraria uses to designate the tiles that will merge with ours using the custom frames
			WorldGen.TileMergeAttempt(-2, TileID.HallowSandstone, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
		}
	}

	public class CorruptFossilTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Corrupt[Type] = true;
			TileID.Sets.CorruptBiome[Type] = 1;
			TileID.Sets.CorruptBiomeSight[Type] = true;
			TileID.Sets.CorruptCountCollection.Add(Type);
			DustType = DustID.Corruption;
			AddMapEntry(new Color(65, 48, 99));
			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[TileID.CorruptSandstone][Type] = true;

			//We do the same thing as the hallow one. For this one, lets show how it would look to register multiple conversions from the same vanilla tile
			TileLoader.RegisterConversion(TileID.DesertFossil, BiomeConversionID.Corruption, ConvertToEvilBiome);
			TileLoader.RegisterConversion(TileID.DesertFossil, BiomeConversionID.Crimson, ConvertToEvilBiome);
		}

		public bool ConvertToEvilBiome(int i, int j, int type, int conversionType) {
			//Since we registered two conversions with this same method, we can use conversiontype to determine which one is happening
			if (conversionType == BiomeConversionID.Corruption)
				WorldGen.ConvertTile(i, j, Type);
			else {
				int crimsonTileType = ModContent.TileType<CrimsonFossilTile>();
				WorldGen.ConvertTile(i, j, crimsonTileType);
			}
			return false;
		}

		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Purity:
				case BiomeConversionID.Sand:
				case BiomeConversionID.PurificationPowder:
					WorldGen.ConvertTile(i, j, TileID.DesertFossil);
					return;
				case BiomeConversionID.Hallow:
					WorldGen.ConvertTile(i, j, ModContent.TileType<HallowedFossilTile>());
					return;
				case BiomeConversionID.Crimson:
					WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonFossilTile>());
					return;
			}
		}

		public override void RandomUpdate(int i, int j) {
			// Don't spread the biome if disabled by journey mode
			if (!WorldGen.AllowedToSpreadInfections)
				return;
			// Spreading tiles (besides grass) don't spread until hardmode. After plantera, the spread rate is also reduced, so we have to account for that
			if (!Main.hardMode || (NPC.downedPlantBoss && WorldGen.genRand.NextBool(2)))
				return;

			//Check a random nearby tile and try to convert it into corruption
			int testX = i + WorldGen.genRand.Next(-3, 4);
			int testY = j + WorldGen.genRand.Next(-3, 4);
			if (!WorldGen.InWorld(testX, testY, 10))
				return;

			//Chlorophyte prevents and repels biome spread, so we should account for that
			if (WorldGen.nearbyChlorophyte(testX, testY)) {
				WorldGen.ChlorophyteDefense(testX, testY);
				return;
			}

			//Sunflowers prevent spread in a very limited area
			if (WorldGen.CountNearBlocksTypes(testX, testY, 2, 1, TileID.Sunflower) <= 0)
				return;

			WorldGen.Convert(testX, testY, BiomeConversionID.Corruption, 1);
		}

		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			WorldGen.TileMergeAttempt(-2, TileID.CorruptSandstone, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
		}
	}

	public class CrimsonFossilTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Crimson[Type] = true;
			TileID.Sets.CrimsonBiome[Type] = 1;
			TileID.Sets.CrimsonBiomeSight[Type] = true;
			TileID.Sets.CrimsonCountCollection.Add(Type);
			DustType = DustID.Crimstone;
			AddMapEntry(new Color(112, 33, 32));

			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[TileID.CrimsonSandstone][Type] = true;
		}

		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Purity:
				case BiomeConversionID.Sand:
				case BiomeConversionID.PurificationPowder:
					WorldGen.ConvertTile(i, j, TileID.DesertFossil);
					return;
				case BiomeConversionID.Hallow:
					WorldGen.ConvertTile(i, j, ModContent.TileType<HallowedFossilTile>());
					return;
				case BiomeConversionID.Corruption:
					WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptFossilTile>());
					return;
			}
		}

		public override void RandomUpdate(int i, int j) {
			// Don't spread the biome if disabled by journey mode
			if (!WorldGen.AllowedToSpreadInfections)
				return;
			// Spreading tiles (besides grass) don't spread until hardmode. After plantera, the spread rate is also reduced, so we have to account for that
			if (!Main.hardMode || (NPC.downedPlantBoss && WorldGen.genRand.NextBool(2)))
				return;

			//Check a random nearby tile and try to convert it into crimson
			int testX = i + WorldGen.genRand.Next(-3, 4);
			int testY = j + WorldGen.genRand.Next(-3, 4);
			if (!WorldGen.InWorld(testX, testY, 10))
				return;

			//Chlorophyte prevents and repels biome spread, so we should account for that
			if (WorldGen.nearbyChlorophyte(testX, testY)) {
				WorldGen.ChlorophyteDefense(testX, testY);
				return;
			}

			//Sunflowers prevent spread in a very limited area
			if (WorldGen.CountNearBlocksTypes(testX, testY, 2, 1, TileID.Sunflower) <= 0)
				return;

			WorldGen.Convert(testX, testY, BiomeConversionID.Crimson, 1);
		}

		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			WorldGen.TileMergeAttempt(-2, TileID.CrimsonSandstone, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
		}
	}

	#region Items
	internal class HallowedFossilTileItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<HallowedFossilTile>());
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<HallowedFossilWallItem>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	internal class CorruptFossilTileItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<CorruptFossilTile>());
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<CorruptFossilWallItem>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	internal class CrimsonFossilTileItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<CrimsonFossilTile>());
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<CrimsonFossilWallItem>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
	#endregion
}