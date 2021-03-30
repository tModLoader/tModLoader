using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class ExampleLeggings : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded leg armor."
				+ "\n5% increased movement speed");
		}

		public override void SetDefaults() {
			Item.width = 18; // width of the item
			Item.height = 18; // height of the item
			Item.sellPrice(gold: 1); // how many coins the item is worth
			Item.rare = ItemRarityID.Green; // the rarity of the item
			Item.defense = 5; // the amount of defense the item will give when equipped
		}

		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.05f; // increase the movement speed of the player
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(45) // this item needs 45 ExampleItem's to craft
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // craft the item in an ExampleWorkbench
				.Register();
		}
	}
}
