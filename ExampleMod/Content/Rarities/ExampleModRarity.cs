using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Content.Rarities
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
}
