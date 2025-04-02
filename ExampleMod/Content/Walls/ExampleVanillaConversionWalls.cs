using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ExampleMod.Content.Tiles;

namespace ExampleMod.Content.Walls
{
	// These three classes showcase how to create wall that act as corruption/crimson/hallow versions of vanilla walls.
	// For this example, we will be making vanilla's desert fossil walls convertible into the three spreading biomes
	public class HallowedFossilWall : ModWall
	{
		public override void SetStaticDefaults() {
			WallID.Sets.Hallow[Type] = true;
			DustType = DustID.Pearlsand;
			AddMapEntry(new Color(157, 76, 152));

			//We need to register a conversion from the vanilla desert fossil wall into our modded variants, so our custom code can be called when the game attempts to convert the vanilla wall
			//Note: WallID.DesertFossil is unused, WallID.DesertFossilEcho is the only fossil wall that can be placed ingame
			WallLoader.RegisterConversion(WallID.DesertFossilEcho, BiomeConversionID.Hallow, ConvertToHallow);
		}

		public bool ConvertToHallow(int i, int j, int type, int conversionType) {

			//This method is called whenever hallow biome conversion happens on a desert fossil wall, as per the RegisterConversion we called in SetStaticDefaults
			//We don't need to check the type or the conversionType as we only registered one conversion with this method, but the same method could be reused for multiple conversion types or walls

			//We can use the ConvertWall utility method to change the fossil wall into our hallowed fossil wall, and it'll automatically handle wall frame updates and network syncing!
			WorldGen.ConvertWall(i, j, Type);
			return false;
		}

		//This code is called when the game attempts to convert our hallowed wall into a new biome
		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Purity:
				case BiomeConversionID.Sand:
					WorldGen.ConvertWall(i, j, WallID.DesertFossilEcho);
					return;
				case BiomeConversionID.Corruption:
					WorldGen.ConvertWall(i, j, ModContent.WallType<CorruptFossilWall>());
					return;
				case BiomeConversionID.Crimson:
					WorldGen.ConvertWall(i, j, ModContent.WallType<CrimsonFossilWall>());
					return;
			}
		}
	}

	public class CorruptFossilWall : ModWall
	{
		public override void SetStaticDefaults() {
			WallID.Sets.Corrupt[Type] = true;
			DustType = DustID.Corruption;
			AddMapEntry(new Color(65, 48, 99));

			WallLoader.RegisterConversion(WallID.DesertFossilEcho, BiomeConversionID.Corruption, ConvertToCorruption);
		}

		public bool ConvertToCorruption(int i, int j, int type, int conversionType) {
			WorldGen.ConvertWall(i, j, Type);
			return false;
		}

		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Purity:
				case BiomeConversionID.Sand:
				// Eventhough this is a corrupt wall, we do not need to check for BiomeConversionID.PurificationPowder, since purification powder doesnt work on walls
					WorldGen.ConvertWall(i, j, WallID.DesertFossilEcho);
					return;
				case BiomeConversionID.Hallow:
					WorldGen.ConvertWall(i, j, ModContent.WallType<HallowedFossilWall>());
					return;
				case BiomeConversionID.Crimson:
					WorldGen.ConvertWall(i, j, ModContent.WallType<CrimsonFossilWall>());
					return;
			}
		}
	}

	public class CrimsonFossilWall : ModWall
	{
		public override void SetStaticDefaults() {
			WallID.Sets.Crimson[Type] = true;
			DustType = DustID.Crimstone;
			AddMapEntry(new Color(112, 33, 32));

			WallLoader.RegisterConversion(WallID.DesertFossilEcho, BiomeConversionID.Crimson, ConvertToCrimson);
		}

		public bool ConvertToCrimson(int i, int j, int type, int conversionType) {
			WorldGen.ConvertWall(i, j, Type);
			return false;
		}

		public override void Convert(int i, int j, int conversionType) {
			switch (conversionType) {
				case BiomeConversionID.Purity:
				case BiomeConversionID.Sand:
					WorldGen.ConvertWall(i, j, WallID.DesertFossilEcho);
					return;
				case BiomeConversionID.Hallow:
					WorldGen.ConvertWall(i, j, ModContent.WallType<HallowedFossilWall>());
					return;
				case BiomeConversionID.Corruption:
					WorldGen.ConvertWall(i, j, ModContent.WallType<CorruptFossilWall>());
					return;
			}
		}
	}

	#region Items
	internal class HallowedFossilWallItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<HallowedFossilWall>());
		}

		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<HallowedFossilTileItem>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	internal class CorruptFossilWallItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<CorruptFossilWall>());
		}

		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<CorruptFossilTileItem>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	internal class CrimsonFossilWallItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<CrimsonFossilWall>());
		}

		public override void AddRecipes() {
			CreateRecipe(4)
				.AddIngredient<CrimsonFossilTileItem>()
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
	#endregion
}