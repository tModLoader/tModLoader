using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ExampleMod.Common.Players
{
	public class ExampleResourcePlayer : ModPlayer
	{
		// Here we create a custom resource, similar to mana or health.
		// Creating some variables to define the current value of our example resource as well as the current maximum value. We also include a temporary max value, as well as some variables to handle the natural regeneration of this resource.
		public int exampleResourceCurrent; // Current value of our example resource
		public const int DefaultExampleResourceMax = 100; // Default maximum value of example resource
		public int exampleResourceMax; // Buffer variable that is used to reset maximum resource to default value in ResetDefaults().
		public int exampleResourceMax2; // Maximum amount of our example resource. We will change that variable to increase maximum amount of our resource
		public float exampleResourceRegenRate; // By changing that variable we can increase/decrease regeneration rate of our resource
		internal int exampleResourceRegenTimer = 0; // A variable that is required for our timer
		public static readonly Color HealExampleResource = new(187, 91, 201); // We can use this for CombatText, if you create an item that replenishes exampleResourceCurrent.

		// In order to make the Example Resource example straightforward, several things have been left out that would be needed for a fully functional resource similar to mana and health. 
		// Here are additional things you might need to implement if you intend to make a custom resource:
		// - Multiplayer Syncing: The current example doesn't require MP code, but pretty much any additional functionality will require this. ModPlayer.SendClientChanges and CopyClientState will be necessary, as well as SyncPlayer if you allow the user to increase exampleResourceMax.
		// - Save/Load permanent changes to max resource: You'll need to implement Save/Load to remember increases to your exampleResourceMax cap.
		// - Resouce replenishment item: Use GlobalNPC.OnKill to drop the item. ModItem.OnPickup and ModItem.ItemSpace will allow it to behave like Mana Star or Heart. Use code similar to Player.HealEffect to spawn (and sync) a colored number suitable to your resource.

		public override void Initialize() {
			exampleResourceMax = DefaultExampleResourceMax;
		}

		public override void ResetEffects() {
			ResetVariables();
		}

		public override void UpdateDead() {
			ResetVariables();
		}

		// We need this to ensure that regeneration rate and maximum amount are reset to default values after increasing when conditions are no longer satisfied (e.g. we unequip an accessory that increaces our recource)
		private void ResetVariables() {
			exampleResourceRegenRate = 1f;
			exampleResourceMax2 = exampleResourceMax;
		}

		public override void PostUpdateMiscEffects() {
			UpdateResource();
		}

		public override void PostUpdate() {
			CapResourceGodMode();
		}

		// Lets do all our logic for the custom resource here, such as limiting it, increasing it and so on.
		private void UpdateResource() {
			// For our resource lets make it regen slowly over time to keep it simple, let's use exampleResourceRegenTimer to count up to whatever value we want, then increase currentResource.
			exampleResourceRegenTimer++; // Increase it by 60 per second, or 1 per tick.

			// A simple timer that goes up to 1 second, increases the exampleResourceCurrent by 1 and then resets back to 0.
			if (exampleResourceRegenTimer > 60 / exampleResourceRegenRate) {
				exampleResourceCurrent += 1;
				exampleResourceRegenTimer = 0;
			}

			// Limit exampleResourceCurrent from going over the limit imposed by exampleResourceMax.
			exampleResourceCurrent = Utils.Clamp(exampleResourceCurrent, 0, exampleResourceMax2);
		}

		private void CapResourceGodMode() {
			if (Main.myPlayer == Player.whoAmI && Player.creativeGodMode) {
				exampleResourceCurrent = exampleResourceMax2;
			}
		}
	}
}
