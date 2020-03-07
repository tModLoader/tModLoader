using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Accessories
{
	/// <summary>
	/// This is an example of a <see cref="ModDoubleJump"/> with inheritance to showcase increasing jumpheight and visuals, used in <see cref="ExampleDoubleJumpAccessory"/>.
	/// A simpler ModDoubleJump is <see cref="SimpleDoubleJump"/>
	/// </summary>
	public abstract class ExampleDoubleJump : ModDoubleJump
	{
		// Optional field we use in our inheritance below, to not override the same hooks again just to adjust one number.
		// It represents the jump intensity and the amount of dust we spawn
		public abstract int Power { get; }

		public override float Jump(ref bool playSound) {
			CombatText.NewText(player.getRect(), Color.Lime, "'" + Power + "!'", true);
			for (int i = 0; i < 20 * Power; i++) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.position.X, player.Center.Y), player.width, player.height / 2, DustID.Fire);
				dust.noLight = true;
				dust.velocity.Y *= Math.Sign(dust.velocity.Y); // Turns dusts' velocity around vertically so they will fly down
				dust.velocity *= 4f;
				dust.scale += Main.rand.NextFloat(0.5f, 0.8f);
			}

			// We want to play our own sound, so we set playSound to false
			playSound = false;
			Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, player.Center);

			return Power;
		}

		public override void MidJump() {
			// This is the dust that spawns while we keep the jump key pressed
			for (int i = 0; i < 2 * Power; i++) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.position.X, player.Center.Y + 6), player.width, 0, DustID.Fire);
				dust.noLight = true;
			}
		}

		public override void HorizontalJumpSpeed(ref float runAccelerationMult, ref float maxRunSpeedMult)
		{
			// Here we increase horizontal movement speed while jumping, similar to most double jumps
			runAccelerationMult = Power;
			maxRunSpeedMult = 0.5f + Power;
		}
	}

	public class ExampleDoubleJump1 : ExampleDoubleJump
	{
		public override int Power => 1;
	}

	public class ExampleDoubleJump2 : ExampleDoubleJump
	{
		public override int Power => 2;
	}

	public class ExampleDoubleJump3 : ExampleDoubleJump
	{
		public override int Power => 3;

		public override bool JumpAgain() {
			// Keep jumping if going to the left
			if (player.direction == -1) return true;
			return false;
		}
	}
}
