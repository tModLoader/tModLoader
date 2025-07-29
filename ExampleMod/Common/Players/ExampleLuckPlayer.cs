using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleLuckPlayer : ModPlayer
	{
		public override void ModifyLuck(ref float luck) { // ModifyLuck is what you'll normally use for any modded content that wants to modify luck.
			// Luck in total has a vanilla soft cap of 1. You can technically go above that value, but there's no benefit to be gained with vanilla luck calculations.
			// However, modders can use the luck value however they want, so going above 1 may be beneficial. Decimal values are still recommended, though.
			if (Main.hardMode) { // If it is currently hardmode...
				luck += 0.5f; // ...add 0.5 luck to the total luck count!
			}
			// Of course you can make luck negative as well, in which case a soft cap of -1 applies.

			// As the above code runs every time luck is calculated, and `hardMode` is accessible on both client and server, we don't need to worry about multiplayer syncing.
			// If you have some code which relies on client side calculations, you will need to sync the variables to calculate luck correctly on the server.
		}

		public override bool PreModifyLuck(ref float luck) { // PreModifyLuck is useful if you want to modify any vanilla luck values or want to prevent vanilla luck calculations from happening.
			Terraria.GameContent.Events.LanternNight.GenuineLanterns = true; // The game now thinks its a Lantern Night all the time, giving you the luck bonus.
			Player.HasGardenGnomeNearby = true; // The game now thinks there's a garden gnome nearby all the time, giving you the luck bonus.

			if (Player.ladyBugLuckTimeLeft < 0) { // If you have bad ladybug luck...
				Player.ladyBugLuckTimeLeft = 0; // ...completely cancel it out.
			}

			return true; // PreModifyLuck returns true by default, but you can also return false if you want to prevent vanilla luck calculations from happening at all.
		}
	}
}
