using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Body value here will result in TML expecting X_Arms.png, X_Body.png and X_FemaleBody.png sprite-sheet files to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Body)]
	public class ExampleBreastplate : ModItem
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Example Breastplate");
			Tooltip.SetDefault("This is a modded body armor."
				+ "\nImmunity to 'On Fire!'"
				+ "\n+20 max mana and +1 max minions");
		}

		public override void SetDefaults() {
			Item.width = 18; // width of the item
			Item.height = 18; // height of the item
			Item.sellPrice(gold: 1); // how many coins the item is worth
			Item.rare = ItemRarityID.Green; // the rarity of the item
			Item.defense = 6; // the amount of defense the item will give when equipped
		}

		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.OnFire] = true; // make the player immune to Fire
			player.statManaMax2 += 20; // increase how many mana points the player can have by 20
			player.maxMinions++; // increase how many minions the player can have by one
		}

		public override void AddRecipes() {
			CreateRecipe().AddIngredient<ExampleItem>(60) // this item needs 60 ExampleItems to craft
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // craft the item at an ExampleWorkbench
				.Register();
		}
	}
}
