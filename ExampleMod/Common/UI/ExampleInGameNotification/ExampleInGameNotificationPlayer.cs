using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleInGameNotification
{
	// This class serves the purpose of pushing our example notification in-game.
	// See ExampleJoinWorldInGameNotification.
	public class ExampleInGameNotificationPlayer : ModPlayer
	{
		public override void OnEnterWorld() {
			// Show our on-join notification when we join the world.
			// This should only display for our player.
			if (Player.whoAmI == Main.myPlayer)
				InGameNotificationsTracker.AddNotification(new ExampleJoinWorldInGameNotification());
		}
	}
}