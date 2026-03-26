using ExampleMod.Content.Items;
using ExampleMod.Content.Tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

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

			// We need to register a conversion from the vanilla desert fossil wall into our modded variants, our method here automatically registers the conversion and fallback values here
			// Note: WallID.DesertFossil is unused, WallID.DesertFossilEcho is the only fossil wall that can be placed ingame
			WallLoader.RegisterSimpleConversion(WallID.DesertFossilEcho, BiomeConversionID.Hallow, Type);

			// Yellow (desert) solution should convert the infected desert fossil wall back into purity, so we do that manually
			WallLoader.RegisterConversion(Type, BiomeConversionID.Sand, WallID.DesertFossilEcho);
		}
	}

	public class CorruptFossilWall : ModWall
	{
		public override void SetStaticDefaults() {
			WallID.Sets.Corrupt[Type] = true;
			DustType = DustID.Corruption;
			AddMapEntry(new Color(65, 48, 99));

			WallLoader.RegisterSimpleConversion(WallID.DesertFossilEcho, BiomeConversionID.Corruption, Type);
			WallLoader.RegisterConversion(Type, BiomeConversionID.Sand, WallID.DesertFossilEcho);
		}
	}

	public class CrimsonFossilWall : ModWall
	{
		public override void SetStaticDefaults() {
			WallID.Sets.Crimson[Type] = true;
			DustType = DustID.Crimstone;
			AddMapEntry(new Color(112, 33, 32));

			WallLoader.RegisterSimpleConversion(WallID.DesertFossilEcho, BiomeConversionID.Crimson, Type);
			WallLoader.RegisterConversion(Type, BiomeConversionID.Sand, WallID.DesertFossilEcho);
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