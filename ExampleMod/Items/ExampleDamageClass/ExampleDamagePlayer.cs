using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.ExampleDamageClass
{
	// This class stores necessary player info for our custom damage class, such as damage multipliers, additions to knockback and crit and our custom resource.
	public class ExampleDamagePlayer : ModPlayer
	{
		public static ExampleDamagePlayer ModPlayer(Player player) {
			return player.GetModPlayer<ExampleDamagePlayer>();
		}

		// Vanilla only really has damage multipliers in code
		// And crit and knockback is usually just added to
		// As a modder, you could make separate variables for multipliers and simple addition bonuses
		public float exampleDamageAdd;
		public float exampleDamageMult = 1f;
		public float exampleKnockback;
		public int exampleCrit;

		// Creating some variables to define the current value of our example resource as well as the maximum value.
		// maximumResource2 will act as our own "statMaxLife2" for temporary changes to the maximum.
		// Make sure to set these as floats if you want to have a resource bar for it.
		public float currentResource;
		public float maximumResource = 100;
		public float maximumResource2;
		// You can make something like this to easily refer to a combination of both, instead of adding them together every time.
		public float OverallMaximumResource { get => maximumResource + maximumResource2; }
		public float resourceRegenRate;
		internal int resourceRegenTimer = 0;

		public override void ResetEffects() {
			ResetVariables();
			UpdateResource();
		}

		public override void UpdateDead() {
			ResetVariables();
		}

		private void ResetVariables() {
			exampleDamageAdd = 0f;
			exampleDamageMult = 1f;
			exampleKnockback = 0f;
			exampleCrit = 0;
			maximumResource2 = 0f;
			resourceRegenRate = 1f;
		}

		// Lets do all our logic for the custom resource here, such as limiting it, increasing it and so on.
		private void UpdateResource() {

			// Limit the currentResource from going over the limit imposed by your maximumResource's.
			if (currentResource > OverallMaximumResource)
				currentResource = OverallMaximumResource;

			// For our resource lets make it regen slowly over time to keep it simple, let's use resourceRegenTimer to count up to whatever value we want, then increase currentResource by our resourceRegenRate.
			resourceRegenTimer++; //Increase it by 60 per second, or 1 per tick.

			// A simple timer that goes up to 3 seconds, increases the currentResource by our resourceRegenRate and then resets back to 0. Add the extra check at the end to stop it flickering in the resource bar.
			if ((resourceRegenTimer > 180 * resourceRegenRate) && currentResource < OverallMaximumResource) {
				currentResource += 1;
				resourceRegenTimer = 0;
			}
		}
	}
}
