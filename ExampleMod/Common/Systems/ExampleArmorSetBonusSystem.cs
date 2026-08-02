using ExampleMod.Common.Players;
using ExampleMod.Content.Items.Armor;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// In tModLoader 1.4.5, armor set bonuses can be registered through ArmorSetBonus.
	// This lets the game show partial set bonus progress in tooltips and keeps set matching logic in one place.
	public class ExampleArmorSetBonusSystem : ModSystem
	{
		public override void PostSetupContent() {
			ArmorSetBonus.Create(ApplyExampleHelmetSetBonus, Mod.GetLocalization("ArmorSetBonus.ExampleHelmet").Key, ArmorSetBonus.PartType.Head)
				.Set(
					ModContent.ItemType<ExampleHelmet>(),
					ModContent.ItemType<ExampleBreastplate>(),
					ModContent.ItemType<ExampleLeggings>()
				)
				.Add();

			ArmorSetBonus.Create(ApplyExampleHoodSetBonus, Mod.GetLocalization("ArmorSetBonus.ExampleHood").Key, ArmorSetBonus.PartType.Head)
				.Set(
					ModContent.ItemType<ExampleHood>(),
					ModContent.ItemType<ExampleBreastplate>(),
					ModContent.ItemType<ExampleLeggings>()
				)
				.Add();
		}

		private static void ApplyExampleHelmetSetBonus(Player player) {
			player.GetDamage(DamageClass.Generic) += ExampleHelmet.AdditiveGenericDamageBonus / 100f; // Increase dealt damage for all weapon classes by 20%.
		}

		private static void ApplyExampleHoodSetBonus(Player player) {
			player.manaCost -= ExampleHood.ManaCostReductionPercent / 100f; // Reduces mana cost by 10%.
			player.GetModPlayer<ExampleArmorSetBonusPlayer>().ExampleSetHood = true;
		}
	}
}
