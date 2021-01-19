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
				Player.luckNeedsSync = true; //Always set this to true after you set the luck value directly so it properly syncs in multiplayer.
			}
			//Of course you can make luck negative as well, in which case a soft cap of -1 applies.
		}

		public override bool PreModifyLuck(ref float luck) { //PreModifyLuck is useful if you want to modify any vanilla luck values or want to prevent vanilla luck calculations from happening.
			Terraria.GameContent.Events.LanternNight.GenuineLanterns = true; //The game now thinks its a Lantern Night all the time, giving you the luck bonus.
			Player.HasGardenGnomeNearby = true; //The game now thinks there's a garden gnome nearby all the time, giving you the luck bonus.

			if (Player.ladyBugLuckTimeLeft < 0) { //If you have bad ladybug luck...
				Player.ladyBugLuckTimeLeft = 0; //...completely cancel it out.
			}

			Player.luckNeedsSync = true; //Tell multiplayer that luck needs to sync.
			return true; //PreModifyLuck returns true by default, but you can also return false if you want to prevent vanilla luck calculations from happening at all.
		}

		public override void ModifyTorchLuck(ref float positiveLuck, ref float negativeLuck) { //ModifyTorchLuck is more unique than the others as it specifically deals with luck given by torches.
			//ModifyTorchLuck is separated from the other hooks since all torches nearby contribute to the same amount of overall torch luck, which has a cap
			//In most use-cases you should add 1 to positive luck for a good luck torch, or 1 to negative luck for a bad luck torch.
			//You can also add a smaller amount (eg 0.5) for a smaller postive/negative luck impact. Remember that the overall torch luck is decided by every torch around the player, so it may be wise to have a smaller amount of luck impact.
			if (ExampleTorchNearby) { //If an ExampleTorch is nearby (set in ExampleTorch's file)...
				positiveLuck += 1f; //...the torch has a positive influence on overall torch luck.
									//Postive luck is capped at 1, any value higher won't make any difference.
									//The influence positive torch luck can have overall is 0.5 (if positive luck is any number less than 1) or 1 (if positive luck is greater than or equal to 1)
									//These result in a 0.1 or 0.2 overall luck bonus respectively
			}
			else {
				negativeLuck += 0.1f; //...otherwise it has a small negative influence on overall torch luck.
									  //On the flip side, negative luck is capped a 2, any value higher won't make any difference.
									  //The influence negative torch luck can have overall is -1.5 (if negative luck is greater than or equal to 2), -1.0 (if negative luck is greater than or equal to 1 and lower than 2), and -0.5 (if negative luck is any number less than 1)
									  //These result in a -0.3, -0.2, or -0.1 overall luck penalty respectively
			}

			//TODO: Make ExampleTorch require being in ExampleBiome when that gets implemented.
		}
	}
}
