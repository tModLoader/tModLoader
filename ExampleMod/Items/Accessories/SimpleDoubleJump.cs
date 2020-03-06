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
			return 2f; // Twice the normal jump height
		}

		public override void MidJump() {
			for (int i = 0; i < 10; i++) {
				Dust dust = Dust.NewDustDirect(new Vector2(player.position.X, player.Center.Y + 6), player.width, 0, DustID.Dirt);
				dust.noLight = true;
			}
		}
	}
}
