using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleBasicFish : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true; // All vanilla fish can be placed in a weapon rack.
			ItemID.Sets.IsBasicFish[Type] = true; // Denotes this item as a fish for inventory sorting. Use IsQuestFish instead for quest fish.
			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults() {
			Item.width = 34;
			Item.height = 34;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 5);
		}

		public override void AddRecipes() {
			// Here is an example of creating a recipe for a different item and using this item as an ingredient.
			// See ExampleRecipes.cs for more information about recipes.
			Recipe.Create(ItemID.BlackenedFish)
				.AddIngredient(Type) // Type is the item ID of the this item. In this case, it is the same thing as doing .AddIngredient(ModContent.ItemType<ExampleBasicFish>()) or .AddIngredient<ExampleBasicFish>()
				.AddTile(TileID.Campfire)
				.Register();
			Recipe.Create(ItemID.CookedFish)
				.AddIngredient(Type)
				.AddTile(TileID.CookingPots)
				.Register();
		}

		// The catch location is defined in ExampleMod/Common/Players/ExampleFishingPlayer
	}
}
