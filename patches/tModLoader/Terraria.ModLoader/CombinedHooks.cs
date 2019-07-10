using System;

namespace Terraria.ModLoader
{
	public static class CombinedHooks
	{
		public static void ModifyWeaponDamage(Player player, Item item, ref float add, ref float mult, ref float flat) {
			ItemLoader.ModifyWeaponDamage(item, player, ref add, ref mult, ref flat);
			PlayerHooks.ModifyWeaponDamage(player, item, ref add, ref mult, ref flat);
		}

		[Obsolete]
		public static void GetWeaponDamage(Player player, Item item, ref int damage) {
			ItemLoader.GetWeaponDamage(item, player, ref damage);
			PlayerHooks.GetWeaponDamage(player, item, ref damage);
		}

		public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult) {
			ItemLoader.ModifyManaCost(item, player, ref reduce, ref mult);
			PlayerHooks.ModifyManaCost(player, item, ref reduce, ref mult);
		}

		public static void OnConsumeMana(Player player, Item item, int manaConsumed) {
			ItemLoader.OnConsumeMana(item, player, manaConsumed);
			PlayerHooks.OnConsumeMana(player, item, manaConsumed);
		}

		public static void OnMissingMana(Player player, Item item, int neededMana) {
			ItemLoader.OnMissingMana(item, player, neededMana);
			PlayerHooks.OnMissingMana(player, item, neededMana);
		}
	}
}
