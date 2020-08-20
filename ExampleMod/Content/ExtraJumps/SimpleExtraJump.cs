using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.ExtraJumps
{
	/// <summary>
	/// This is a simple example of a <see cref="ModExtraJump"/>. Enable via player.EnableDoubleJump<SimpleExtraJump>();
	/// </summary>
	public class SimpleExtraJump : ModExtraJump
	{
		public override void Jump(ref float jumpHeight, ref bool playSound) {
			// Twice the normal jump height
			jumpHeight = 2f;
		}

		public override void PerformingJump() {
			// This is the dust that spawns while we keep the jump key pressed
			for (int i = 0; i < 5; i++) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.position.X, player.Center.Y + 6), player.width, 0, DustID.Dirt);
				dust.noLight = true;
			}
		}

		public override void HorizontalJumpSpeed(ref float runAccelerationMult, ref float maxRunSpeedMult) {
			// Here we increase horizontal movement speed while jumping, similar to most double jumps
			runAccelerationMult = 2f;
			maxRunSpeedMult = 1.5f;
		}
	}

	public class SimpleExtraJumpBeforeSandstorm : SimpleExtraJump
	{
		public override VanillaExtraJump JumpAfter => VanillaExtraJump.Mounts;

		public override void Jump(ref float jumpHeight, ref bool playSound) {
			base.Jump(ref jumpHeight, ref playSound);
			CombatText.NewText(player.Hitbox, Color.White, "before sandstorm");
		}
	}

	public class SimpleExtraJumpAfterSandstorm : SimpleExtraJump
	{
		public override VanillaExtraJump JumpAfter => VanillaExtraJump.Sandstorm;

		public override void Jump(ref float jumpHeight, ref bool playSound) {
			base.Jump(ref jumpHeight, ref playSound);
			CombatText.NewText(player.Hitbox, Color.White, "after sandstorm");
		}
	}
}
