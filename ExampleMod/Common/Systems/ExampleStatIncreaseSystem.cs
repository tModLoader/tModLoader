using ExampleMod.Common.Players;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// This class showcases an example of modifying how many mana stars are drawn
	public class ExampleStatIncreaseSystem : ModSystem
	{
		public override void ModifyStatSnapshot(Player player, ref PlayerStatsSnapshot snapshot) {
			// Due to one Example Mana Crystal increasing the player's maximum mana by only 10, some logic should be used to help make the increases seem fluid
			int manaCrystals = player.GetModPlayer<ExampleStatIncreasePlayer>().exampleManaCrystals;

			if (manaCrystals <= 0) {
				// Nothing to do
				return;
			}

			// Every odd number should display an extra star/bar compared to the number before it
			int extraStarsToDraw = manaCrystals % 2 == 1 ? 1 : 0;

			// NOTE: PlayerStatsSnapshot.AmountOfLifeHearts and PlayerStatsSnapshot.AmountOfManaStars are automatically capped to be below 20.
			//       However, this is not the case for being above 0.
			//       A value of <= 0 is treated as "no resources" and causes the rendering of the resource set (mana stars/bars in this case) to be skipped.
			snapshot.AmountOfManaStars += extraStarsToDraw;
		}
	}
}
