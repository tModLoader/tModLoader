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
	public class ExampleHelmet : ModItem
	{
		public static readonly int AdditiveGenericDamageBonus = 20;

		public static LocalizedText SetBonusText { get; private set; }

		public override void SetStaticDefaults() {
			// If your head equipment should draw hair while drawn, use one of the following:
			// ArmorIDs.Head.Sets.HidesHead[Item.headSlot] = true; // Don't draw the head at all. Used by Space Creature Mask
			// ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true; // Draw hair as if a hat was covering the top. Used by Wizards Hat
			// ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true; // Draw all hair as normal. Used by Mime Mask, Sunglasses
			// ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

			SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(AdditiveGenericDamageBonus);

			// ArmorSetBonus.Create registers a set bonus for specific armor pieces. Each Set(head, body, legs) call adds a combination of pieces that grants the bonus, use 0 for a piece that isn't needed.
			// The SetBonus tooltip line is shown on every piece, even when not worn, along with how many pieces of the set are worn.
			// ExampleBreastplate and ExampleLeggings are shared with ExampleHood, which gives a different set bonus, so PartType.Head is passed to mark the head piece as the one that decides the bonus.
			// The body and legs then show "Changes with head piece" instead of this bonus.
			// See ExampleHood for the IsArmorSet and UpdateArmorSet hooks, which allow any condition but only show the tooltip while the full set is worn.
			ArmorSetBonus.Create(UpdateArmorSetBonus, SetBonusText, ArmorSetBonus.PartType.Head)
				.Set(Type, ModContent.ItemType<ExampleBreastplate>(), ModContent.ItemType<ExampleLeggings>())
				.Add();
		}

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 5; // The amount of defense the item will give when equipped
		}

		// Called every update while the full set registered in SetStaticDefaults is worn.
		private static void UpdateArmorSetBonus(Player player) {
			player.GetDamage(DamageClass.Generic) += AdditiveGenericDamageBonus / 100f; // Increase dealt damage for all weapon classes by 20%
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
