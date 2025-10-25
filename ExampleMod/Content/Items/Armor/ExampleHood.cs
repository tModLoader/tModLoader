using ExampleMod.Common.Players;
using Terraria;
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
			// We are passing in "{0}" into WithFormatArgs to replace "{0}" with itself because we do the final formatting for this LocalizedText in UpdateArmorSet itself according to the players current ReversedUpDownArmorSetBonuses setting.
			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs("{0}", ManaCostReductionPercent);
		}

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 4; // The amount of defense the item will give when equipped
		}

		// IsArmorSet determines what armor pieces are needed for the setbonus to take effect
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<ExampleBreastplate>() && legs.type == ModContent.ItemType<ExampleLeggings>();
		}

		// UpdateArmorSet allows you to give set bonuses to the armor.
		public override void UpdateArmorSet(Player player) {
			// This is the setbonus tooltip:
			//   Double tap or hold DOWN/UP to toggle various armor shadow effects
			//   10% reduced mana cost
			player.setBonus = SetBonusText.Format(Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN"));
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
