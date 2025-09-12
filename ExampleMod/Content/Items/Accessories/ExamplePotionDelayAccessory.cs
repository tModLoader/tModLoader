using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	public class ExamplePotionDelayAccessory : ModItem
	{
		// By declaring these here, changing the values will alter the effect, and the tooltip
		public static readonly int FlatDelayDecrease = 10;
		public static readonly int MultiplicativeDelayBonus = 10;

		// Insert the modifier values into the tooltip localization. More info on this approach can be found on the wiki: https://github.com/tModLoader/tModLoader/wiki/Localization#binding-values-to-localizations
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FlatDelayDecrease, MultiplicativeDelayBonus);

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// The effects are applied in ExampleStatBonusAccessoryPlayer below.
			player.GetModPlayer<ExamplePotionDelayAccessoryPlayer>().examplePotionDelayAccessory = true;
		}
	}

	// Some movement effects are not suitable to be modified in ModItem.UpdateAccessory due to how the math is done.
	// ModPlayer.PostUpdateRunSpeeds is suitable for these modifications.
	public class ExamplePotionDelayAccessoryPlayer : ModPlayer
	{
		public bool examplePotionDelayAccessory = false;

		public override void ResetEffects() {
			examplePotionDelayAccessory = false;
		}

		public override void ModifyGlobalPotionDelay(out StatModifier potionDelay) {
			base.ModifyGlobalPotionDelay(out potionDelay);

			if (!examplePotionDelayAccessory) {
				return;
			}

			// You can learn more about StatModifiers by referring to ExampleStatBonusAccessory.
			// Note that since we want to apply an additive decrease, we use `-=` to subtract the total
			// instead of `+=` to increase. You can also use `+=` with a negative value.
			potionDelay.Flat -= ExamplePotionDelayAccessory.FlatDelayDecrease;
			potionDelay *= 1 + ExamplePotionDelayAccessory.MultiplicativeDelayBonus / 100f;
		}
	}
}