using ExampleMod.Common.Players;
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
		public static readonly int ManaCostReductionPercent = 10;

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 4; // The amount of defense the item will give when equipped
		}

		// This matching logic is still used by the default IsVanitySet implementation so ArmorSetShadows can run when the full set is visible.
		// The actual 1.4.5 armor set bonus effect is registered in ExampleArmorSetBonusSystem through ArmorSetBonus.Create.
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<ExampleBreastplate>() && legs.type == ModContent.ItemType<ExampleLeggings>();
		}

		public override void ArmorSetShadows(Player player) {
			var exampleArmorSetBonusPlayer = player.GetModPlayer<ExampleArmorSetBonusPlayer>();
			if (exampleArmorSetBonusPlayer.ShadowStyle == 1) {
				player.armorEffectDrawShadow = true;
			}
			else if (exampleArmorSetBonusPlayer.ShadowStyle == 2) {
				player.armorEffectDrawOutlines = true;
			}
			else if (exampleArmorSetBonusPlayer.ShadowStyle == 3) {
				player.armorEffectDrawOutlinesForbidden = true;
			}
			else if (exampleArmorSetBonusPlayer.ShadowStyle == 4) {
				exampleArmorSetBonusPlayer.CustomShadow = true;
			}
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
