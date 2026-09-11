using ExampleMod.Common.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Head value here will result in TML expecting a X_Head.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Head)]
	public class ExampleHood : ModItem
	{
		public static readonly int ManaCostReductionPercent = 10;

		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults() {
			// This is the armor set bonus tooltip:
			//   Double tap or hold DOWN/UP to toggle various armor shadow effects
			//   10% reduced mana cost
			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(ManaCostReductionPercent);

			// Registers an armor set. Note that ExampleHelmet also registers a similar armor set. The PartType parameter is needed in this case because the 2 sets have different tooltips.
			AddArmorSet<ExampleHood, ExampleBreastplate, ExampleLeggings>(SetBonusText, Terraria.DataStructures.ArmorSetBonus.PartType.Head);
		}

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 4; // The amount of defense the item will give when equipped
		}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		public override void UpdateArmorSet(Player player, ArmorSetBonus armorSetBonus) {
			player.manaCost -= ManaCostReductionPercent / 100f; // Reduces mana cost by 10%
			player.GetModPlayer<ExampleArmorSetBonusPlayer>().ExampleSetHood = true;
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
