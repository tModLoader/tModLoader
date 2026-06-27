using ExampleMod.Content.Items;
using ExampleMod.Content.Walls;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

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

			// We need to register a conversion from the vanilla desert fossil into our modded variants, so our custom code can be called when the game attempts to convert the vanilla tile
			TileLoader.RegisterSimpleConversion(TileID.DesertFossil, BiomeConversionID.Hallow, Type);
			TileID.Sets.Infectable[TileID.DesertFossil] = true; // Adding desert fossil to infectable tiles, without it no infection will spread over to it.

			TileLoader.RegisterConversion(Type, BiomeConversionID.Sand, TileID.DesertFossil); // Yellow (desert) solution also converts evil/hallowed tiles back into purity, so don't forget that check!
		}

		public override void RandomUpdate(int i, int j) {
			// We use this helper method to mimic vanilla behavior for spreading tiles, letting our hallowed fossil infect convert nearby tiles into hallowed versions of themselves
			WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Hallow);
		}

		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			// We use this method to set the merge values of the adjacent tiles to -2 if the tile nearby is a pearlsandstone block
			// -2 is what terraria uses to designate the tiles that will merge with ours using the custom frames
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

			TileLoader.RegisterSimpleConversion(TileID.DesertFossil, BiomeConversionID.Corruption, Type);
			//TileID.Sets.Infectable[TileID.DesertFossil] = true; Since desert fossil was already added to infectable tiles in HallowedFossilTile, we don't need to add it again.
			//Still, having a commented out version of the code here is a reminder that this is needed for the tile to be infectable.

			TileLoader.RegisterConversion(Type, BiomeConversionID.Sand, TileID.DesertFossil);
		}

		public override void RandomUpdate(int i, int j) {
			WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Corruption);
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

			TileLoader.RegisterSimpleConversion(TileID.DesertFossil, BiomeConversionID.Crimson, Type);
			//TileID.Sets.Infectable[TileID.DesertFossil] = true; Since desert fossil was already added to infectable tiles in HallowedFossilTile, we don't need to add it again.
			//Still, having a commented out version of the code here is a reminder that this is needed for the tile to be infectable.

			TileLoader.RegisterConversion(Type, BiomeConversionID.Sand, TileID.DesertFossil);
		}

		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Sand:
					WorldGen.ConvertTile(i, j, TileID.DesertFossil);
					return;
			}
		}

		public override void RandomUpdate(int i, int j) {
			WorldGen.SpreadInfectionToNearbyTile(i, j, BiomeConversionID.Crimson);
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