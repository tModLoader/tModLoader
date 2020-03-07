using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Accessories
{
	/// <summary>
	/// This is a simple example of a <see cref="ModDoubleJump"/>. Enable via player.EnableDoubleJump<SimpleDoubleJump>();
	/// </summary>
	public class SimpleDoubleJump : ModDoubleJump
	{
		public override float Jump(ref bool playSound) {
			// Twice the normal jump height
			return 2f;
		}

		public override void MidJump() {
			// This is the dust that spawns while we keep the jump key pressed
			for (int i = 0; i < 10; i++) {
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
}
