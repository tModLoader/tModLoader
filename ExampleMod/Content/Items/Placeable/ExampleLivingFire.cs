using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleLivingFire : ModItem
	{
		// We will be using this color several times.
		// Defining it like this means we only need to change this Vector3 if we want to change the color of everything.
		public static Vector3 LightColor = new Vector3(0.7f, 0.8f, 0.8f);

		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true; // This set stops the item from burning in lava even with White rarity.
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleLivingFireTile>());
			Item.width = 12;
			Item.height = 12;
		}

		public override void PostUpdate() {
			// Add some lighting when the item is dropped in the world.
			// Curiously, only the regular Living Fire Block creates light.
			Lighting.AddLight(Item.Center, LightColor);
		}

		public override void AddRecipes() {
			CreateRecipe(20)
				.AddIngredient(ItemID.LivingFireBlock, 20)
				.AddIngredient<ExampleItem>()
				.AddTile(TileID.CrystalBall)
				.SortAfterFirstRecipesOf(ItemID.LivingUltrabrightFireBlock) // places the recipe right after vanilla fire block recipes
				.Register();
		}
	}
}