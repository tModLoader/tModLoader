using ExampleMod.Content.Items;
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
			TileID.Sets.HallowBiome[Type] = true;
			TileID.Sets.HallowBiomeSight[Type] = true;
			TileID.Sets.HallowCountCollection[Type] = true;
			DustType = DustID.Pearlsand;
			AddMapEntry(new Color(157, 76, 152));

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
		public override bool Convert(int i, int j, int conversionType) {
			//Purification powder doesn't convert hallow tiles back into pure versions
			if (conversionType == BiomeConversionID.PurificationPowder)
				return false;
			//Yellow (desert) solution also converts evil/hallowed tiles back into purity, so don't forget that check!
			else if(conversionType == BiomeConversionID.Purity || conversionType == BiomeConversionID.Sand) {
				WorldGen.ConvertTile(i, j, TileID.DesertFossil);
				return false;
			}
			else if (conversionType == BiomeConversionID.Corruption) {
				WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptFossilTile>());
				return false;
			}
			else if(conversionType == BiomeConversionID.Crimson) {
				WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonFossilTile>());
				return false;
			}
			//This example showcases how to make hallow and evil biome conversion work, but you can extend this code to work for the other vanilla solutions.
			//Just don't forget to register the conversion type in SetStaticDefaults if you want a vanilla tile to turn into your new modded tile.
			//else if (conversionType == BiomeConversionID.Snow) {
			//	WorldGen.ConvertTile(i, j, TileID.Slush);
			//}

			return true;
		}
	}

	public class CorruptFossilTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Corrupt[Type] = true;
			TileID.Sets.CorruptBiome[Type] = true;
			TileID.Sets.CorruptBiomeSight[Type] = true;
			TileID.Sets.CorruptCountCollection[Type] = true;
			DustType = DustID.Corruption;
			AddMapEntry(new Color(65, 48, 99));

			//We do the same thing as the hallow one. For this one, lets show how it would look to register multiple conversions from the same vanilla tile
			TileLoader.RegisterConversion(TileID.DesertFossil, BiomeConversionID.Corruption, ConvertToEvilBiome);
			TileLoader.RegisterConversion(TileID.DesertFossil, BiomeConversionID.Crimson, ConvertToEvilBiome);
		}

		public bool ConvertToEvilBiome(int i, int j, int type, int conversionType) {
			//Since we registered two conversions with this same method, we can use conversiontype to determine which one is happening
			if (conversionType == BiomeConversionID.Corruption)
				WorldGen.ConvertTile(i, j, Type);
			else
				WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonFossilTile>());
			return false;
		}

		public override bool Convert(int i, int j, int conversionType) {
			if (conversionType == BiomeConversionID.Purity || conversionType == BiomeConversionID.Sand || conversionType == BiomeConversionID.PurificationPowder) {
				WorldGen.ConvertTile(i, j, TileID.DesertFossil);
				return false;
			}
			else if (conversionType == BiomeConversionID.Hallow) {
				WorldGen.ConvertTile(i, j, ModContent.TileType<HallowedFossilTile>());
				return false;
			}
			else if (conversionType == BiomeConversionID.Crimson) {
				WorldGen.ConvertTile(i, j, ModContent.TileType<CrimsonFossilTile>());
				return false;
			}

			return true;
		}
	}

	public class CrimsonFossilTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			TileID.Sets.Crimson[Type] = true;
			TileID.Sets.CrimsonBiome[Type] = true;
			TileID.Sets.CrimsonBiomeSight[Type] = true;
			TileID.Sets.CrimsonCountCollection[Type] = true;
			DustType = DustID.Crimstone;
			AddMapEntry(new Color(112, 33, 32));
		}

		public override bool Convert(int i, int j, int conversionType) {
			if (conversionType == BiomeConversionID.Purity || conversionType == BiomeConversionID.Sand || conversionType == BiomeConversionID.PurificationPowder) {
				WorldGen.ConvertTile(i, j, TileID.DesertFossil);
				return false;
			}
			else if (conversionType == BiomeConversionID.Corruption) {
				WorldGen.ConvertTile(i, j, ModContent.TileType<CorruptFossilTile>());
				return false;
			}
			else if (conversionType == BiomeConversionID.Hallow) {
				WorldGen.ConvertTile(i, j, ModContent.TileType<HallowedFossilTile>());
				return false;
			}

			return true;
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