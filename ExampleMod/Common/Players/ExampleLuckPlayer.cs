using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleLuckPlayer : ModPlayer
	{
		public bool ExampleTorchNearby;
		public override void ModifyLuck(ref float luck) { //ModifyLuck is what you'll normally use for any modded content that wants to modify luck.
			//Luck in total has a vanilla soft cap of 1. You can technically go above that value, but theres no benefit to be gained with vanilla luck calculations.
			//However, modders can use the luck value however they want, so going above 1 may be beneficial. Decimal values are still recommended, though.
			if (Main.hardMode) { //If it is currently hardmode...
				luck += 0.5f; //...add 0.5 luck to the total luck count!
			}
			//Of course you can make luck negative as well, in which case a soft cap of -1 applies.
		}

		public override bool PreModifyLuck(ref float luck) { //PreModifyLuck is useful if you want to modify any vanilla luck values or want to prevent vanilla luck calculations from happening.
			Terraria.GameContent.Events.LanternNight.GenuineLanterns = true; //The game now thinks its a Lantern Night all the time, giving you the luck bonus.
			player.HasGardenGnomeNearby = true; //The game now thinks there's a garden gnome nearby all the time, giving you the luck bonus.

			if (player.ladyBugLuckTimeLeft < 0) { //If you have bad ladybug luck...
				player.ladyBugLuckTimeLeft = 0; //...completely cancel it out.
			}
				
			return true; //PreModifyLuck returns true by default, but you can also return false if you want to prevent vanilla luck calculations from happening at all.
		}

		public override void ModifyTorchLuck(ref float positiveLuck, ref float negativeLuck) { //ModifyTorchLuck is more unique than the others as it specifically deals with luck given by torches.
			//ModifyTorchLuck is separated from the other hooks since all torches nearby contribute to the same amount of overall torch luck, which has a cap.
			if (ExampleTorchNearby) { //If an ExampleTorch is nearby (set in ExampleTorch's file)...
				positiveLuck += 1f; //...the torch has a positive influence on overall torch luck.
			}
			else {
				negativeLuck += 0.1f; //...otherwise it has a small negative influence on overall torch luck.
			}
			//TODO: Make ExampleTorch require being in ExampleBiome when that gets implemented.
		}
	}
}
