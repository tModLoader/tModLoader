using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Head value here will result in TML expecting a X_Head.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Head)]
	public class ExampleHood : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded hood.");
		}

		public override void SetDefaults() {
			Item.width = 18; // width of the item
			Item.height = 18; // height of the item
			Item.sellPrice(gold: 1); // how many coins the item is worth
			Item.rare = ItemRarityID.Green; // the rarity of the item
			Item.defense = 4; // the amount of defense the item will give when equipped
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<ExampleBreastplate>() && legs.type == ModContent.ItemType<ExampleLeggings>();
		}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "reduces mana cost by 10%";  // this is the setbonus tooltip
			player.manaCost -= 0.1f; // reduces mana cost by 10%
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(30) // this item needs 30 ExampleItems to craft
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // craft the item at an ExampleWorkbench
				.Register();
		}
	}
}
