using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class ExampleHelmet : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded helmet.");
		}

		public override void SetDefaults() {
			Item.width = 18; // width of the item
			Item.height = 18; // height of the item
			Item.sellPrice(gold: 1); // how many coins the item is worth
			Item.rare = ItemRarityID.Green; // the rarity of the item
			Item.defense = 5; // the amount of defense the item will give when equipped
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<ExampleBreastplate>() && legs.type == ModContent.ItemType<ExampleLeggings>();
		}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Increases dealt damage by 20%"; // this is the setbonus tooltip
			player.GetDamage(DamageClass.Generic) += 0.2f; // increase dealt damage for all weapon classes by 20%
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(30) // this item needs 30 ExampleItem's to craft
				.AddTile<Tiles.Furniture.ExampleWorkbench>() // craft the item in an ExampleWorkbench
				.Register();
		}
	}
}
