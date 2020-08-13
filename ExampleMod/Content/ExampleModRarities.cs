using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Content
{
	public class ExampleModRarity : ModRarity
	{
		public override Color RarityColor => new Color(200, 215, 230);

		public override void ModifyOffsetRarity(int vanillaOffset, ref int newRarity) {
			if (vanillaOffset > 0) { // If the offset is 1 or 2 (a positive modifier).
				newRarity = ModContent.RarityType<ExampleHigherTierModRarity>(); // Make the rarity of items that have this rarity with a positive modifier the higher tier one.
			}
		}
	}

	public class ExampleHigherTierModRarity : ModRarity
	{
		public override Color RarityColor => new Color(Main.DiscoR / 2, (byte)(Main.DiscoG / 1.25f), (byte)(Main.DiscoB / 1.5f));

		public override void ModifyOffsetRarity(int vanillaOffset, ref int newRarity) {
			if (vanillaOffset < 0) { // If the offset is -1 or -2 (a negative modifier).
				newRarity = ModContent.RarityType<ExampleModRarity>(); // Make the rarity of items that have this rarity with a negative modifier the lower tier one.
			}
		}
	}
}
