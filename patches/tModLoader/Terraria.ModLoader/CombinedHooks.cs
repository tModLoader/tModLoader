using System;

namespace Terraria.ModLoader
{
	public static class CombinedHooks
	{
		public static void ModifyWeaponDamage(Player player, Item item, ref float add, ref float mult) {
			ItemLoader.ModifyWeaponDamage(item, player, ref add, ref mult);
			PlayerHooks.ModifyWeaponDamage(player, item, ref add, ref mult);
		}

		[Obsolete]
		public static void GetWeaponDamage(Player player, Item item, ref int damage) {
			ItemLoader.GetWeaponDamage(item, player, ref damage);
			PlayerHooks.GetWeaponDamage(player, item, ref damage);
		}
	}
}
