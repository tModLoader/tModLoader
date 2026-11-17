using ExampleMod.Common.Players;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	/// <summary>
	/// This class demonstrates how to modify existing armor set bonuses.
	/// <para/> In this example, we add additional effects to the Magic Hat armor set, increasing "example resource" by 50.
	/// <para/> To do this, we adjust both the set description and the actual effect of the set bonus. 
	/// </summary>
	public class ArmorSetTweaks : GlobalItem
	{
		public static readonly int ResourceBoost = 50;

		public override void ModifyArmorSets() {
			// To add to an existing armor set description, we will append to the existing description.
			// This localization key is "{0}" followed by a newline and then our additional description. The {0} will be replaced with the existing description.
			LocalizedText MagicHatAdditionalEffectText = Language.GetOrRegister("Mods.ExampleMod.ArmorSetTweaks.MagicHatAdditionalEffectText");

			// We loop through all armor set bonuses and find the ones with the identifier "MagicHat", which is the identifier for the Magic Hat armor set bonus. There are multiple ArmorSetBonus entries for the Magic Hat armor set, one for each of the different body pieces (robes) that can be used to complete the set. We will want to affect all of them.
			foreach (var armorSetBonus in ArmorSetBonuses.All.Where(x => x.Identifier == "MagicHat")) {
				armorSetBonus.Description = MagicHatAdditionalEffectText.WithFormatArgs(armorSetBonus.Description, ResourceBoost);
			}
		}

		public override void UpdateArmorSet(Player player, ArmorSetBonus armorSetBonus) {
			// If an armor set bonus is active, we can check its identifier to see if it is the "Magic Hat" armor set bonus. If it is, we can apply our additional effects.
			if (armorSetBonus?.Identifier == "MagicHat") {
				var modPlayer = player.GetModPlayer<ExampleResourcePlayer>();
				modPlayer.exampleResourceMax2 += ResourceBoost; // Add 50 to the exampleResourceMax2
			}
		}
	}
}
