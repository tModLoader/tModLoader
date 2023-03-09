using ExampleMod.Content;
using ExampleMod.Content.Items.Accessories;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	/// <summary>
	/// ModPlayer class coupled with <seealso cref="ExampleInfoDisplay"/> and <seealso cref="ExampleInfoAccessory"/> to show off how to properly add a
	/// new info accessory (such as a Radar, Lifeform Analyzer, etc.)
	/// </summary>
	public class ExampleInfoDisplayPlayer : ModPlayer
	{
		// Flag checking when information display should be activated
		public bool showMinionCount;

		// Make sure to use the right Reset hook. This one is unique, as it will still be
		// called when the game is paused; this allows for info accessories to keep updating properly.
		public override void ResetInfoAccessories() {
			showMinionCount = false;
		}

		// If we have another nearby player on our team, we want to get their info accessories working on us,
		// just like in vanilla. This is what this hook is for.
		public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer) {
			if (otherPlayer.GetModPlayer<ExampleInfoDisplayPlayer>().showMinionCount) {
				showMinionCount = true;
			}
		}
	}
}
