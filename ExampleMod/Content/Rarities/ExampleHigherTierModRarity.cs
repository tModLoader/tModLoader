using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Rarities
{
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
