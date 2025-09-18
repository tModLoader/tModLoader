using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	// This example accessory adjusts healing potion sickness/delay/cooldown, similar to the Philosopher's Stone.
	public class ExamplePotionDelayAccessory : ModItem
	{
		// By declaring these here, changing the values will alter the effect, and the tooltip
		public static readonly int FlatDelayDecrease = 10;
		public static readonly int MultiplicativeDelayDecrease = 15;

		// Insert the modifier values into the tooltip localization. More info on this approach can be found on the wiki: https://github.com/tModLoader/tModLoader/wiki/Localization#binding-values-to-localizations
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FlatDelayDecrease, MultiplicativeDelayDecrease);

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// You can learn more about StatModifiers by referring to ExampleStatBonusAccessory.

			// Note that since we want to apply an additive decrease, we use `-=` to subtract the total
			// instead of `+=` to increase. You can also use `+=` with a negative value.
			// Decreases the final potion delay after multipliers. Adjusting PotionDelayModifier.Base would adjust the delay before multipliers
			// Multiply by 60 to convert ticks to seconds.
			player.PotionDelayModifier.Flat -= FlatDelayDecrease * 60;

			// Decreases the potion delay multiplicitively. For reference, Philosopher's Stone does player.PotionDelay *= 0.75f to reduce it by 25%.
			player.PotionDelayModifier *= 1 - MultiplicativeDelayDecrease / 100f; // Reduce delay by 15%
		}
	}
}
