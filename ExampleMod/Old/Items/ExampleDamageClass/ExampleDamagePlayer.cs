using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items.ExampleDamageClass
{
	// This class stores necessary player info for our custom damage class, such as damage multipliers, additions to knockback and crit, and our custom resource that governs the usage of the weapons of this damage class.
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

		public override void ResetEffects() {
			ResetVariables();
		}

		public override void UpdateDead() {
			ResetVariables();
		}

		private void ResetVariables() {
			exampleDamageAdd = 0f;
			exampleDamageMult = 1f;
			exampleKnockback = 0f;
			exampleCrit = 0;
		}
	}
}
