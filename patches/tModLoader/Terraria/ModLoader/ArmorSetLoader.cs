using System;
using System.Collections.Generic;

namespace Terraria.ModLoader
{
	public static class ArmorSetLoader
	{
		public static List<ArmorSet> ArmorSets { get; set; } = new List<ArmorSet>();

		public static void RegisterArmorSet(ArmorSet armorSet) => ArmorSets.Add(armorSet);

		// TO-DO: GlobalArmorSet?

		/// <summary>
		/// Checks against every defined armor set, and applies the first set bonus found.<br/>
		/// Also checks against and invokes vanity set effects, if applicable.<br/>
		/// </summary>
		/// <param name="player">
		/// The player to check against and apply defined armor sets for.<br/>
		/// </param>
		/// <returns>
		/// Whether or not a defined armor set was found and applied.<br/>
		/// </returns>
		public static bool CheckDefinedArmorSets(Player player) {
			foreach (ArmorSet set in ArmorSets) {
				if (set.ActiveVanity(player))
					set.ApplyVanityEffects(player);

				if (set.ActiveFunctional(player)) {
					player.setBonus = set.SetBonusDescription(ref player.setBonusColor);
					set.ApplyFunctionalEffects(player);
					return true;
				}
			}

			return false;
		}
	}
}
